using BatteryClient;
using Geni_View_SettingTool.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Geni_View_SettingTool.Common
{
    public static class Global
    {
        public static string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";//24 hour format
        public static string _dateTimeFormatfull = "yyyy-MM-dd HH:mm:ss:ffff";//24 hour format
        public static string _dateFormat = "yyyy-MM-dd";//24 hour format
        public static string _fileDateTime = "yyyy-MM-dd_HH-mm-ss-fff";//24 hour format

        public static Setting _setting = new Setting();
        public static MQTTHelper _mQTTHelper;

        public static Dashboard _dashboard = new Dashboard();
        public static ConcurrentDictionary<string, Device> _devices = new ConcurrentDictionary<string, Device>();

        public static appViewModel _appViewModel = new appViewModel();

    }
}