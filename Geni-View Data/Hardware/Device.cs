using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [Index(nameof(SerialNumber), IsUnique = true)]
    public class Device : Abstract.Data
    {
        public Device() { }
        
        public Device(Device device)
        {
            IsDeactivated   = device.IsDeactivated;
            SerialNumber    = device.SerialNumber;
            FirmwareVersion = device.FirmwareVersion;
            Manufacturer    = device.Manufacturer;
            ProductName     = device.ProductName;
            DeviceType      = device.DeviceType;
            Community       = device.Community;
        }

        public Device DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            Device n = (Device)this.MemberwiseClone();
            n.FirmwareVersion = this.FirmwareVersion == null ? null : string.Copy(this.FirmwareVersion);
            n.SerialNumber = this.SerialNumber == null ? null : string.Copy(this.SerialNumber);
            n.Manufacturer = this.Manufacturer == null ? null : string.Copy(this.Manufacturer);
            n.ProductName = this.ProductName == null ? null : string.Copy(this.ProductName);
            n.DevicePath = this.DevicePath == null ? null : string.Copy(this.DevicePath);
            n.AgentDeviceLog = this.AgentDeviceLog == null ? null : this.AgentDeviceLog.DeepCopy();
            n.DeviceSettings = this.DeviceSettings == null ? null : this.DeviceSettings.DeepCopy();
            n.InternalDeviceLog = this.InternalDeviceLog == null ? null : this.InternalDeviceLog.DeepCopy();

            return n;
        }

        public long ID { get; set; }

        public bool IsDeactivated { get; set; }

        [StringLength(10)]
        public string SerialNumber { get; set; }

        public string FirmwareVersion { get; set; }

        #region USB Device

        [NotMapped]
        public bool IsOpen { get; set; }

        public string Manufacturer { get; set; }

        public string ProductName { get; set; }

        public DeviceTypes DeviceType { get; set; }

        [NotMapped]
        public string DevicePath { get; set; }

        #endregion

        // Explicit FK properties mapping to EF6-era column names (Community_ID, Group_ID)
        [Column("Community_ID")]
        public long? CommunityID { get; set; }

        [Column("Group_ID")]
        public long? GroupID { get; set; }

        [NotMapped]
        public AgentDeviceLog AgentDeviceLog { get; set; }

        [NotMapped]
        public DeviceSettings DeviceSettings { get; set; }

        [NotMapped]
        public InternalDeviceLog InternalDeviceLog { get; set; }

        #region Navigation Properties

        public virtual ICollection<AgentDeviceLog> AgentDeviceLogCollection { get; set; }

        public virtual ICollection<DeviceSettings> DeviceSettingsCollection { get; set; }

        public virtual ICollection<InternalDeviceLog> InternalDeviceLogCollection { get; set; }

        public virtual ICollection<DeviceEvent> DeviceEventCollection { get; set; }

        [ForeignKey(nameof(CommunityID))]
        public virtual Community Community { get; set; }

        [ForeignKey(nameof(GroupID))]
        public virtual Group Group { get; set; }

        public void ClearNavigationProperties()
        {
            AgentDeviceLogCollection = null;
            DeviceSettingsCollection = null;
            InternalDeviceLogCollection = null;
            DeviceEventCollection = null;
            Community = null;
            Group = null;
        }

        #endregion

        public override string ToString()
        {
            return $"{DeviceType} {SerialNumber}";
        }
    }
}
