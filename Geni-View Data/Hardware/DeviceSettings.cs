using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace GeniView.Data.Hardware
{
    public class DeviceSettings : Abstract.Data
    {
        public DeviceSettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceSettings n = (DeviceSettings)this.MemberwiseClone();
            n.UserInformation = this.UserInformation == null ? null : this.UserInformation.DeepCopy();
            n.SystemInformation = this.SystemInformation == null ? null : this.SystemInformation.DeepCopy();
            n.StandbySettings = this.StandbySettings == null ? null : this.StandbySettings.DeepCopy();
            n.AlertSettings = this.AlertSettings == null ? null : this.AlertSettings.DeepCopy();
            n.BatteryStateOfChargeSettings = this.BatteryStateOfChargeSettings == null ? null : this.BatteryStateOfChargeSettings.DeepCopy();
            n.PowerOutputSettings = this.PowerOutputSettings == null ? null : this.PowerOutputSettings.DeepCopy();

            return n;
        }

        [XmlIgnore] // Need this for XmlSerializer used for exporting settings to XML file.
        public long ID { get; set; }

        public int Bays { get; set; }

        public int OemIdentifier { get; set; }

        public ChargingDischargingModes? ChargingMode { get; set; }

        public ChargingDischargingModes? DischargingMode { get; set; }

        public DeviceStandbySettings StandbySettings { get; set; }

        public DeviceAlertSettings AlertSettings { get; set; }

        public bool? BargraphDimming { get; set; }

        public BatteryStateOfChargeSettings BatteryStateOfChargeSettings { get; set; }

        public DeviceUserInfo UserInformation { get; set; }

        public DeviceSystemInfo SystemInformation { get; set; }

        public DevicePowerOutputSettings PowerOutputSettings { get; set; }

        [XmlIgnore] // Need this for XmlSerializer used for exporting settings to XML file.
        public DateTime? DeviceTime { get; set; }

        /// <summary>
        /// Minutes
        /// </summary>
        public int LoggingInterval { get; set; }


        [XmlIgnore] // Need this for XmlSerializer used for exporting settings to XML file.
        public DateTime Timestamp { get; set; }

        #region Navigation Properties

        // Explicit FK properties — EF6 DB columns are Device_ID and Agent_ID (with underscores)
        public long? Device_ID { get; set; }

        public long? Agent_ID { get; set; }

        [XmlIgnore] // Need this for XmlSerializer used for exporting settings to XML file.
        [ForeignKey("Device_ID")]
        public virtual Device Device { get; set; }

        [XmlIgnore] // Need this for XmlSerializer used for exporting settings to XML file.
        [ForeignKey("Agent_ID")]
        public virtual Agent.Agent Agent { get; set; }

        public void ClearNavigationProperties()
        {
            Device = null;
            Agent = null;
        }

        #endregion

        #region Equality Check

        public static bool operator ==(DeviceSettings a, DeviceSettings b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(DeviceSettings a, DeviceSettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            DeviceSettings other = obj as DeviceSettings;

            if (other == null) return false;

            return Bays == other.Bays &&
                   OemIdentifier == other.OemIdentifier &&
                   ChargingMode == other.ChargingMode &&
                   DischargingMode == other.DischargingMode &&
                   StandbySettings == other.StandbySettings &&
                   AlertSettings == other.AlertSettings &&
                   BargraphDimming == other.BargraphDimming &&
                   BatteryStateOfChargeSettings == other.BatteryStateOfChargeSettings &&
                   UserInformation == other.UserInformation &&
                   SystemInformation == other.SystemInformation &&
                   PowerOutputSettings == other.PowerOutputSettings &&
                   //DeviceTime == other.DeviceTime && // TODO: Need to ignore this as it changes every time data is retrieved from Device.
                   LoggingInterval == other.LoggingInterval;
        }

        public override int GetHashCode()
        {
            return (Bays + OemIdentifier + ChargingMode.ToString() + DischargingMode.ToString() + StandbySettings + AlertSettings + BargraphDimming +
                BatteryStateOfChargeSettings + UserInformation + SystemInformation + PowerOutputSettings + /* DeviceTime + */ LoggingInterval).GetHashCode();
        }

        #endregion
    }
}
