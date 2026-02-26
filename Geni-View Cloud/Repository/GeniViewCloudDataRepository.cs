using GeniView.Cloud.Models;
using GeniView.Data.Agent;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;

namespace GeniView.Cloud.Repository
{
    public partial class GeniViewCloudDataRepository : DbContext
    {
        public GeniViewCloudDataRepository(DbContextOptions<GeniViewCloudDataRepository> options)
            : base(options)
        {
        }

        public virtual DbSet<Agent> Agents { get; set; }
        public virtual DbSet<Battery> Batteries { get; set; }
        public virtual DbSet<AgentBatteryLog> AgentBatteryLog { get; set; }
        public virtual DbSet<InternalBatteryLog> InternalBatteryLog { get; set; }
        public virtual DbSet<InternalDeviceLog> InternalDeviceLog { get; set; }
        public virtual DbSet<Community> Communities { get; set; }
        public virtual DbSet<AgentDeviceLog> AgentDeviceLog { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DeviceEvent> DeviceEvents { get; set; }
        public virtual DbSet<Group> Groups { get; set; }

        public virtual DbSet<MailServer> MailServer { get; set; }
        public virtual DbSet<DeviceEventNotification> DeviceEventActionNotifications { get; set; }
        public virtual DbSet<ApplicationUpdate> ApplicationUpdates { get; set; }
        public virtual DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public virtual DbSet<UserActivityHistory> UserActivityHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Disable lazy loading proxies — same behaviour as EF6 ProxyCreationEnabled = false
            // Use explicit .Include() calls in repositories for eager loading.
        }
    }
}
