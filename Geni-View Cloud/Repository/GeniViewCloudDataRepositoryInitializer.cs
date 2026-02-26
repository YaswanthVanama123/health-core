using GeniView.Cloud.Models;
using GeniView.Cloud.PowerBI;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    // WARNING: CHANGING THIS WILL CAUSE DATA LOSS
    // Called once at startup via Program.cs after EF Core migrations are applied.
    public class GeniViewCloudDataRepositoryInitializer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Seed(GeniViewCloudDataRepository context)
        {
            try
            {
                // Populate device event notification definitions
                foreach (var item in DeviceEventNotification.Seed())
                {
                    if (!context.DeviceEventActionNotifications.Any(x => x.UID == item.UID))
                        context.DeviceEventActionNotifications.Add(item);
                }

                // Populate application update entries
                foreach (var item in ApplicationUpdate.Seed())
                {
                    if (!context.ApplicationUpdates.Any(x => x.AppId == item.AppId))
                        context.ApplicationUpdates.Add(item);
                }

                // Create default agent for G3 flow
                var findAgent = context.Agents.FirstOrDefault(a => a.Name.ToLower() == "default");
                if (findAgent == null)
                {
                    var defaultAgent = Data.Agent.Agent.Default();
                    context.Agents.Add(defaultAgent);
                }

                // Create SQL views for analytics
                try
                {
                    context.Database.ExecuteSqlRaw(StoredProcedures.AgentBatteryLogsWithDurationView);
                    context.Database.ExecuteSqlRaw(StoredProcedures.AgentDeviceLogsWithDurationView);
                    context.Database.ExecuteSqlRaw(StoredProcedures.InternalBatteryLogsWithDurationView);
                    context.Database.ExecuteSqlRaw(StoredProcedures.InternalDeviceLogsWithDurationView);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Cannot create SQL views.");
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Database seeding failed.");
            }
        }
    }
}
