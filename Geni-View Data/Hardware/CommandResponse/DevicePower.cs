using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DevicePower : Abstract.Data
    {
        public DevicePower DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DevicePower n = (DevicePower)this.MemberwiseClone();

            return n;
        }

        /// <summary>
        /// V
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// A
        /// </summary>
        public double Current { get; set; }

        public double Power { get { return Voltage * Current; } }
    }
}
