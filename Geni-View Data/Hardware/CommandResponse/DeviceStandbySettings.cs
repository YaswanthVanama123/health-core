using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DeviceStandbySettings : Abstract.Data
    {
        public DeviceStandbySettings DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceStandbySettings n = (DeviceStandbySettings)this.MemberwiseClone();

            return n;
        }

        /// <summary>
        /// mA
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// Minutes
        /// </summary>
        public int Time { get; set; }

        public bool? StandbyOnExternalPowerInput { get; set; }

        public static bool operator ==(DeviceStandbySettings a, DeviceStandbySettings b)
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

        public static bool operator !=(DeviceStandbySettings a, DeviceStandbySettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            DeviceStandbySettings other = obj as DeviceStandbySettings;

            if (other == null) return false;

            return Current == other.Current &&
                   Time == other.Time &&
                   StandbyOnExternalPowerInput == other.StandbyOnExternalPowerInput;
        }

        public override int GetHashCode()
        {
            return (Current + Time + StandbyOnExternalPowerInput.ToString()).GetHashCode();
        }
    }
}
