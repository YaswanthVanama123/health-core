using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using GeniView.Data.Web;
using GeniView.Data.Hardware;
using GeniView.Data.Agent;
using GeniView.Data.Hardware.Event;

namespace GeniView.Cloud.Models
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Please, select role!!!")]
        public string RoleName { get; set; }
        public string GroupName { get; set; }
        public string CommunityName { get; set; }

        public bool ActivateUserByEmail { get; set; }
        public bool isUserLocked { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class GroupViewModel
    {
        public long CommunityID { get; set; }
        public long? ParentGroupID { get; set; }
        public Group Group { get; set; }
    }

    public class AgentViewModel
    {
        public ExtraInfo Status { get; set; }
        public Agent Agent { get; set; }
    }
    #region Batteries
    public class BatteriesListViewModel
    {
        public long ID { get; set; }
        public Battery Battery { get; set; }
        public long? CommunityID { get; set; }
        public long? GroupID { get; set; }
        public Community Community { get; set; }
        public Group Group { get; set; }
        public DateTime? LastSeenOn { get; set; }
        public DateTime? FirstSeenOn { get; set; }
        public string State { get; set; }
        public bool isOnline { get; set; }
        public ExtraInfo Status { get; set; }
        public ExtraInfo Alert { get; set; }
        public ExtraInfo ChargingLevel { get; set; }
        public ExtraInfo Temperature { get; set; }
        public AgentBatteryLog LastAgentBatteryLog { get; set; }
    }

    public class BatteryDetailViewModel
    {
        public BatterySettings LastSettings { get; set; }
        public AgentBatteryLog LastAgentBatteryLog { get; set; }
        public DateTime? FirstSeenOn { get; set; }
        public DateTime? LastSeenOn { get; set; }
        public Battery Battery { get; set; }
        public ExtraInfo Status { get; set; }
        public ExtraInfo Voltage { get; set; }
        public ExtraInfo Charging { get; set; }
        public ExtraInfo RemainingCapacity { get; set; }
        public ExtraInfo CalcCapacity { get; set; }
        public BatteryStates State { get; set; }
    }

    public class BatteryHistoryLogFilter
    {
        public long ID { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
        public bool isPeriodicDataTriggerIncluded { get; set; }
        public List<InternalBatteryLog> LogList { get; set; }
    }
    #endregion

    #region Devices
    public class DeviceListViewModel
    {
        public long? CommunityID { get; set; }
        public long? GroupID { get; set; }

        public DateTime? FirstSeenOn { get; set; }
        public DateTime? LastSeenOn { get; set; }
        public double PowerOutput { get; set; }
        public long ID { get; set; }
        public string SerialNumber { get; set; }
        public Community Community { get; set; }
        public Group Group { get; set; }
        public AgentDeviceLog LastAgentDeviceLog { get; set; }
        public DeviceSettings LastDeviceSetting { get; set; }
        // Hide or show Data according to Status
        public bool isOnline { get; set; }
        public ExtraInfo Status { get; set; }
        public ExtraInfo Alert { get; set; }
        public ExtraInfo Capacity { get; set; }
        public ExtraInfo Temperature { get; set; }
    }

    public class DeviceDetailsViewModel
    {
        public DeviceSettings LastSettings { get; set; }
        public AgentDeviceLog LastAgentDeviceLog { get; set; }
        public DateTime? FirstSeenOn { get; set; }
        public DateTime? LastSeenOn { get; set; }
        public Device Device { get; set; }
        public Community Community { get; set; }
        public Group Group { get; set; }
        public ExtraInfo PowerOutput { get; set; }
        public ExtraInfo PowerInput { get; set; }
    }

    public class DeviceHistoryLogFilter
    {
        public long ID { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
        public bool isPeriodicDataTriggerIncluded { get; set; }
        public List<DeviceHistoryViewModel> LogList { get; set; }
    }

    public class DeviceEventLogFilter
    {
        public long DeviceID { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
        public List<DeviceEvent> EventList { get; set; }
    }

    public class DeviceHistoryViewModel
    {
        public InternalDeviceLog InternalDeviceLog { get; set; }
        public IEnumerable<Battery> Batteries { get; set; }
        public IEnumerable<int> OemIds { get; set; }
    }
    #endregion

    public class ExtraInfo
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Title { get; set; }
        public string Color { get; set; }
    }

    public class DeviceActionNotificationViewModel
    {
        public bool IsChecked { get; set; }
        public DeviceEventNotification DeviceEventNotification { get; set; }
    }

    public class MessageViewModel
    {
        public string FullName { get; set; }
        public string CallbackUrl { get; set; }
        public string Subject { get; set; }
    }

    public class DeviceLocationViewModel
    {
        public string SerialNumber { get; set; }
        public string UserName { get; set; }
        public double Longitude { get; set; }
        public double Lattitude { get; set; }
        public bool IsUnknown { get; set; }
        public ExtraInfo Status { get; set; }
    }

    public class LocationLatLong
    {
        public double Longitude { get; set; }
        public double Lattitude { get; set; }
    }
}