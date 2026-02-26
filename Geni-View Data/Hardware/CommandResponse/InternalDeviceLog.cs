using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace GeniView.Data.Hardware
{
    [Index(nameof(Device_ID), nameof(Timestamp), Name = "IX_Device_ID_Timestamp")]
    public class InternalDeviceLog : Abstract.Data
    {
        public InternalDeviceLog() { }

        public InternalDeviceLog(int eventCodeRaw, int batteriesPresentRaw, int batteriesPoweringSystemRaw, int batteriesChargingRaw, int statusRaw, int temperature)
        {
            EventCodeRaw = eventCodeRaw;
            BatteriesPresentRaw = batteriesPresentRaw;
            BatteriesPoweringSystemRaw = batteriesPoweringSystemRaw;
            BatteriesChargingRaw = batteriesChargingRaw;

            // Set the corresponding values for other properties.
            EventCode = Enum.IsDefined(typeof(InternalDeviceLogEventCodes), EventCodeRaw) ? (InternalDeviceLogEventCodes)EventCodeRaw : InternalDeviceLogEventCodes.Unknown;
            EventCodeText = EnumHelper.GetFriendlyText(EventCode, ",");

            BatteriesPresent = (Batteries)BatteriesPresentRaw;
            BatteriesPresentText = EnumHelper.GetFriendlyText(BatteriesPresent, ",");
            BatteriesPresentCount = DataHelpers.CountBits(BatteriesPresentRaw);

            BatteriesPoweringSystem = (Batteries)BatteriesPoweringSystemRaw;
            BatteriesPoweringSystemText = EnumHelper.GetFriendlyText(BatteriesPoweringSystem, ",");
            BatteriesPoweringSystemCount = DataHelpers.CountBits(BatteriesPoweringSystemRaw);

            BatteriesCharging = (Batteries)BatteriesChargingRaw;
            BatteriesChargingText = EnumHelper.GetFriendlyText(BatteriesCharging, ",");
            BatteriesChargingCount = DataHelpers.CountBits(BatteriesChargingRaw);

            Status = new DeviceStatus(statusRaw, temperature);
        }

        public InternalDeviceLog DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            InternalDeviceLog n = (InternalDeviceLog)this.MemberwiseClone();
            n.EventCodeText = this.EventCodeText == null ? null : string.Copy(this.EventCodeText);
            n.DeviceSerialNumber = this.DeviceSerialNumber == null ? null : string.Copy(this.DeviceSerialNumber);
            n.BatteriesPresentText = this.BatteriesPresentText == null ? null : string.Copy(this.BatteriesPresentText);
            n.BatteriesPoweringSystemText = this.BatteriesPoweringSystemText == null ? null : string.Copy(this.BatteriesPoweringSystemText);
            n.BatteriesChargingText = this.BatteriesChargingText == null ? null : string.Copy(this.BatteriesChargingText);
            n.Status = this.Status == null ? null : this.Status.DeepCopy();
            n.PowerOutput = this.PowerOutput == null ? null : this.PowerOutput.DeepCopy();
            n.RawData = this.RawData == null ? null : string.Copy(this.RawData);

            if (this.InternalBatteryLogs != null)
            {
                n.InternalBatteryLogs = new List<InternalBatteryLog>();

                foreach (var item in this.InternalBatteryLogs)
                {
                    n.InternalBatteryLogs.Add(item.DeepCopy());
                }
            }
            else
            {
                n.InternalBatteryLogs = null;
            }

            return n;
        }

        [XmlIgnore] // Need this for XmlSerializer used for exporting to XML file.
        public long ID { get; set; }

        public long LogIndex { get; set; }

        public string EventCodeText { get; set; }

        [XmlIgnore]
        public InternalDeviceLogEventCodes EventCode { get; set; }

        [XmlElement("EventCode")]
        public int EventCodeRaw { get; set; }

        public DateTime Timestamp { get; set; }

        public string DeviceSerialNumber { get; set; }

        public string BatteriesPresentText { get; set; }

        [XmlIgnore]
        public Batteries BatteriesPresent { get; set; }

        public int BatteriesPresentCount { get; set; }

        [XmlElement("BatteriesPresent")]
        public int BatteriesPresentRaw { get; set; }

        public string BatteriesPoweringSystemText { get; set; }

        [XmlIgnore]
        public Batteries BatteriesPoweringSystem { get; set; }

        public int BatteriesPoweringSystemCount { get; set; }

        [XmlElement("BatteriesPoweringSystem")]
        public int BatteriesPoweringSystemRaw { get; set; }

        public string BatteriesChargingText { get; set; }

        [XmlIgnore]
        public Batteries BatteriesCharging { get; set; }

        public int BatteriesChargingCount { get; set; }

        [XmlElement("BatteriesCharging")]
        public int BatteriesChargingRaw { get; set; }

        public DeviceStatus Status { get; set; }

        public DevicePower PowerOutput { get; set; }

        [NotMapped]
        [XmlIgnore] // Need this for XmlSerializer used for exporting to XML file.
        public List<InternalBatteryLog> InternalBatteryLogs { get; set; }

        public string RawData { get; set; }

        #region Navigation Properties
        public Nullable<long> Agent_ID { get; set; }

        public Nullable<long> Device_ID { get; set; }

        [ForeignKey("Device_ID")]  //Add this attribute or we cannot update the Device_ID.
        [XmlIgnore] // Need this for XmlSerializer used for exporting to XML file.
        public virtual Device Device { get; set; }

        [ForeignKey("Agent_ID")]  //Add this attribute or we cannot update the Agent_ID.
        [XmlIgnore] // Need this for XmlSerializer used for exporting to XML file.
        public virtual Agent.Agent Agent { get; set; }

        public void ClearNavigationProperties()
        {
            Device = null;
            Agent = null;
        }

        #endregion
    }
}
