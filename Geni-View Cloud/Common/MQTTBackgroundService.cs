using GeniView.Cloud.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeniView.Cloud.Common
{
    /// <summary>
    /// Hosts the MQTT connection as a proper ASP.NET Core BackgroundService.
    /// Replaces the fire-and-forget Task.Run in Program.cs (Phase 6).
    /// </summary>
    public class MQTTBackgroundService : BackgroundService
    {
        private readonly MQTTHelper _mqtt;
        private readonly ILogger<MQTTBackgroundService> _logger;

        public MQTTBackgroundService(MQTTHelper mqtt, ILogger<MQTTBackgroundService> logger)
        {
            _mqtt   = mqtt;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Short delay so the web server is fully started before connecting.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return; // Shutdown requested before delay finished — exit cleanly.
            }

            try
            {
                _logger.LogInformation("MQTTBackgroundService: connecting to broker.");
                await _mqtt.Connect();
                _logger.LogInformation("MQTTBackgroundService: connected.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTTBackgroundService: initial connection failed.");
            }

            // Keep the service alive until the host shuts down.
            // MQTTHelper handles its own reconnect logic internally on disconnect.
            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => Task.CompletedTask);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MQTTBackgroundService: stopping — disconnecting from broker.");
            try
            {
                _mqtt.Disconnect();
                _mqtt.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MQTTBackgroundService: error during disconnect.");
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
