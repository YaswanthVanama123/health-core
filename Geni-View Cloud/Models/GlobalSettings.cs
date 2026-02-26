using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GeniView.Cloud.Models
{
    // GlobalSettings reads from appsettings.json via IConfiguration.
    // Call GlobalSettings.Initialize(configuration) once from Program.cs at startup.
    // In-memory overrides (via GeneralSettingsController) shadow the config values at runtime.
    // TODO Phase 3: persist dynamic changes to the GeneralSettings database table instead of in-memory.
    public class GlobalSettings
    {
        private static IConfiguration? _configuration;
        private static readonly Dictionary<string, string?> _overrides = new Dictionary<string, string?>();

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static string? GetRaw(string configName)
        {
            if (_overrides.TryGetValue(configName, out var ov))
                return ov;
            return _configuration?[$"AppSettings:{configName}"];
        }

        private static void SetRaw(string configName, string? value)
        {
            _overrides[configName] = value;
        }

        private static T Get<T>(string configName)
        {
            string? value = GetRaw(configName);
            if (value == null)
            {
                if (_configuration == null)
                    throw new InvalidOperationException("GlobalSettings.Initialize() has not been called.");
                throw new Exception($"Could not find setting '{configName}'");
            }
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static DateTime OnlineRangeInMinutes
            => DateTime.UtcNow.AddMinutes(-1 * Get<int>("OnlineRangeInMinutes"));

        public static int OnlineRangeInMinutesValue
        {
            get => Get<int>("OnlineRangeInMinutes");
            set => SetRaw("OnlineRangeInMinutes", value.ToString());
        }

        public static DateTime OfflineRangeInDays
            => DateTime.UtcNow.AddDays(-1 * Get<int>("OfflineRangeInDays"));

        public static int OfflineRangeInDaysValue
        {
            get => Get<int>("OfflineRangeInDays");
            set => SetRaw("OfflineRangeInDays", value.ToString());
        }

        public static string SuccessColor
        {
            get => Get<string>("SuccessColor");
            set => SetRaw("SuccessColor", value);
        }

        public static string WarningColor
        {
            get => Get<string>("WarningColor");
            set => SetRaw("WarningColor", value);
        }

        public static string AlertColor
        {
            get => Get<string>("AlertColor");
            set => SetRaw("AlertColor", value);
        }

        public static int SuccessTemperature
        {
            get => Get<int>("SuccessTemperature");
            set => SetRaw("SuccessTemperature", value.ToString());
        }

        public static int AlertTemperature
        {
            get => Get<int>("AlertTemperature");
            set => SetRaw("AlertTemperature", value.ToString());
        }

        public static int SuccessChargingLVL
        {
            get => Get<int>("SuccessChargingLVL");
            set => SetRaw("SuccessChargingLVL", value.ToString());
        }

        public static int AlertChargingLVL
        {
            get => Get<int>("AlertChargingLVL");
            set => SetRaw("AlertChargingLVL", value.ToString());
        }

        public static int IsStateOfChargeReadyToUse
        {
            get => Get<int>("IsStateOfChargeReadyToUse");
            set => SetRaw("IsStateOfChargeReadyToUse", value.ToString());
        }

        public static double NominalVoltage
        {
            get => Get<double>("NominalVoltage");
            set => SetRaw("NominalVoltage", value.ToString(CultureInfo.InvariantCulture));
        }

        public static string BingMapKey
        {
            get => Get<string>("BingMapKey");
            set => SetRaw("BingMapKey", value);
        }

        public static int NotificationDelayTimeInSeconds
        {
            get => Get<int>("NotificationDelayTimeInSeconds");
            set => SetRaw("NotificationDelayTimeInSeconds", value.ToString());
        }

        public static int NotificationToleranceInMinutes
        {
            get => Get<int>("NotificationToleranceInMinutes");
            set => SetRaw("NotificationToleranceInMinutes", value.ToString());
        }

        public static int UserLockoutTimeInMinutes
        {
            get => Get<int>("UserLockoutTimeInMinutes");
            set => SetRaw("UserLockoutTimeInMinutes", value.ToString());
        }

        public static int ScanDeviceDurationMinutes => Get<int>("ScanDeviceDurationMinutes");
    }
}
