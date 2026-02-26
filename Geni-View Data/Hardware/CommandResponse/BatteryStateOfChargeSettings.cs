using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatteryStateOfChargeSettings : Abstract.Data
    {
        public BatteryStateOfChargeSettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatteryStateOfChargeSettings n = (BatteryStateOfChargeSettings)this.MemberwiseClone();

            return n;
        }

        /// <summary>
        /// V
        /// </summary>
        public double VoltageCutOff { get; set; }

        /// <summary>
        /// %
        /// </summary>
        public int PercentageCutOff { get; set; }

        public static bool operator ==(BatteryStateOfChargeSettings a, BatteryStateOfChargeSettings b)
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

        public static bool operator !=(BatteryStateOfChargeSettings a, BatteryStateOfChargeSettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            BatteryStateOfChargeSettings other = obj as BatteryStateOfChargeSettings;

            if (other == null) return false;

            return VoltageCutOff == other.VoltageCutOff &&
                   PercentageCutOff == other.PercentageCutOff;
        }

        public override int GetHashCode()
        {
            return (VoltageCutOff + PercentageCutOff).GetHashCode();
        }
    }
}
