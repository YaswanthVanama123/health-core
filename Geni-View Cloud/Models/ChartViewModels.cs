using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class GroupHeirarchyModel
    {
        public string GroupID { get; set; }
        public string ParentGroupID { get; set; }
        public string GroupName { get; set; }
        public string ParentGroupName { get; set; }
    }

    public class CommunitiesGroupCountModel
    {
        public string CommunityName { get; set; }
        public long GroupCount { get; set; }
    }

    public class BatteryModel
    {
        public long LogIndex { get; set; }
        public double? Voltage { get; set; }
        public double? Current { get; set; }
        public double? Power { get; set; }
        public double? AverageCurrent { get; set; }
        public int? RelativeStateofCharge { get; set; }
        public double? RemainingCapacity { get; set; }
        public double? CalculatedCapacity { get; set; }
        public int? CycleCount { get; set; }
        public int? Temperature { get; set; }
        public long? BatteryID { get; set; }
        public int? Bay { get; set; }
        // Global 
        public string LogDate { get; set; }
        public DateTime TimeStampt { get; set; }
        // Device Master Chart Properties
        public string DeviceLogDate { get; set; }
        public int?  DeviceTemperature { get; set; }
    }

    public class DeviceModel
    {
        //Device Model
        public long LogIndex { get; set; }
        public double? PowerCurrentInput { get; set; }
        public double? PowerCurrentOutput { get; set; }
        public double? PowerVoltageInput { get; set; }
        public double? PowerVoltageOutput { get; set; }
        public double? PowerTemperatureOutput { get; set; }
        public double? PowerOutput { get; set; }
        public double? PowerInput { get; set; }
        public string LogDate { get; set; }
        public DateTime TimeStampt { get; set; }
    }

    public class DeviceDashboardModel
    {
        public int DeviceOffline { get; set; }
        public int DeviceOnlineOnBattery { get; set; }
        public int DeviceOnlinePluggedIn { get; set; }
        public int DeviceUnknown { get; set; }
      
    }
    public class BatteryDashboardModel
    {
        //Batteries
        public int BatteryDischarging { get; set; }
        public int BatteryCharging { get; set; }
        public int BatteryNeedsCharging { get; set; }
        public int BatteryReadyToUse { get; set; }
        public int BatteryUnknown { get; set; }

        // Power Statistics
        public int PowerAvailableStateOfCharge { get; set; }
        public double PowerAvailableCapacity { get; set; }
        public int PowerUsageStateOfCharge { get; set; }
        public double PowerUsageCapacity { get; set; }
        public double PowerUsageConsumption { get; set; }
    }

    public class OnlineItemsChartModel
    {
        public string Date { get; set; }
        public int OnlineCount { get; set; }
    }
    
    public class DeviceBayDataModel
    {
        public int BayCount { get; set; }
        public List<string> Capacity { get; set; }
        public List<string> SerialNumber { get; set; }
    }

}