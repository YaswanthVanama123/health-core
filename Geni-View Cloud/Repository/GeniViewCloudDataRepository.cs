using GeniView.Cloud.Models;
using GeniView.Data.Agent;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GeniView.Cloud.Repository
{
    public partial class GeniViewCloudDataRepository : DbContext
    {
        public GeniViewCloudDataRepository(DbContextOptions<GeniViewCloudDataRepository> options)
            : base(options)
        {
        }

        // Parameterless constructor for use in repositories until DI is fully wired in Phase 3.
        // TODO Phase 3: remove this and inject via IServiceProvider in all callers.
        public GeniViewCloudDataRepository()
            : base(new DbContextOptionsBuilder<GeniViewCloudDataRepository>().Options)
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("GeniViewCloudDataRepository"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EF6 used PluralizingTableNameConvention (entity class name → plural table name).
            // EF Core uses the DbSet property name as-is. Map each mismatched table explicitly.
            modelBuilder.Entity<AgentDeviceLog>().ToTable("AgentDeviceLogs");
            modelBuilder.Entity<AgentBatteryLog>().ToTable("AgentBatteryLogs");
            modelBuilder.Entity<InternalBatteryLog>().ToTable("InternalBatteryLogs");
            modelBuilder.Entity<InternalDeviceLog>().ToTable("InternalDeviceLogs");
            modelBuilder.Entity<MailServer>().ToTable("MailServers");
            modelBuilder.Entity<DeviceEventNotification>().ToTable("DeviceEventNotifications");
            modelBuilder.Entity<UserActivityHistory>().ToTable("UserActivityHistories");

            // EF6 convention for implicit FK columns: {NavigationPropertyName}_{PKPropertyName}
            // EF Core convention for shadow FKs:     {NavigationPropertyName}{PKPropertyName}
            // Configure column names explicitly for entities that couldn't have [Column] attributes added.
            modelBuilder.Entity<InternalDeviceLog>()
                .HasOne<Device>()
                .WithMany(d => d.InternalDeviceLogCollection)
                .HasForeignKey("Device_ID");

            modelBuilder.Entity<InternalBatteryLog>()
                .HasOne<Battery>()
                .WithMany(b => b.InternalBatteryLogCollection)
                .HasForeignKey("Battery_ID");
        }
    }
}
