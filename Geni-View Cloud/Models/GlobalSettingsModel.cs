using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class GlobalSettingModel
    {
        public int OnlineRangeInMinutes { get; set; }
        public int OfflineRangeInDays { get; set; }
        public string SuccessColor { get; set; }
        public string WarningColor { get; set; }
        public string AlertColor { get; set; }
        public int SuccessTemperature { get; set; }
        public int AlertTemperature { get; set; }
        public int SuccessChargingLVL { get; set; }
        public int AlertChargingLVL { get; set; }
        public int IsStateOfChargeReadyToUse { get; set; }
        public double NominalVoltage { get; set; }
        public string BingMapKey { get; set; }
        public int NotificationDelayTimeInSeconds { get; set; }
        public int NotificationToleranceInMinutes { get; set; }
        public int UserLockoutTimeInMinutes { get; set; }
    }
}