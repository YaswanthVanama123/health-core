using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DeviceAlertSettings : Abstract.Data
    {
        public DeviceAlertSettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceAlertSettings n = (DeviceAlertSettings)this.MemberwiseClone();

            return n;
        }

        public DeviceAlertTypes AlertType { get; set; }

        public DisplayModes DisplayMode { get; set; }

        public SystemModes SystemMode { get; set; }

        /// <summary>
        /// 10 second steps.
        /// </summary>
        public int LowBatteryAlertInterval { get; set; }

        /// <summary>
        /// 5 minute steps.
        /// </summary>
        public int LowBatteryAlertLevel { get; set; }

        #region Equality Check

        public static bool operator ==(DeviceAlertSettings a, DeviceAlertSettings b)
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

        public static bool operator !=(DeviceAlertSettings a, DeviceAlertSettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            DeviceAlertSettings other = obj as DeviceAlertSettings;

            if (other == null) return false;

            return AlertType == other.AlertType &&
                   DisplayMode == other.DisplayMode &&
                   SystemMode == other.SystemMode &&
                   LowBatteryAlertInterval == other.LowBatteryAlertInterval &&
                   LowBatteryAlertLevel == other.LowBatteryAlertLevel;
        }

        public override int GetHashCode()
        {
            return (AlertType.ToString() + DisplayMode + SystemMode + LowBatteryAlertInterval + LowBatteryAlertLevel).GetHashCode();
        }

        #endregion
    }
}
