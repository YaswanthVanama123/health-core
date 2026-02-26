using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GeniView.Cloud.Models
{
    public static class MQTTTopic
    {
        static public string Battery { get; set; }              = "battery";
        static public string Dock { get; set; }                 = "dock";
        static public string LogRate { get; set; }              = "lograte";

        static public string BatteryLog { get; set; } = "battery/log";
        static public string DockLog { get; set; } = "dock/log";

        static public string BatteryLogRate { get; set; }       = "battery/lograte/cmd";
        static public string BatteryLogRateResult { get; set; } = "battery/lograte/result";

        static public string BatteryOTA { get; set; } = "battery/ota/cmd";
        static public string BatteryOTAResult { get; set; } = "battery/ota/result";

        static public string BatteryNTP { get; set; } = "battery/ntp/cmd";
        static public string BatteryNTPResult { get; set; } = "battery/ntp/result";

        //Because need to identify the result topic. Use compiled regex performance almost close to split and more clearly.
        static public readonly Regex BatteryLogRegex           = new Regex(@"^battery\/log\/[^\/]+$", RegexOptions.Compiled);
        static public readonly Regex BatteryLograteRegex       = new Regex(@"^battery\/lograte\/[^\/]+$", RegexOptions.Compiled);
        static public readonly Regex BatteryLograteResultRegex = new Regex(@"^battery\/lograte\/result\/[^\/]+$", RegexOptions.Compiled);
        static public readonly Regex ResultRegex = new Regex(@"/result/");
        static public readonly Regex DockLogRegex = new Regex(@"^dock\/log\/[^\/]+$", RegexOptions.Compiled);


        static public List<string> Topics { get; set; } = new List<string>()
        {
           $"{BatteryLog}/#",
           $"{BatteryLogRateResult}/#",
           $"{BatteryOTAResult}/#",
           $"{BatteryNTPResult}/#",
        };


        public static string GetLogRate(string id)
        {
            string result = $"{BatteryLogRate}/{id}";

            return result;
        }

        public static string GetOTA(string id)
        {
            string result = $"{BatteryOTA}/{id}";

            return result;
        }

        public static string GetNTP(string id)
        {
            string result = $"{BatteryNTP}/{id}";

            return result;
        }

    }
}