using GeniView.Cloud.Common.Queue;
using GeniView.Cloud.Models;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using RenityArtemis.Web.Common;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GeniView.Cloud.Common
{
    public static class Global
    {
        public static string dateFormat         = "yyyy-MM-dd";
        public static string dateTimeFormat     = "yyyy-MM-dd HH:mm:ss";
        public static string fileDateTime       = "yyyy-MM-dd_HH-mm-ss-fff";
        public static string dateTimeFormatfull = "yyyy-MM-dd HH:mm:ss:ffff";

        // Set at application startup in Program.cs:
        //   Global._serverPath = app.Environment.ContentRootPath;
        public static string _serverPath = string.Empty;
        public static string _otaPath    = @"Files\Device\displayboard.bin";

        public static ConcurrentQueue<MQTTData> _msgQueue = new ConcurrentQueue<MQTTData>();
        public static Logger _logger = LogManager.GetCurrentClassLogger();

        public static QueueHelp _queueHelp = new QueueHelp();
        public static bool _scanDevice = false;

        // MemCacheHelper requires IMemoryCache — initialized in Program.cs after DI is built.
        // Access via IMemoryCache injection in controllers/services instead of this static.
        public static MemCacheHelper? _memCacheHelper = null;

#if (DEBUG)
        public static bool _enableHangFire = false;
#else
        public static bool _enableHangFire = true;
#endif

        public static bool SetScanDevice(bool value)
        {
            _scanDevice = value;
            return _scanDevice;
        }

        public static void DebugPrintf(string msg, bool enable)
        {
#if (DEBUG)
            if (enable)
            {
                Debug.WriteLine(msg);
            }
#endif
        }

        public static MemCacheHelper? GetMemCacheHelper()
        {
            return _memCacheHelper;
        }
    }
}
