using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GeniView.Cloud.Controllers.API
{
    public class CommandApiController : BaseApiController
    {

        public class CmdRequest
        {
            public string Type { get; set; }
            public string Data { get; set; }
            public string DeviceType { get; set; }
            public string DeviceId { get; set; }
        }
        public enum CMDCATEGORY
        {
            BATTERY,
            DOCK,
            BATTERYRESULT,
            DOCKRESULT
        }

        public Dictionary<string, string> BasicPublishTopics { get; set; } = new Dictionary<string, string>()
        {
            { CMDCATEGORY.BATTERY.ToString(), "battery/cmd/" },
            { CMDCATEGORY.DOCK.ToString()   , "dock/cmd/"},
            { CMDCATEGORY.BATTERYRESULT.ToString()   , "server/cmd/battery/result/"},
            { CMDCATEGORY.DOCKRESULT.ToString()   , "server/cmd/dock/result/"}
        };

        BatteriesDataRepository _batteriesrpo = new BatteriesDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/SendCommand/")]
        public IActionResult SendCommand(CmdRequest cmd)
        {
            try
            {
                //According to the command type, publish the message to MQTT broker
                if (BasicPublishTopics.ContainsKey(cmd.DeviceType.ToUpper()) == false)
                {
                    return BadRequest("Device type is not exist!");
                }
                else
                {
                    //Prepare the topic for specify usage
                    string topic = $"{BasicPublishTopics[cmd.DeviceType.ToUpper()]}{cmd.DeviceId}";

                    //Transfer the data to real command
                    string message = cmd.Data;

                    //Publish to deivce
                    //MQTTHelper.Instance.Publish(topic, message, MqttQualityOfServiceLevel.AtLeastOnce);

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/ReportFrequencyBatch/")]
        public async Task<IActionResult> GetReportFrequency(List<string> SerialNumberCode )
        {
            var result = "";
            try
            {
                if (SerialNumberCode == null || SerialNumberCode.Any() == false)
                {
                    SerialNumberCode = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode.ToString()).ToList();
                }
                else
                {
                    var ret = SerialNumberCode.Where(x => 
                        !_batteriesrpo.GetBatteries(_db).Select(b => b.SerialNumberCode.ToString()).Contains(x)
                    ).ToList();

                    StringBuilder sb = new StringBuilder();

                    foreach (var item in ret)
                    {
                        sb.Append($"{item},");
                    }

                    if (ret != null && ret.Any() == true)
                    {
                        return ResponseErrorMessage(HttpStatusCode.NotFound, sb.ToString().TrimEnd(','));
                    }
                }
               

                var data = Global._memCacheHelper.GetLogRateResult(SerialNumberCode);

                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/ReportFrequency/")]
        public async Task<IActionResult> PostReportFrequency(LogRate logRate, string SerialNumberCode = null)
        {
            List<object> result = new List<object>();

            try
            {
                if (ModelState.IsValid == true)
                {
                    if (string.IsNullOrEmpty(SerialNumberCode) == true)
                    {
                        //To all device.

                        var batteries = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode).ToList();

                        foreach (var item in batteries)
                        {
                            LogRate cmd = new LogRate(item.ToString(),logRate.IntervalSec);
                            string para = JsonConvert.SerializeObject(cmd);

                            string topic = MQTTTopic.GetLogRate(item.ToString());

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.AtLeastOnce);
                            var data = new { SN = item.Value.ToString(), ret.IsSuccess , ret.ReasonCode , ret.ReasonString};
                            result.Add(data);
                        }

                        return Ok(result);
                    }
                    else
                    {
                        //Specify a device

                        var exist = _batteriesrpo
                            .GetBatteries(_db).Where(x => x.SerialNumberCode.ToString() == SerialNumberCode).Any();

                        if (exist == true)
                        {
                            string topic = MQTTTopic.GetLogRate(SerialNumberCode.ToString());

                            LogRate cmd = new LogRate(SerialNumberCode.ToString(), logRate.IntervalSec);
                            string para = JsonConvert.SerializeObject(cmd);

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.AtLeastOnce);
                            var data = new { SN = SerialNumberCode, ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                            result.Add(data);

                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest($"SerialNumberCode {SerialNumberCode} doesn't exist.");
                        }
                    }
                }
                else
                {
                    var message = string.Join(" ", ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage));

                    return BadRequest(message);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/SetOTA/")]
        public async Task<IActionResult> PostOTA(string SerialNumberCode = null)
        {
            List<object> result = new List<object>();

            string filePath = $@"{Global._serverPath}{Global._otaPath}";
            
            //If file does'n exist will create folder.
            if (System.IO.File.Exists(filePath) == false)
            {
                return BadRequest($"OTA file doesn't exist.");
            }

            try
            {
                if (ModelState.IsValid == true)
                {
                    string path = Url.Content("~/" + Global._otaPath);

                    if (string.IsNullOrEmpty(SerialNumberCode) == true)
                    {
                        //To All

                        var batteries = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode).ToList();

                        //Clear
                        var cache = new ConcurrentDictionary<string, CommandResult>();
                        Global._memCacheHelper.SetCache<ConcurrentDictionary<string, CommandResult>>("OTAResult", cache, -1);

                        foreach (var item in batteries)
                        {
                            OTA cmd = new OTA(item.ToString(), path);
                            string para = JsonConvert.SerializeObject(cmd);

                            string topic = MQTTTopic.GetOTA(item.ToString());

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.AtLeastOnce);
                            var data = new { SN = item.Value.ToString(), ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                            result.Add(data);
                        }

                        return Ok(result);
                    }
                    else
                    {
                        //Specify a device

                        var exist = _batteriesrpo
                            .GetBatteries(_db).Where(x => x.SerialNumberCode.ToString() == SerialNumberCode).Any();
                        OTA cmd = new OTA(SerialNumberCode, path);


                        if (exist == true)
                        {
                            string para = JsonConvert.SerializeObject(cmd);
                            string topic = MQTTTopic.GetOTA(SerialNumberCode.ToString());

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.AtLeastOnce);
                            var data = new { SN = SerialNumberCode, ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                            result.Add(data);

                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest($"SerialNumberCode {SerialNumberCode} doesn't exist.");
                        }
                    }
                }
                else
                {
                    var message = string.Join(" ", ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage));

                    return BadRequest(message);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/GetOTA/")]
        public async Task<IActionResult> GetOTA(List<string> SerialNumberCode)
        {
            var result = "";
            try
            {
                if (SerialNumberCode == null || SerialNumberCode.Any() == false)
                {
                    SerialNumberCode = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode.ToString()).ToList();
                }
                else
                {
                    var ret = SerialNumberCode.Where(x =>
                        !_batteriesrpo.GetBatteries(_db).Select(b => b.SerialNumberCode.ToString()).Contains(x)
                    ).ToList();


                    if (ret != null && ret.Any() == true)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var item in ret)
                        {
                            sb.Append($"{item},");
                        }

                        return ResponseErrorMessage(HttpStatusCode.NotFound, sb.ToString().TrimEnd(','));
                    }
                }


                var data = Global._memCacheHelper.GetOTAResult(SerialNumberCode);

                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/UploadOTAFile/")]
        public async Task<IActionResult> UploadOTAFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return StatusCode((int)System.Net.HttpStatusCode.UnsupportedMediaType, "No file uploaded.");
            }

            try
            {
                string filePath = "{Global._serverPath}{Global._otaPath}";
                string fileUrlPath = Url.Content("~/" + Global._otaPath);

                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                }

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fs);
                }

                return Ok(fileUrlPath);
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/SetNTP/")]
        public async Task<IActionResult> PostNTP(NTP ntp , string SerialNumberCode = null)
        {
            List<object> result = new List<object>();

            try
            {
                if (ModelState.IsValid == true)
                {

                    if (string.IsNullOrEmpty(ntp.NTPURL) == true && string.IsNullOrEmpty(ntp.NTPUTC) == true)
                    {
                        return BadRequest("NTP URL or UTC is empty.");
                    }

                    if (ntp.NTPURL != null && ntp.NTPURL.Length >= 1)
                    {
                        ntp.NTPUTC = "";
                    }
                    else
                    {
                        ntp.NTPURL = "";
                    }

                    if (string.IsNullOrEmpty(SerialNumberCode) == true)
                    {
                        //To all device

                        var batteries = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode).ToList();

                        //Clear
                        var cache = new ConcurrentDictionary<string, CommandResult>();
                        Global._memCacheHelper.SetCache<ConcurrentDictionary<string, CommandResult>>("NTPResult", cache, -1);

                        foreach (var item in batteries)
                        {
                            NTP cmd = new NTP(item.ToString(), ntp.NTPURL , ntp.NTPUTC);
                            string para = JsonConvert.SerializeObject(cmd);

                            string topic = MQTTTopic.GetNTP(item.ToString());

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.AtLeastOnce);
                            var data = new { SN = item.Value.ToString(), ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                            result.Add(data);
                        }

                        return Ok(result);
                    }
                    else
                    {
                        //Specify a device
                        var exist = _batteriesrpo
                            .GetBatteries(_db).Where(x => x.SerialNumberCode.ToString() == SerialNumberCode).Any();

                        NTP cmd = new NTP(SerialNumberCode, ntp.NTPURL, ntp.NTPUTC);


                        if (exist == true)
                        {
                            string para = JsonConvert.SerializeObject(cmd);
                            string topic = MQTTTopic.GetNTP(SerialNumberCode.ToString());

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.AtLeastOnce);
                            var data = new { SN = SerialNumberCode, ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                            result.Add(data);

                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest($"SerialNumberCode {SerialNumberCode} doesn't exist.");
                        }
                    }
                }
                else
                {
                    var message = string.Join(" ", ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.Exception));

                    var ret = CollectModelState(ModelState);
                    return BadRequest(ret);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/GetNTP/")]
        public async Task<IActionResult> GetNTP(List<string> SerialNumberCode)
        {
            var result = "";
            try
            {
                if (SerialNumberCode == null || SerialNumberCode.Any() == false)
                {
                    SerialNumberCode = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode.ToString()).ToList();
                }
                else
                {
                    var ret = SerialNumberCode.Where(x =>
                        !_batteriesrpo.GetBatteries(_db).Select(b => b.SerialNumberCode.ToString()).Contains(x)
                    ).ToList();


                    if (ret != null && ret.Any() == true)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var item in ret)
                        {
                            sb.Append($"{item},");
                        }

                        return ResponseErrorMessage(HttpStatusCode.NotFound, sb.ToString().TrimEnd(','));
                    }
                }


                var data = Global._memCacheHelper.GetNTPResult(SerialNumberCode);

                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        private string CollectModelState(ModelStateDictionary ModelState)
        {
            var errorMessageBuilder = new StringBuilder();

            foreach (var key in ModelState.Keys)
            {
                var modelStateEntry = ModelState[key];
                if (modelStateEntry.Errors.Any())
                {
                    errorMessageBuilder.AppendLine($"Key: {key}");
                    foreach (var error in modelStateEntry.Errors)
                    {
                        errorMessageBuilder.AppendLine($"  ErrorMessage: {error.ErrorMessage}");
                        if (error.Exception != null)
                        {
                            errorMessageBuilder.AppendLine($"  Exception: {error.Exception.Message}");
                        }
                    }
                }
            }

            return errorMessageBuilder.ToString();
        }
    }
}
