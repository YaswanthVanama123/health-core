using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeniView.Data.Hardware
{
    [Index(nameof(Battery_ID), nameof(Timestamp), Name = "IX_BatteryID_Timestamp")]
    public class AgentBatteryLog : Abstract.Data
    {
        public AgentBatteryLog DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            AgentBatteryLog n = (AgentBatteryLog)this.MemberwiseClone();

            n.OperatingData = this.OperatingData == null ? null : this.OperatingData.DeepCopy();
            n.SlowChangingDataA = this.SlowChangingDataA == null ? null : this.SlowChangingDataA.DeepCopy();
            n.SlowChangingDataB = this.SlowChangingDataB == null ? null : this.SlowChangingDataB.DeepCopy();
            n.TimeEstimate = this.TimeEstimate == null ? null : this.TimeEstimate.DeepCopy();
            n.StatusText = this.StatusText == null ? null : string.Copy(this.StatusText);
            n.DeviceSerialNumber = this.DeviceSerialNumber == null ? null : string.Copy(this.DeviceSerialNumber);

            return n;
        }

        public long ID { get; set; }

        public int Bay { get; set; }

        public BatteryOperatingData OperatingData { get; set; }

        public BatterySlowChangingDataA SlowChangingDataA { get; set; }

        public BatterySlowChangingDataB SlowChangingDataB { get; set; }

        public BatteryTimeEstimate TimeEstimate { get; set; }

        public BatteryStates Status { get; set; }

        public string StatusText { get; set; }

        public long? InternalBatteryLogCount { get; set; }

        #region Log

        public string DeviceSerialNumber { get; set; }

        public DateTime Timestamp { get; set; }

        #endregion

        #region Navigation Properties

        public Nullable<long> Agent_ID { get; set; }

        public Nullable<long> Battery_ID { get; set; }

        [ForeignKey("Battery_ID")]  //Add this attribute or we cannot update the Battery_ID.
        public virtual Battery Battery { get; set; }

        [ForeignKey("Agent_ID")]  //Add this attribute or we cannot update the Agent_ID.
        public virtual Agent.Agent Agent { get; set; }

        public void ClearNavigationProperties()
        {
            Battery = null;
            Agent = null;
        }

        #endregion
    }
}
