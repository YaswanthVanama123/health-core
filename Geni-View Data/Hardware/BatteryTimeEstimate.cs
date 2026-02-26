using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class BatteryTimeEstimate : Abstract.Data
    {
        public BatteryTimeEstimate() { }

        public BatteryTimeEstimate(double estimate, BatteryTimeEstimateModes mode)
        {
            Estimate = estimate;
            EstimateMode = mode;

            switch (mode)
            {
                case BatteryTimeEstimateModes.Charging:
                    EstimateDescription = "Time to full charge: " + DataHelpers.TimeSpanToFriendlyString(TimeSpan.FromSeconds(estimate));
                    break;
                case BatteryTimeEstimateModes.Discharging:
                    EstimateDescription = "Time to full discharge: " + DataHelpers.TimeSpanToFriendlyString(TimeSpan.FromSeconds(estimate));
                    break;
                default:
                    break;
            }
        }

        public BatteryTimeEstimate DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            BatteryTimeEstimate n = (BatteryTimeEstimate)this.MemberwiseClone();

            n.EstimateDescription = this.EstimateDescription == null ? null : string.Copy(this.EstimateDescription);

            return n;
        }

        /// <summary>
        /// Returns time in seconds for 1) if charging, time to full charge,
        /// and 2) else, time to full discharge.
        /// </summary>
        public double Estimate { get; set; }

        public BatteryTimeEstimateModes EstimateMode { get; set; }

        public string EstimateDescription { get; set; }
    }
}
