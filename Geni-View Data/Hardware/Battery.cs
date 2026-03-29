using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [Index(nameof(SerialNumberCode), IsUnique = true)]
    public class Battery : Abstract.Data
    {
        public Battery() { }

        public Battery(long serialNumberCode)
        {
            SerialNumberCode = serialNumberCode;
            SerialNumber = DataHelpers.BatterySerialNumberCodeToSerialNumber(serialNumberCode);
        }

        public Battery(Battery battery)
        {
            IsDeactivated              = battery.IsDeactivated;
            DesignCapacity             = battery.DesignCapacity;
            DesignVoltage              = battery.DesignVoltage;
            SerialNumberCode           = battery.SerialNumberCode;
            SerialNumber               = DataHelpers.BatterySerialNumberCodeToSerialNumber(battery.SerialNumberCode.GetValueOrDefault());
            FirmwareVersion            = battery.FirmwareVersion;
            BatteryConfiguration       = battery.BatteryConfiguration;
            BatteryPackFirmwareVersion = battery.BatteryPackFirmwareVersion;
            BatteryChemistry           = battery.BatteryChemistry;
            BatteryTechnology          = battery.BatteryTechnology;
            Community                  = battery.Community;
        }

        public Battery DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            Battery n = (Battery)this.MemberwiseClone();
            n.SerialNumber = this.SerialNumber == null ? null : string.Copy(this.SerialNumber);
            n.BatteryTechnology = this.BatteryTechnology == null ? null : string.Copy(this.BatteryTechnology);
            n.AgentBatteryLog = this.AgentBatteryLog == null ? null : this.AgentBatteryLog.DeepCopy();
            n.BatterySettings = this.BatterySettings == null ? null : this.BatterySettings.DeepCopy();

            return n;
        }

        public long ID { get; set; }

        public bool IsDeactivated { get; set; }

        /// <summary>
        /// Ah
        /// </summary>
        public double DesignCapacity { get; set; }

        /// <summary>
        /// V
        /// </summary>
        public double DesignVoltage { get; set; }

        public long? SerialNumberCode { get; set; }

        public string SerialNumber { get; set; }

        public int FirmwareVersion { get; set; }

        public int BatteryConfiguration { get; set; }

        public int BatteryPackFirmwareVersion { get; set; }

        public int BatteryChemistry { get; set; }

        public string BatteryTechnology { get; set; }

        [NotMapped]
        public AgentBatteryLog AgentBatteryLog { get; set; }

        [NotMapped]
        public BatterySettings BatterySettings { get; set; }

        // Explicit FK properties mapping to EF6-era column names (Community_ID, Group_ID)
        [Column("Community_ID")]
        public long? CommunityID { get; set; }

        [Column("Group_ID")]
        public long? GroupID { get; set; }

        #region Navigation Properties

        public virtual ICollection<AgentBatteryLog> AgentBatteryLogCollection { get; set; }

        public virtual ICollection<BatterySettings> BatterySettingsCollection { get; set; }

        public virtual ICollection<InternalBatteryLog> InternalBatteryLogCollection { get; set; }

        [ForeignKey(nameof(CommunityID))]
        public virtual Community Community { get; set; }

        [ForeignKey(nameof(GroupID))]
        public virtual Group Group { get; set; }

        public void ClearNavigationProperties()
        {
            AgentBatteryLogCollection = null;
            BatterySettingsCollection = null;
            InternalBatteryLogCollection = null;
            Community = null;
            Group = null;
        }

        #endregion

        public override string ToString()
        {
            return $"Battery {SerialNumber}";
        }
    }
}
