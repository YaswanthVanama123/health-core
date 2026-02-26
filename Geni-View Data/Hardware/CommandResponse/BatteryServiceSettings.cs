using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatteryServiceSettings : Abstract.Data
    {
        public BatteryServiceSettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatteryServiceSettings n = (BatteryServiceSettings)this.MemberwiseClone();

            return n;
        }

        /// <summary>
        /// Cycle count to light "Service" indicator.
        /// </summary>
        public int ServiceInterval { get; set; }

        /// <summary>
        /// When this reaches ServiceInterval, "Service" indicator will light up.
        /// </summary>
        public int ServiceCycleCount { get; set; }


        #region Equality Check

        public static bool operator ==(BatteryServiceSettings a, BatteryServiceSettings b)
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

        public static bool operator !=(BatteryServiceSettings a, BatteryServiceSettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            BatteryServiceSettings other = obj as BatteryServiceSettings;

            if (other == null) return false;

            return ServiceInterval == other.ServiceInterval &&
                   ServiceCycleCount == other.ServiceCycleCount;
        }

        public override int GetHashCode()
        {
            return (ServiceInterval + ServiceCycleCount).GetHashCode();
        }

        #endregion
    }
}
