using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DeviceSystemInfo : Abstract.Data
    {
        public DeviceSystemInfo DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceSystemInfo n = (DeviceSystemInfo)this.MemberwiseClone();
            n.SystemPartNumber = this.SystemPartNumber == null ? null : string.Copy(this.SystemPartNumber);
            n.SystemSerialNumber = this.SystemSerialNumber == null ? null : string.Copy(this.SystemSerialNumber);
            n.SlaveSerialNumber = this.SlaveSerialNumber == null ? null : string.Copy(this.SlaveSerialNumber);

            return n;
        }

        public string SystemPartNumber { get; set; }

        public string SystemSerialNumber { get; set; }

        public string SlaveSerialNumber { get; set; }

        public static bool operator ==(DeviceSystemInfo a, DeviceSystemInfo b)
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

        public static bool operator !=(DeviceSystemInfo a, DeviceSystemInfo b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            DeviceSystemInfo other = obj as DeviceSystemInfo;

            if (other == null) return false;

            return SystemPartNumber == other.SystemPartNumber && SlaveSerialNumber == other.SlaveSerialNumber && SystemSerialNumber == other.SystemSerialNumber;
        }

        public override int GetHashCode()
        {
            return (SystemPartNumber + SlaveSerialNumber + SystemSerialNumber).GetHashCode();
        }
    }
}
