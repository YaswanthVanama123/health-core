using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace GeniView.Data.Hardware
{
    [Index(nameof(Battery_ID), nameof(Timestamp), Name = "IX_BatteryID_Timestamp")]
    public class InternalBatteryLog : Abstract.Data
    {
        public InternalBatteryLog() { }

        public InternalBatteryLog(int eventCodeRaw, int statusARaw, int statusBRaw)
        {
            EventCodeRaw = eventCodeRaw;
            // Bay position is stored in StatusA bits 8-10. 
            // Set this before assigning to BatteryOperatingStatus as it will clear these bits.
            Bay = (statusARaw & 0x700) >> 8;

            // Set the corresponding values for other properties.
            EventCode = Enum.IsDefined(typeof(InternalDeviceLogEventCodes), EventCodeRaw) ? (InternalDeviceLogEventCodes)EventCodeRaw : InternalDeviceLogEventCodes.Unknown;
            EventCodeText = EnumHelper.GetFriendlyText(EventCode, ",");

            BatteryOperatingStatus = new BatteryOperatingStatus(statusARaw, statusBRaw);
        }

        public InternalBatteryLog DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            InternalBatteryLog n = (InternalBatteryLog)this.MemberwiseClone();
            n.UniqueLogIndex = this.UniqueLogIndex == null ? null : string.Copy(this.UniqueLogIndex);
            n.EventCodeText = this.EventCodeText == null ? null : string.Copy(this.EventCodeText);
            n.BatterySerialNumber = this.BatterySerialNumber == null ? null : string.Copy(this.BatterySerialNumber);
            n.DeviceSerialNumber = this.DeviceSerialNumber == null ? null : string.Copy(this.DeviceSerialNumber);
            n.BatteryOperatingStatus = this.BatteryOperatingStatus == null ? null : this.BatteryOperatingStatus.DeepCopy();
            n.StatusText = this.StatusText == null ? null : string.Copy(this.StatusText);
            n.RawData = this.RawData == null ? null : string.Copy(this.RawData);

            return n;
        }

        [XmlIgnore] // Need this for XmlSerializer used for exporting to XML file.
        public long ID { get; set; }

        public int Bay { get; set; }

        /// <summary>
        /// Unique log index that is a combination of <see cref="LogIndex"/>, <see cref="DeviceLogIndex"/> and "D" or "B" for Device/Battery sourced respectively. 
        /// See <seealso cref="LogIndex"/> for details.
        /// </summary>
        public string UniqueLogIndex { get; set; }

        /// <summary>
        /// NOTE: Originally, we only add battery log to db if the log with same index does not exist for given battery.
        ///       It appears that device logs can have same battery log embedded, but with different device log indices obviosly.
        ///       Due to this, many battery logs would be "missing" in joint device log + battery log queries.
        ///       To fix this, we allow battery log index to be non-unique, but batterylog index + device log index should be unique.
        ///       For unique index (uniqueness is only for the current battery, not global), use <see cref="UniqueLogIndex"/>
        /// </summary>
        public long LogIndex { get; set; }

        public string EventCodeText { get; set; }

        [XmlIgnore]
        public InternalDeviceLogEventCodes EventCode { get; set; }

        [XmlElement("EventCode")]
        public int EventCodeRaw { get; set; }
        public DateTime Timestamp { get; set; }

        public long BatterySerialNumberCode { get; set; }

        public string BatterySerialNumber { get; set; }

        public int BatteryFirmwareVersion { get; set; }

        public string DeviceSerialNumber { get; set; }

        public long DeviceLogIndex { get; set; }

        /// <summary>
        /// V
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// A
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// A
        /// </summary>
        public double AverageCurrent { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public int RelativeStateOfCharge { get; set; }

        /// <summary>
        /// Ah
        /// </summary>
        public double RemainingCapacity { get; set; }

        // NOTE: EndOfLifeCapacity was changed to OemIdentifier in InternalDeviceLog>InternalBatteryLog section.
        //       InternalBatteryLog stored inside Battery does not have OemIdentifier but has EndOfLifeCapacity.
        //       For consistency, removed EndOfLifeCapacity from InternalBatteryLog class completely as there might 
        //       be a situation where calculating it would be impossible due to missing data on the Device (Battery Design Capacity).
        public int OemIdentifier { get; set; }

        public int CycleCount { get; set; }

        public BatteryOperatingStatus BatteryOperatingStatus { get; set; }

        /// <summary>
        /// Degree Celcius
        /// </summary>
        public int Temperature { get; set; }

        /// <summary>
        /// Ah
        /// </summary>
        public double CalculatedCapacity { get; set; }

        /// <summary>
        /// Indicates where this log is retrieved from.
        /// </summary>
        public InternalBatteryLogSources Source { get; set; }

        public BatteryStates Status { get; set; }

        public string StatusText { get; set; }

        public string RawData { get; set; }

        #region Navigation Properties

        public Nullable<long> Agent_ID { get; set; }
        public Nullable<long> Battery_ID { get; set; }

        [XmlIgnore] // Need this for XmlSerializer used for exporting to XML file.
        [ForeignKey("Battery_ID")]  //Add this attribute or we cannot update the Battery_ID.
        public virtual Battery Battery { get; set; }

        [ForeignKey("Agent_ID")]  //Add this attribute or we cannot update the Agent_ID.
        [XmlIgnore] // Need this for XmlSerializer used for exporting to XML file.
        public virtual Agent.Agent Agent { get; set; }

        public void ClearNavigationProperties()
        {
            Battery = null;
            Agent = null;
        }

        #endregion
    }
}
