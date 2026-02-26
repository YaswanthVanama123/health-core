using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [Index(nameof(Device_ID), nameof(Timestamp), Name = "IX_Device_ID_Timestamp")]
    public class AgentDeviceLog : Abstract.Data
    {
        public AgentDeviceLog()
        {
            // Set defaults.
            SetBatteriesPresent(Batteries.None);
            SetBatteriesPoweringSystem(Batteries.None);
            SetBatteriesCharging(Batteries.None);
        }


        public AgentDeviceLog DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            AgentDeviceLog n = (AgentDeviceLog)this.MemberwiseClone();

            if (this.BayData != null)
            {
                n.BayData = new List<DeviceBay>();

                foreach (var item in this.BayData)
                {
                    n.BayData.Add(item.DeepCopy());
                }
            }
            else
            {
                n.BayData = null;
            }

            n.Status = this.Status == null ? null : this.Status.DeepCopy();
            n.PowerInput = this.PowerInput == null ? null : this.PowerInput.DeepCopy();
            n.Location = this.Location == null ? null : this.Location.DeepCopy();
            n.TimeEstimate = this.TimeEstimate == null ? null : this.TimeEstimate.DeepCopy();
            n.BatteriesPresentText = this.BatteriesPresentText == null ? null : string.Copy(this.BatteriesPresentText);
            n.BatteriesPoweringSystemText = this.BatteriesPoweringSystemText == null ? null : string.Copy(this.BatteriesPoweringSystemText);
            n.BatteriesChargingText = this.BatteriesChargingText == null ? null : string.Copy(this.BatteriesChargingText);

            return n;
        }

        /// <summary>
        /// Sets the <see cref="BatteriesPresent"/> together with corresponding properties.
        /// </summary>
        /// <param name="value"></param>
        public void SetBatteriesPresent(Batteries value)
        {
            BatteriesPresent = value;
            BatteriesPresentText = EnumHelper.GetFriendlyText(BatteriesPresent, ",");
            BatteriesPresentCount = DataHelpers.CountBits((int)BatteriesPresent);
        }

        /// <summary>
        /// Sets the <see cref="BatteriesPoweringSystem"/> together with corresponding properties.
        /// </summary>
        /// <param name="value"></param>
        public void SetBatteriesPoweringSystem(Batteries value)
        {
            BatteriesPoweringSystem = value;
            BatteriesPoweringSystemText = EnumHelper.GetFriendlyText(BatteriesPoweringSystem, ",");
            BatteriesPoweringSystemCount = DataHelpers.CountBits((int)BatteriesPoweringSystem);
        }

        /// <summary>
        /// Sets the <see cref="BatteriesCharging"/> together with corresponding properties.
        /// </summary>
        /// <param name="value"></param>
        public void SetBatteriesCharging(Batteries value)
        {
            BatteriesCharging = value;
            BatteriesChargingText = EnumHelper.GetFriendlyText(BatteriesCharging, ",");
            BatteriesChargingCount = DataHelpers.CountBits((int)BatteriesCharging);
        }

        public long ID { get; set; }

        [NotMapped]
        public List<DeviceBay> BayData { get; set; }

        public DeviceStatus Status { get; set; }

        public DevicePower PowerInput { get; set; }

        public DevicePower PowerOutput { get; set; }

        public double ChargingPower { get { return Math.Abs(PowerInput.Power - PowerOutput.Power); } }

        public int DeviceCapacity { get; set; }

        public DeviceTimeEstimate TimeEstimate { get; set; }

        public bool? IsExternalPowerInputApplied { get; set; }

        public bool? IsExternalPowerInputNotGood { get; set; }

        public string BatteriesPresentText { get; set; }

        public Batteries BatteriesPresent { get; set; }

        public int BatteriesPresentCount { get; set; }

        public string BatteriesPoweringSystemText { get; set; }

        public Batteries BatteriesPoweringSystem { get; set; }

        public int BatteriesPoweringSystemCount { get; set; }

        public string BatteriesChargingText { get; set; }

        public Batteries BatteriesCharging { get; set; }

        public int BatteriesChargingCount { get; set; }

        public long? InternalDeviceLogCount { get; set; }

        public DeviceLocation Location { get; set; }

        #region Log

        public DateTime Timestamp { get; set; }

        #endregion

        #region Navigation Properties

        public Nullable<long> Agent_ID { get; set; }

        public Nullable<long> Device_ID { get; set; }

        [ForeignKey("Device_ID")]  //Add this attribute or we cannot update the Device_ID.
        public virtual Device Device { get; set; }

        [ForeignKey("Agent_ID")]  //Add this attribute or we cannot update the Agent_ID.
        public virtual Agent.Agent Agent { get; set; }

        public void ClearNavigationProperties()
        {
            Device = null;
            Agent = null;
        }

        #endregion
    }
}
