using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DeviceLocation
    {
        public DeviceLocation DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceLocation n = (DeviceLocation)this.MemberwiseClone();

            return n;
        }

        public bool IsUnknown { get; set; }
        public double Longitude { get; set; }
        public double Lattitude { get; set; }
    }
}
