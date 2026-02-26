using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DeviceTimeEstimate : Abstract.Data
    {
        public DeviceTimeEstimate() { }

        public DeviceTimeEstimate(double estimate, DeviceTimeEstimateModes mode)
        {
            Estimate = estimate;
            EstimateMode = mode;

            switch (mode)
            {
                case DeviceTimeEstimateModes.NoBattery:
                    EstimateDescription = "No battery attached";
                    break;
                case DeviceTimeEstimateModes.NoPowerOutput:
                    EstimateDescription = "No power output";
                    break;
                case DeviceTimeEstimateModes.OnBatteryPower:
                    EstimateDescription = "Remaining runtime: " + DataHelpers.TimeSpanToFriendlyString(TimeSpan.FromSeconds(estimate));
                    break;
                case DeviceTimeEstimateModes.OnExternalPower:
                    EstimateDescription = "Available runtime: " + DataHelpers.TimeSpanToFriendlyString(TimeSpan.FromSeconds(estimate));
                    break;
                default:
                    break;
            }
        }

        public DeviceTimeEstimate DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceTimeEstimate n = (DeviceTimeEstimate)this.MemberwiseClone();

            n.EstimateDescription = this.EstimateDescription == null ? null : string.Copy(this.EstimateDescription);

            return n;
        }

        /// <summary>
        /// Returns time in seconds for device available runtime 
        /// depending on whether device has battery, device is on battery power, or external power.
        /// </summary>
        public double Estimate { get; set; }

        public DeviceTimeEstimateModes EstimateMode { get; set; }

        public string EstimateDescription { get; set; }
    }
}
