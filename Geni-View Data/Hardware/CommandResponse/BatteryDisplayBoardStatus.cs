using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatteryDisplayBoardStatus : Abstract.Data
    {
        public BatteryDisplayBoardStatus DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatteryDisplayBoardStatus n = (BatteryDisplayBoardStatus)this.MemberwiseClone();

            return n;
        }

        /// <summary>
        /// %
        /// </summary>
        public int PerceivedStateOfCharge { get; set; }

        public int ActiveLEDs { get; set; }

        public bool? IsDisplayFlashing { get; set; }

        public bool? IsDisplayRed { get; set; }

        public FuelGaugeChargeModes? ChargeMode { get; set; }

        public int StatusRaw { get; set; }
    }
}
