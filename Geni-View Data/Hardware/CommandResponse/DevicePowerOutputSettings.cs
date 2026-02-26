using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DevicePowerOutputSettings : Abstract.Data
    {
        public DevicePowerOutputSettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DevicePowerOutputSettings n = (DevicePowerOutputSettings)this.MemberwiseClone();

            return n;
        }

        /// <summary>
        /// V
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// W
        /// </summary>
        public long Power { get; set; }

        public short Trim { get; set; }

        public int PotValue { get; set; }

        [NotMapped]
        public bool SaveToDigitalPot { get; set; }

        public bool? IsDCConverterFitted { get; set; }

        public static bool operator ==(DevicePowerOutputSettings a, DevicePowerOutputSettings b)
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

        public static bool operator !=(DevicePowerOutputSettings a, DevicePowerOutputSettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            DevicePowerOutputSettings other = obj as DevicePowerOutputSettings;

            if (other == null) return false;

            return Voltage == other.Voltage &&
                   Power == other.Power &&
                   Trim == other.Trim &&
                   PotValue == other.PotValue &&
                   SaveToDigitalPot == other.SaveToDigitalPot &&
                   IsDCConverterFitted == other.IsDCConverterFitted;
        }

        public override int GetHashCode()
        {
            return (Voltage + Power + Trim + PotValue + SaveToDigitalPot.ToString() + IsDCConverterFitted.ToString()).GetHashCode();
        }
    }
}
