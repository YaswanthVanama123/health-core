using GeniView.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class Message
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public Message(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class Command
    {
        public string ID { get; set; }

        public string Cmd { get; set; }

        public Guid? Guid { get; set; }

        public string DateTimeUTC { get; set; }

        public string Raw { get; set; }

        public Command()
        {
            Guid = System.Guid.NewGuid();
            DateTimeUTC = DateTime.UtcNow.ToString("o");//ISO 8601
            Raw = "";
        }
    }

    public class CommandResult : Command
    {
        //public int IntervalSec { get; set; }
        public string SN { get; set; }

        public bool Result { get; set; }

        public string Connection { get; set; }

        public string Type { get; set; }

        public bool Status { get; set; }

        public string SSID { get; set; }

        public string PWD { get; set; }

        public string Broker { get; set; }

        public string BrokerAccount { get; set; }

        public string BrokerPWD { get; set; }

        public int IntervalSec { get; set; }

        public CommandResult() 
        {
            if (ID != null && ID.Length >= 10)
            {
                var snCodeCheck = long.TryParse(ID, out long sncode);
                if (snCodeCheck)
                {
                    SN = DataHelpers.BatterySerialNumberCodeToSerialNumber(sncode);
                }
            }
        }

        [JsonConstructor]
        public CommandResult(string id)
        {
            ID = id;
            if (id != null && id.Length >= 10)
            {
                var snCodeCheck = long.TryParse(id, out long sncode);
                if (snCodeCheck)
                {
                    SN = DataHelpers.BatterySerialNumberCodeToSerialNumber(sncode);
                }
            }
        }
    }


    public class LogRate : Command
    {
        [Required]
        [Range(1,int.MaxValue)]
        public int IntervalSec { get; set; }

        public LogRate() { }
        public LogRate(string id, int sec)
        {
            ID = id;
            Cmd = "LogRate";
            IntervalSec = sec;
        }
    }


    public class OTA : Command
    {
        [JsonProperty(Order = 1)]
        public string URL { get; set; }

        public OTA() { }
        public OTA(string id,  string url)
        {
            ID = id;
            Cmd = "OTA";
            URL = url;
        }
    }

    public class NTP : Command
    {
        [JsonProperty(Order = 1)]
        public string NTPURL { get; set; }
        [JsonProperty(Order = 2)]
        public string NTPUTC { get; set; }

        public NTP() { }
        public NTP(string id, string url, string utc)
        {
            ID = id;
            Cmd = "NTP";
            NTPURL = url;
            NTPUTC = utc;
        }
    }
}