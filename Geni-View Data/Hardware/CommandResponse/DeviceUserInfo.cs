using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DeviceUserInfo : Abstract.Data
    {
        public DeviceUserInfo DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceUserInfo n = (DeviceUserInfo)this.MemberwiseClone();
            n.Name = this.Name == null ? null : string.Copy(this.Name);

            return n;
        }

        public string Name { get; set; }

        public static bool operator ==(DeviceUserInfo a, DeviceUserInfo b)
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

        public static bool operator !=(DeviceUserInfo a, DeviceUserInfo b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            DeviceUserInfo other = obj as DeviceUserInfo;

            if (other == null) return false;

            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return (Name).GetHashCode();
        }
    }
}
