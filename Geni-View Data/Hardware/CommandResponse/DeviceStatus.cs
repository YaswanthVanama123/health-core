using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace GeniView.Data.Hardware
{
    [ComplexType]
    public class DeviceStatus : Abstract.Data
    {
        public DeviceStatus() { }

        public DeviceStatus(int statusRaw, int temperature)
        {
            StatusRaw = statusRaw;
            Temperature = temperature;

            // Set the corresponding values for other properties.
            Status = 
                StatusRaw.HasValue && 
                EnumHelper.IsFlagsDefined((DeviceStates)StatusRaw) ? (DeviceStates)StatusRaw : DeviceStates.Unknown;

            StatusText = EnumHelper.GetFriendlyText(Status, ",");
        }

        public DeviceStatus DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            DeviceStatus n = (DeviceStatus)this.MemberwiseClone();
            n.StatusText = this.StatusText == null ? null : string.Copy(this.StatusText);

            return n;
        }

        public string StatusText { get; set; }

        [XmlIgnore]
        public DeviceStates Status { get; set; }

        [XmlElement("Status")]
        public int? StatusRaw { get; set; }

        public int Temperature { get; set; }
    }
}
