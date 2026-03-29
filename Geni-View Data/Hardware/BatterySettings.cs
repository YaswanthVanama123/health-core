using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    public class BatterySettings : Abstract.Data
    {
        public BatterySettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatterySettings n = (BatterySettings)this.MemberwiseClone();
            n.ServiceSettings = this.ServiceSettings == null ? null : this.ServiceSettings.DeepCopy();
            n.DeviceSerialNumber = this.DeviceSerialNumber == null ? null : string.Copy(this.DeviceSerialNumber);

            return n;
        }

        public long ID { get; set; }

        public int Bay { get; set; }

        public BatteryServiceSettings ServiceSettings { get; set; }

        public DateTime? DateOfManufacture { get; set; }

        public int OemIdentifier { get; set; }

        #region Log

        public string DeviceSerialNumber { get; set; }

        public DateTime Timestamp { get; set; }

        #endregion

        #region Navigation Properties

        // Explicit FK properties — EF6 DB columns are Battery_ID and Agent_ID (with underscores)
        public long? Battery_ID { get; set; }

        public long? Agent_ID { get; set; }

        [ForeignKey("Battery_ID")]
        public virtual Battery Battery { get; set; }

        [ForeignKey("Agent_ID")]
        public virtual Agent.Agent Agent { get; set; }

        public void ClearNavigationProperties()
        {
            Battery = null;
            Agent = null;
        }

        #endregion

        #region Equality Check

        public static bool operator ==(BatterySettings a, BatterySettings b)
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

        public static bool operator !=(BatterySettings a, BatterySettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            BatterySettings other = obj as BatterySettings;

            if (other == null) return false;

            return ServiceSettings == other.ServiceSettings &&
                   DateOfManufacture == other.DateOfManufacture &&
                   OemIdentifier == other.OemIdentifier;
        }

        public override int GetHashCode()
        {
            return (ServiceSettings.ToString() + DateOfManufacture + OemIdentifier).GetHashCode();
        }

        #endregion
    }
}
