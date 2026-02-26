using GeniView.Cloud.Common.Queue;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using MQTTnet.Client;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static GeniView.Cloud.Common.DataDefine;

namespace GeniView.Cloud.Common
{
    public class MQTTMsgParser
    {
        private bool showDebugMsg = false;

        private static Logger _logger = LogManager.GetCurrentClassLogger();


        public Dictionary<string, Battery> _batteriesDic = new Dictionary<string, Battery>();
        public Dictionary<string, Device> _devicesDic = new Dictionary<string, Device>();
        public Dictionary<string, List<DeviceEvent>> _eventDeviceDic = new Dictionary<string, List<DeviceEvent>>();


        public List<Battery> Battery 
        {
            get 
            {
                List<Battery> result = new List<Battery>();

                foreach (var item in _batteriesDic)
                {
                    var battery = item.Value;
                    result.Add(battery);

                }

                return result;
            }
        }

        public List<Device> Device
        {
            get
            {
                List<Device> result = new List<Device>();

                foreach (var item in _devicesDic)
                {
                    var device = item.Value;
                    result.Add(device);

                }
                return result;
            }
        }

        public MQTTMsgParser()
        {

        }

        public void HandleMessage(CancellationToken ct)
        {
            var logRateResult = new ConcurrentBag<CommandResult>();
            var otaResult = new ConcurrentBag<CommandResult>();
            var ntpResult = new ConcurrentBag<CommandResult>();

            var msgData = Global._queueHelp.DequeueWhile(ct);

            var batteryLogs = msgData.Where(d => MQTTTopic.BatteryLogRegex.IsMatch(d.Topic) == true).ToList();
            handleBatteryLog(batteryLogs);

            var resultmsg = msgData.Where(d => MQTTTopic.ResultRegex.IsMatch(d.Topic) == true).ToList();

            //Parser from payload cmd type
            foreach (var msg in resultmsg)
            {
                var payload = JsonConvert.DeserializeObject<CommandResult>(msg.Payload);

                switch (payload.Cmd)
                {
                    case "LogRateResult":
                        logRateResult.Add(payload);
                        break;

                    case "OTAResult":
                        otaResult.Add(payload);
                        break;

                    case "NTPResult":
                        ntpResult.Add(payload);
                        break;

                    default:
                        break;  
                }

            }

            handleLogRateResult("LogRateResult",logRateResult);
            handleOTAResult("OTAResult",otaResult);
            handleNTPResult("NTPResult", ntpResult);

        }

        public void handleBatteryLog(List<MQTTData> mQTTDatalist)
        {
            foreach (var data in mQTTDatalist)
            {
                // now deviceID is equal to SerialNumberCode of Battery
                string deviceId = data.Topic.Substring("battery/log/".Length);
                Global.DebugPrintf($"handleBatteryLog Log: ID={deviceId}, Payload: {data.Payload}", showDebugMsg);
                var battryLogs = JsonConvert.DeserializeObject<List<G3BatteryDockLog>>(data.Payload);

                (var device, var battery,var deviceEvents) = G3BatteryDataRepository.transfer(deviceId, battryLogs);

                //Device
                if (_devicesDic.ContainsKey(device.SerialNumber) == false)
                {
                    _devicesDic.Add(device.SerialNumber, device);
                }
                else
                {
                    // add the tranfer log to exist device.
                    foreach(var log in device.InternalDeviceLogCollection)
                    {
                        _devicesDic[device.SerialNumber].InternalDeviceLogCollection.Add(log);
                    }

                    foreach (var log in device.AgentDeviceLogCollection)
                    {
                        _devicesDic[device.SerialNumber].AgentDeviceLogCollection.Add(log);
                    }
                }

                //Battery
                if (_batteriesDic.ContainsKey(battery.SerialNumber) == false)
                {
                    _batteriesDic.Add(battery.SerialNumber, battery);
                }
                else
                {
                    // add the tranfer log to exist battery.
                    foreach (var log in battery.InternalBatteryLogCollection)
                    {
                        _batteriesDic[battery.SerialNumber].InternalBatteryLogCollection.Add(log);
                    }

                    foreach (var log in battery.AgentBatteryLogCollection)
                    {
                        _batteriesDic[battery.SerialNumber].AgentBatteryLogCollection.Add(log);
                    }
                }

                if (deviceEvents.Count >=1 )
                {
                    var eventDevice = deviceEvents.FirstOrDefault();
                    if (eventDevice != null)
                    {
                        var eventjson = JsonConvert.SerializeObject(deviceEvents);

                        if (_eventDeviceDic.ContainsKey(eventDevice.DeviceSerialNumber) == false)
                        {
                            _eventDeviceDic.Add(eventDevice.DeviceSerialNumber, deviceEvents);


                            _logger.Trace($"Create dicti event({deviceEvents.Count}):{eventjson}");
                        }
                        else
                        {
                            // add the tranfer log to exist battery.
                            _eventDeviceDic[eventDevice.DeviceSerialNumber].AddRange(deviceEvents);

                            _logger.Trace($"Add dicti Event({deviceEvents.Count}):{eventjson}");
                        }
                    }
                }


            }
        }

        public void handleLogRateResult(string cmd,ConcurrentBag<CommandResult> mQTTDatalist)
        {
            if (mQTTDatalist.Any() == true)
            {
                var cache = Global._memCacheHelper.GetCache<ConcurrentDictionary<string, CommandResult>>(cmd);//From Cache

                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, CommandResult>();
                }


                foreach (var data in mQTTDatalist)
                {

                    cache.AddOrUpdate(data.ID, data, (key, existingValue) =>
                    {
                        //If key exist will update the value
                        //existingValue.ID          = data.ID;
                        existingValue.Result = data.Result;
                        existingValue.DateTimeUTC = data.DateTimeUTC;
                        existingValue.Guid = data.Guid;
                        existingValue.Raw = data.Raw;

                        return existingValue;
                    });


                    _logger.Info($"LogRateResult: {data.ID} {data.Result} {data.DateTimeUTC} {data.Raw}");
                }

                Global._memCacheHelper.SetCache<ConcurrentDictionary<string, CommandResult>>(cmd, cache, -1);

            }
        }

        public void handleOTAResult(string cmd,ConcurrentBag<CommandResult> mQTTDatalist)
        {
            if (mQTTDatalist.Any() == true)
            {
                var cache = Global._memCacheHelper.GetCache<ConcurrentDictionary<string, CommandResult>>(cmd);//From Cache

                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, CommandResult>();
                }


                foreach (var data in mQTTDatalist)
                {
                    
                    cache.AddOrUpdate(data.ID, data, (key, existingValue) =>
                    {
                        //If key exist will update the value
                        //existingValue.ID          = data.ID;
                        existingValue.Result      = data.Result;
                        existingValue.DateTimeUTC = data.DateTimeUTC;
                        existingValue.Guid        = data.Guid;
                        existingValue.Raw         = data.Raw;

                        return existingValue;
                    });


                    _logger.Info($"OTAResult: {data.ID} {data.Result} {data.DateTimeUTC} {data.Raw}");
                }

                Global._memCacheHelper.SetCache<ConcurrentDictionary<string, CommandResult>>(cmd, cache, -1);

            }
        }

        public void handleNTPResult(string cmd, ConcurrentBag<CommandResult> mQTTDatalist)
        {
            if (mQTTDatalist.Any() == true)
            {
                var cache = Global._memCacheHelper.GetCache<ConcurrentDictionary<string, CommandResult>>(cmd);//From Cache

                if (cache == null)
                {
                    cache = new ConcurrentDictionary<string, CommandResult>();
                }


                foreach (var data in mQTTDatalist)
                {

                    cache.AddOrUpdate(data.ID, data, (key, existingValue) =>
                    {
                        //If key exist will update the value
                        //existingValue.ID          = data.ID;
                        existingValue.Result = data.Result;
                        existingValue.DateTimeUTC = data.DateTimeUTC;
                        existingValue.Guid = data.Guid;
                        existingValue.Raw = data.Raw;

                        return existingValue;
                    });


                    _logger.Info($"NTPResult: {data.ID} {data.Result} {data.DateTimeUTC} {data.Raw}");
                }

                Global._memCacheHelper.SetCache<ConcurrentDictionary<string, CommandResult>>(cmd, cache, -1);

            }
        }

    }
}