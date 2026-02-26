using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Common;
using GeniView.Cloud.Common.Queue;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using Hangfire;
using MQTTnet.Protocol;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using static GeniView.Cloud.Common.DataDefine;

namespace GeniView.Cloud.Controllers.API
{
    public class LogApiController : BaseApiController
    {
        bool _debugEnable = true;

        MQTTMsgParser _mQTT = new MQTTMsgParser();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        DevicesDataRepository _devicesRepo = new DevicesDataRepository();
        BatteriesDataRepository _batteriesRepo = new BatteriesDataRepository();
        G3BatteryDataRepository _g3BatteriesRepo = new G3BatteryDataRepository();
        DeviceEventsDataRepository _gdeviceEventRepository = new DeviceEventsDataRepository();

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet, Route("api/log/ProcessLog")]
        public IActionResult ProcessLog()
        {
            try
            {
                var ret = ProcessLog(CancellationToken.None);

                return Ok(ret);
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet, Route("api/ScanDevice/")]
        public IActionResult ScanDevice(bool enable)
        {
            List<Message> result = new List<Message>();

            try
            {
                if (Global._scanDevice == false && enable == true)
                {
                    //Create disable job after 3 min.
                    var jobId = BackgroundJob.Schedule(
                                () => Global.SetScanDevice(false),
                                TimeSpan.FromMinutes(GlobalSettings.ScanDeviceDurationMinutes));

                    result.Add(new Message("jobId", jobId));
                    Global._scanDevice = enable; //Enable scan.
                }
                else if (enable == false)
                {
                    Global._scanDevice = enable;
                }

                result.Add(new Message("Paraement", enable));
                result.Add(new Message("Scan device status", Global._scanDevice));
                result.Add(new Message("Scan device Duration", GlobalSettings.ScanDeviceDurationMinutes));


                return Ok(result);
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet, Route("api/DumpBatteryLog/")]
        public IActionResult DumpBatteryLog(
            string bsn = null, string dsn = null, string times = null, string timee = null, long? ids = null, long? ide = null, int? eventcode = null, string eventcodetxt = null, string batterystatus = null
            )
        {
            try
            {

                var ret = _g3BatteriesRepo.Query(bsn, dsn, times, timee, ids, ide, eventcode, eventcodetxt, batterystatus, _db).ToList();


                return Ok(ret);
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet, Route("api/ClearLog/")]
        public IActionResult ClearLog()
        {
            try
            {
                var bLog = _g3BatteriesRepo.Clear(_db);
                var dLog = _devicesRepo.Clear(_db);

                var bCount = _db.InternalBatteryLog.Count();
                var dCount = _db.InternalDeviceLog.Count();

                return Ok($"Clear log battery={bLog} qty={bCount} device={dLog} qty={dCount}");
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet, Route("api/Test/")]
        public IActionResult Test()
        {
            try
            {
                G3Definitions g3Definitions = new G3Definitions();
                List<DeviceEvent> datas = new List<DeviceEvent>();
                var data = g3Definitions.GetEvent(InternalDeviceLogEventCodes.BatteryAttached);

                var agent = _db.Agents.First();
                var device = _db.Devices.First();

                data.Device_ID = 1;
                data.Agent_ID = 1;

                data.Timestamp = DateTime.UtcNow;
                data.DeviceSerialNumber = device.SerialNumber;
                datas.Add(data);
                var ret = _gdeviceEventRepository.CreateBatch(datas, _db);



                return Ok(ret);
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Test/")]
        public IActionResult TestPost([FromBody] Object para)
        {
            try
            {
                string ret = para.ToString();
                return Ok(ret);
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        public List<Message> ProcessLog(CancellationToken cancellationToken)
        {
            List<Message> result = new List<Message>();

            //_logger.Trace($"ProcessLogJob Start {Global._scanDevice}");


            _mQTT.HandleMessage(cancellationToken);


            if (Global._scanDevice == true)
            {
                result.Add(new Message("ScanDevice", Global._scanDevice));
                //_batteriesRepo.CreateBatch(_mQTT._batteriesDic, _db);
                //_devicesRepo.CreateBatch(_mQTT._devicesDic, _db);

                List<Battery> batteryRet = _batteriesRepo.CreateAndUpdateBatch(_mQTT._batteriesDic, _db);
                List<Device> deviceRet = _devicesRepo.CreateAndUpdateBatch(_mQTT._devicesDic, _db);

                result.Add(new Message("batteryCreateUpdate", batteryRet));
                result.Add(new Message("deviceCreateUpdate", deviceRet));
            }


            //Get exist device and battery.
            var batteries = _batteriesRepo.FindBySNCode(_mQTT.Battery, _db).ToList();
            var devices = _devicesRepo.FindBySN(_mQTT.Device, _db).ToList();

            var bLog = WriteBattryLogs(batteries);
            var dLog = WriteDeviceLogs(devices);
            var eventLog = WirteDeviceEvent(batteries, devices);


            if (batteries.Count > 0)
            {
                result.Add(new Message("batteryLog", $"{batteries.Count}-{bLog}"));
            }

            if (devices.Count > 0)
            {
                result.Add(new Message("deviceLog", $"{devices.Count}-{dLog}"));

            }

            if (_mQTT._eventDeviceDic.Any() == true)
            {
                result.Add(new Message("eventLog", $"{_mQTT._eventDeviceDic.Count}-{dLog}"));

            }

            return result;

        }

        public void ProcessLogJob(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
            var ct = cancellationToken;

                if (ct.IsCancellationRequested == true)
                {
                    _logger.Error("ProcessLogJob is cancel.");
                }

                var ret = ProcessLog(ct);
            });
        }

        public void FinishLog(CancellationToken ct)
        {

            _logger.Info("Finishlog start.");
            while (true)
            {
                if (ct.IsCancellationRequested == true)
                {
                    _logger.Warn("FinishLog is cancel.");
                }
                _mQTT = new MQTTMsgParser();
                var ret = ProcessLog(ct);

                if (ret.Any() == true)
                {
                    _logger.Info($"FinishLog:{JsonConvert.SerializeObject(ret)}");
                }


            }

            _logger.Info("Finishlog end.");

        }

        private string WriteBattryLogs(List<Battery> batteries)
        {
            List<InternalBatteryLog> datas = new List<InternalBatteryLog>();
            List<AgentBatteryLog> agentDatas = new List<AgentBatteryLog>();

            var agent = GetDefaultAgentID();
            var batteryList = _db.Batteries.ToList();

            foreach (var item in batteries)
            {
                long key = item.SerialNumberCode.GetValueOrDefault();
                if (key != null)
                {
                    var ret = _mQTT._batteriesDic.TryGetValue(key.ToString(), out Battery battery);

                    if (ret == true)
                    {
                        var findBattery2 = batteryList.Where(d => d.SerialNumberCode == battery.SerialNumberCode).FirstOrDefault();

                        foreach (var log in battery.InternalBatteryLogCollection)
                        {
                            var findBattery = batteryList.Where(d => d.SerialNumberCode == log.BatterySerialNumberCode).FirstOrDefault();
                            if (findBattery != null)
                            {
                                log.Battery_ID = findBattery.ID;
                                log.Agent_ID = agent;
                            }
                        }
                        datas.AddRange(battery.InternalBatteryLogCollection);

                        foreach (var agentBattery in battery.AgentBatteryLogCollection)
                        {
                            agentBattery.Battery_ID = findBattery2.ID;
                            agentBattery.Agent_ID = agent;
                        }

                        agentDatas.AddRange(battery.AgentBatteryLogCollection);
                    }
                }
            }

            if (datas.Any() == true)
            {
                var log = datas.Select(x => new { x.LogIndex, x.BatterySerialNumber, x.BatterySerialNumberCode, x.DeviceSerialNumber });
                _logger.Trace($"InternalBattery({datas.Count}):{JsonConvert.SerializeObject(log)}");
                _g3BatteriesRepo.CreateBatch(datas, _db);
            }

            if (agentDatas.Any() == true)
            {
                var log = agentDatas.Select(x => new { x.ID, x.Battery_ID, x.DeviceSerialNumber , x.Timestamp });
                _logger.Trace($"AgentBattery({datas.Count}):{JsonConvert.SerializeObject(log)}");
                _g3BatteriesRepo.CreateBatch(agentDatas, _db);
            }

            return datas.Count.ToString();
        }

        private string WriteDeviceLogs(List<Device> devices)
        {
            List<InternalDeviceLog> datas = new List<InternalDeviceLog>();
            List<AgentDeviceLog> agentdatas = new List<AgentDeviceLog>();

            var agent = GetDefaultAgentID();

            var deviceList = _db.Devices.ToList();

            foreach (var item in devices)
            {
                string key = item.SerialNumber;

                if (key != null)
                {
                    var ret = _mQTT._devicesDic.TryGetValue(key, out Device device);

                    if (ret == true)
                    {
                        var findDevice2 = deviceList.Where(d => d.SerialNumber == device.SerialNumber).FirstOrDefault();

                        foreach (var log in device.InternalDeviceLogCollection)
                        {
                            var findDevice = deviceList.Where(d => d.SerialNumber == log.DeviceSerialNumber).FirstOrDefault();
                            if (findDevice != null)
                            {
                                log.Device_ID = findDevice.ID;
                                log.Agent_ID = agent;

                            }
                        }
                        datas.AddRange(device.InternalDeviceLogCollection);


                        foreach (var agentLog in device.AgentDeviceLogCollection)
                        {
                            if (findDevice2 != null)
                            {
                                agentLog.Device_ID = findDevice2.ID;
                                agentLog.Agent_ID = agent;
                            }
                        }
                        agentdatas.AddRange(device.AgentDeviceLogCollection);
                    }
                }
            }

            if (datas.Any() == true)
            {
                var log = datas.Select(x => new { x.LogIndex, x.DeviceSerialNumber });

                _logger.Trace($"Device({datas.Count}):{JsonConvert.SerializeObject(log)}");
            }

            _g3BatteriesRepo.CreateBatch(datas, _db);
            _g3BatteriesRepo.CreateBatch(agentdatas, _db);

            return datas.Count.ToString();
        }

        private string WirteDeviceEvent(List<Battery> batteries, List<Device> devices)
        {
            List<DeviceEvent> datas = new List<DeviceEvent>();
            var agent = GetDefaultAgentID();
            var batteryList = _db.Batteries.ToList();

            //Use exist batteries get event from dictionary.
            foreach (var item in batteries)
            {
                string key = item.SerialNumber;
                if (key != null)
                {
                    var ret = _mQTT._eventDeviceDic.TryGetValue(key.ToString(), out List<DeviceEvent> deviceEvents);

                    if (ret == true && deviceEvents != null && deviceEvents.Any() == true)
                    {
                        datas.AddRange(deviceEvents);//Add
                    }
                }
            }

            //Use exist devices get event from dictionary.
            foreach (var item in devices)
            {
                string key = item.SerialNumber;
                if (key != null)
                {
                    var ret = _mQTT._eventDeviceDic.TryGetValue(key.ToString(), out List<DeviceEvent> deviceEvents);

                    if (ret == true && deviceEvents != null && deviceEvents.Any() == true)
                    {
                        //Update device event data
                        foreach (var deviceEvent in deviceEvents)
                        {
                            deviceEvent.Device_ID = item.ID;
                        }

                        datas.AddRange(deviceEvents); //Add
                    }
                }
            }

            _gdeviceEventRepository.CreateBatch(datas, _db);
            return datas.Count.ToString();
        }


        #region Testing

        public List<Message> TestCreateJob()
        {
            List<Message> result = new List<Message>();
            List<MQTTData> mqttData = new List<MQTTData>();

            //for (int x = 0; x < 80; x++)
            //{
            //    var batterylogs = new List<G3BatteryDockLog>();
            //    //string deviceId = $"215659331{x}";

            //    //for (int y = 0; y < 1000; y++)
            //    //{
            //    //    //string str = "[{\"battery_id\":\"2156593322\",\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":171,\"TimeStampLow\":205,\"BatteryVoltage\":22.911,\"BatteryCurrent\":-0.01,\"BatteryAverageCurrent\":-0.009,\"BatteryCycleCount\":100,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9.11,\"BatteryEOLCapacity\":10.911,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":30,\"DockSerial\":4369,\"DockSerial2\":8738,\"DockSerial3\":13107,\"DockSerial4\":17476,\"DockSerial5\":21845,\"DockIndexLow\":2233,\"DockIndexHi\":3058,\"BatteryCapacity\":10.911,\"DockStatus\":8,\"DockCurrent\":1000,\"DockVoltage\":5000,\"DockTemp\":25,\"DockEvent\":2,\"DockFirmware\":3}},{\"battery_id\":\"2156593322\",\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":171,\"TimeStampLow\":205,\"BatteryVoltage\":22.911,\"BatteryCurrent\":-0.01,\"BatteryAverageCurrent\":-0.009,\"BatteryCycleCount\":100,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9.11,\"BatteryEOLCapacity\":10.911,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":30,\"DockSerial\":4369,\"DockSerial2\":8738,\"DockSerial3\":13107,\"DockSerial4\":17476,\"DockSerial5\":21845,\"DockIndexLow\":2234,\"DockIndexHi\":3058,\"BatteryCapacity\":10.911,\"DockStatus\":8,\"DockCurrent\":1000,\"DockVoltage\":5000,\"DockTemp\":25,\"DockEvent\":2,\"DockFirmware\":3}},{\"battery_id\":\"2156593322\",\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":171,\"TimeStampLow\":205,\"BatteryVoltage\":22.911,\"BatteryCurrent\":-0.01,\"BatteryAverageCurrent\":-0.009,\"BatteryCycleCount\":100,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9.11,\"BatteryEOLCapacity\":10.911,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":30,\"DockSerial\":4369,\"DockSerial2\":8738,\"DockSerial3\":13107,\"DockSerial4\":17476,\"DockSerial5\":21845,\"DockIndexLow\":2235,\"DockIndexHi\":3058,\"BatteryCapacity\":10.911,\"DockStatus\":8,\"DockCurrent\":1000,\"DockVoltage\":5000,\"DockTemp\":25,\"DockEvent\":2,\"DockFirmware\":3}},{\"battery_id\":\"2156593322\",\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":171,\"TimeStampLow\":205,\"BatteryVoltage\":22.911,\"BatteryCurrent\":-0.01,\"BatteryAverageCurrent\":-0.009,\"BatteryCycleCount\":100,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9.11,\"BatteryEOLCapacity\":10.911,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":30,\"DockSerial\":4369,\"DockSerial2\":8738,\"DockSerial3\":13107,\"DockSerial4\":17476,\"DockSerial5\":21845,\"DockIndexLow\":2236,\"DockIndexHi\":3058,\"BatteryCapacity\":10.911,\"DockStatus\":8,\"DockCurrent\":1000,\"DockVoltage\":5000,\"DockTemp\":25,\"DockEvent\":2,\"DockFirmware\":3}},{\"battery_id\":\"2156593322\",\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":171,\"TimeStampLow\":205,\"BatteryVoltage\":22.911,\"BatteryCurrent\":-0.01,\"BatteryAverageCurrent\":-0.009,\"BatteryCycleCount\":100,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9.11,\"BatteryEOLCapacity\":10.911,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":30,\"DockSerial\":4369,\"DockSerial2\":8738,\"DockSerial3\":13107,\"DockSerial4\":17476,\"DockSerial5\":21845,\"DockIndexLow\":2237,\"DockIndexHi\":3058,\"BatteryCapacity\":10.911,\"DockStatus\":8,\"DockCurrent\":1000,\"DockVoltage\":5000,\"DockTemp\":25,\"DockEvent\":2,\"DockFirmware\":3}},{\"battery_id\":\"2156593322\",\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":171,\"TimeStampLow\":205,\"BatteryVoltage\":22.911,\"BatteryCurrent\":-0.01,\"BatteryAverageCurrent\":-0.009,\"BatteryCycleCount\":100,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9.11,\"BatteryEOLCapacity\":10.911,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":30,\"DockSerial\":4369,\"DockSerial2\":8738,\"DockSerial3\":13107,\"DockSerial4\":17476,\"DockSerial5\":21845,\"DockIndexLow\":2238,\"DockIndexHi\":3058,\"BatteryCapacity\":10.911,\"DockStatus\":8,\"DockCurrent\":1000,\"DockVoltage\":5000,\"DockTemp\":25,\"DockEvent\":2,\"DockFirmware\":3}}]";

            //    //    //string str = "[{\"battery_id\":\"2156593322\",\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":171,\"TimeStampLow\":205,\"BatteryVoltage\":22.911,\"BatteryCurrent\":-0.01,\"BatteryAverageCurrent\":-0.009,\"BatteryCycleCount\":100,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9.11,\"BatteryEOLCapacity\":10.911,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":30,\"DockSerial\":4369,\"DockSerial2\":8738,\"DockSerial3\":13107,\"DockSerial4\":17476,\"DockSerial5\":21845,\"DockIndexLow\":2233,\"DockIndexHi\":3058,\"BatteryCapacity\":10.911,\"DockStatus\":8,\"DockCurrent\":1000,\"DockVoltage\":5000,\"DockTemp\":25,\"DockEvent\":2,\"DockFirmware\":3}}]";
            //    //    string str = "[{\"battery_id\":2156593322,\"firmware_version\":\"30891\",\"wifi_ssid\":\"MyWiFiNetwork\",\"wifi_rssi\":-70,\"guid\":\"0123456789ABCDEF\",\"event_log\":{\"Index\":1,\"EventCode\":50,\"TimeStampHi\":26266,\"TimeStampLow\":26695,\"DockEvent\":50,\"DockSerial\":4369,\"DockFirmware\":3,\"DockIndexLow\":172,\"DockIndexHi\":1,\"DockStatus\":8,\"DockVoltage\":5000,\"DockCurrent\":1000,\"DockTemp\":2511,\"BatteryVoltage\":22911,\"BatteryCurrent\":-100,\"BatteryAverageCurrent\":-9,\"BatteryRelStateofCharge\":80,\"BatteryRemainingCapacity\":9110,\"BatteryCapacity\":10911,\"BatteryEOLCapacity\":10911,\"BatteryCycleCount\":100,\"BatteryStatus\":18434,\"BatteryFlags\":32768,\"BatteryTemp\":3011}}]";

            //    //    str = str.Replace("2156593322", deviceId);

            //    //    //var time = DateTime.Parse("2024-07-31T03:11:00");
            //    //    var unixTime = DeviceDataHelpers.DateTimeToUnixTime(DateTime.UtcNow);

            //    //    var uint32 = (UInt32)unixTime;
            //    //    UInt16 hi =0;
            //    //    UInt16 low =0;

            //    //    DeviceDataHelpers.Uint32ToUint16(unixTime,ref hi,ref low);

            //    //    var timestamp = DeviceDataHelpers.Uint16ToUint32(hi,low);

            //    //    var Timestamp = DeviceDataHelpers.UnixTimeToDateTime(timestamp);


            //    //    MQTTData mQTTData = new MQTTData()
            //    //    {
            //    //        Topic = $"battery/log/{deviceId}",
            //    //        Payload = str
            //    //    };

            //    //    mqttData.Add(mQTTData);

            //    //    Thread.Sleep(1);
            //    //}

            //    long deviceId = 2156593310 + x;

            //    var log = new G3BatteryDockLog();
            //    var logs = log.Fack(deviceId, 100);
            //    var logJson = JsonConvert.SerializeObject(logs);

            //    MQTTData mQTTData = new MQTTData()
            //    {
            //        Topic = $"battery/log/{deviceId}",
            //        Payload = logJson
            //    };

            //    mqttData.Add(mQTTData);

            //}

            //_logger.Debug($"TestCreateJob Start{DateTime.Now.ToString(Global.dateTimeFormatfull)}");
            //result.Add(new Message("ScanDevice", Global._scanDevice));

            //_mQTT.handleBatteryLog(mqttData);

            ////_batteriesRepo.CreateBatch(_mQTT._batteriesDic, _db);
            ////_devicesRepo.CreateBatch(_mQTT._devicesDic, _db);

            //List<Battery> batteryRet = _batteriesRepo.CreateAndUpdateBatch(_mQTT._batteriesDic, _db);
            //List<Device> deviceRet = _devicesRepo.CreateAndUpdateBatch(_mQTT._devicesDic, _db);

            //result.Add(new Message("batteryCreateUpdate", batteryRet));
            //result.Add(new Message("deviceCreateUpdate", deviceRet));


            //var batteries = _batteriesRepo.FindBySNCode(_mQTT.Battery, _db).ToList();
            //var devices = _devicesRepo.FindBySN(_mQTT.Device, _db).ToList();


            //var bLog = WriteBattryLogs(batteries);
            //var dLog = WriteDeviceLogs(devices);

            //result.Add(new Message("batteryLog", $"{batteries.Count}-{bLog}"));
            //result.Add(new Message("deviceLog", $"{devices.Count}-{dLog}"));

            //_logger.Debug($"batteryLog {batteries.Count}-{bLog}");
            //_logger.Debug($"deviceLog {devices.Count}-{dLog}");


            //_logger.Debug($"TestCreateJob End {DateTime.Now.ToString(Global.dateTimeFormatfull)}");

            return result;
        }

        public string TestQueueJob()
        {
            string result = "";

            Task.Run(() =>
            {
            var ct = CancellationToken.None;

                for (int i = 0; i < 60; i++)
                {
                    _logger.Info($"TestQueueJob i={i} ct={ct.IsCancellationRequested}");

                    Thread.Sleep(1000);
                }

                _logger.Info($"TestQueueJob Finish ct={ct.IsCancellationRequested}");

            });

            return result;
        }

        public List<MQTTData> TestEnqeueJob()
        {
            List<Message> result = new List<Message>();
            List<MQTTData> mqttData = new List<MQTTData>();

            for (int x = 0; x < 5; x++)
            {
                var batterylogs = new List<G3BatteryDockLog>();

                long deviceId = 2156593310 + x;

                var log = new G3BatteryDockLog();
                var logs = log.Fack(deviceId, 100);
                var logJson = JsonConvert.SerializeObject(logs);

                MQTTData mQTTData = new MQTTData()
                {
                    Topic = $"battery/log/{deviceId}",
                    Payload = logJson
                };

                Global._queueHelp.Enqueue(mQTTData);


                mqttData.Add(mQTTData);

            }

            _logger.Info($"Queue qty:{Global._queueHelp._queue.Count}");

            return mqttData;
        }
    }
    #endregion
}
