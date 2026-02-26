using Microsoft.EntityFrameworkCore;
using NLog;
using System;

namespace GeniView.Cloud.Repository
{
    // Ensures the Hangfire SQL database exists at startup.
    // Called once from Program.cs before Hangfire services start.
    public class HangfireRepositoryInitializer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void EnsureCreated(HangfireRepository context)
        {
            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Hangfire database creation failed.");
            }
        }
    }
}
