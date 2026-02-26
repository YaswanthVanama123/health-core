using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Agent;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeniView.Cloud.Services
{
    public class GeniViewDeviceDataPublishService : IDeviceDataPublishService
    {
        public PublishedAgentDataInformation GetPublishedAgentDeviceLogsInformation(string serialNumber, DateTime fromDate, DateTime toDate)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedAgentDeviceLogsInformation)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var query = db.Entry(originalDevice).Collection(x => x.AgentDeviceLogCollection)
                                                            .Query()
                                                            .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        //var query = originalDevice.DeviceDataCollection.Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        int dataCount = query.Count();

                        DateTime? latestDataDate = null;
                        if (dataCount > 0)
                            latestDataDate = DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc); // Set kind to UTC without changing value.

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedAgentDeviceLogsInformation)} called with Device SN: {serialNumber} completed.");

                        return new PublishedAgentDataInformation()
                        {
                            Count = dataCount,
                            LatestEntryDate = latestDataDate
                        };
                    }
                    else
                    {
                        return new PublishedAgentDataInformation();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PublishedAgentDataInformation GetPublishedAgentBatteryLogsInformation(long serialNumberCode, DateTime fromDate, DateTime toDate)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedAgentBatteryLogsInformation)} called with Battery SN: {serialNumberCode}.");

            try
            {
                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Battery originalBattery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);

                    if (originalBattery != null)
                    {
                        var query = db.Entry(originalBattery).Collection(x => x.AgentBatteryLogCollection)
                                                             .Query()
                                                             .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        // var query = originalBattery.BatteryDataCollection.Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        int dataCount = query.Count();

                        DateTime? latestDataDate = null;
                        if (dataCount > 0)
                            latestDataDate = DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc); // Set kind to UTC without changing value.

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedAgentBatteryLogsInformation)} called with Battery SN: {serialNumberCode} completed.");

                        return new PublishedAgentDataInformation()
                        {
                            Count = dataCount,
                            LatestEntryDate = latestDataDate
                        };
                    }
                    else
                    {
                        return new PublishedAgentDataInformation();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PublishedSettingsInformation GetPublishedDeviceSettingsInformation(string serialNumber, DateTime fromDate, DateTime toDate)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedDeviceSettingsInformation)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var query = db.Entry(originalDevice).Collection(x => x.DeviceSettingsCollection)
                                                            .Query()
                                                            .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        //var query = originalDevice.DeviceSettingsCollection.Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        int settingsCount = query.Count();

                        DateTime? latestSettingsDate = null;
                        if (settingsCount > 0)
                            latestSettingsDate = DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc); // Set kind to UTC without changing value.

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedDeviceSettingsInformation)} called with Device SN: {serialNumber} completed.");

                        return new PublishedSettingsInformation()
                        {
                            Count = settingsCount,
                            LatestEntryDate = latestSettingsDate
                        };
                    }
                    else
                    {
                        return new PublishedSettingsInformation();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PublishedSettingsInformation GetPublishedBatterySettingsInformation(long serialNumberCode, DateTime fromDate, DateTime toDate)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedBatterySettingsInformation)} called with Battery SN: {serialNumberCode}.");

            try
            {
                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Battery originalBattery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);

                    if (originalBattery != null)
                    {
                        var query = db.Entry(originalBattery).Collection(x => x.BatterySettingsCollection)
                                                             .Query()
                                                             .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        //var query = originalBattery.BatterySettingsCollection.Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        int settingsCount = query.Count();

                        DateTime? latestSettingsDate = null;
                        if (settingsCount > 0)
                            latestSettingsDate = DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc); // Set kind to UTC without changing value.

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedBatterySettingsInformation)} called with Battery SN: {serialNumberCode} completed.");

                        return new PublishedSettingsInformation()
                        {
                            Count = settingsCount,
                            LatestEntryDate = latestSettingsDate
                        };
                    }
                    else
                    {
                        return new PublishedSettingsInformation();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PublishedInternalLogsInformation GetPublishedInternalDeviceLogsInformation(string serialNumber, long fromLogIndex, long toLogIndex)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedInternalDeviceLogsInformation)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var query = db.Entry(originalDevice).Collection(x => x.InternalDeviceLogCollection)
                                                            .Query()
                                                            .Where(x => x.LogIndex >= fromLogIndex && x.LogIndex <= toLogIndex);

                        //var query = originalDevice.DeviceLogCollection.Where(x => x.LogIndex >= fromLogIndex && x.LogIndex <= toLogIndex);

                        int logCount = query.Count();

                        long? latestLogIndex = null;
                        if (logCount > 0)
                            latestLogIndex = query.Max(x => x.LogIndex);

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedInternalDeviceLogsInformation)} called with Device SN: {serialNumber} completed.");

                        return new PublishedInternalLogsInformation()
                        {
                            Count = logCount,
                            LatestEntryIndex = latestLogIndex
                        };
                    }
                    else
                    {
                        return new PublishedInternalLogsInformation();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PublishedInternalLogsInformation GetPublishedInternalBatteryLogsInformation(long serialNumberCode, long fromLogIndex, long toLogIndex)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedInternalBatteryLogsInformation)} called with Battery SN: {serialNumberCode}.");

            try
            {
                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Battery originalBattery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);

                    if (originalBattery != null)
                    {
                        var query = db.Entry(originalBattery).Collection(x => x.InternalBatteryLogCollection)
                                                             .Query()
                                                             .Where(x => x.LogIndex >= fromLogIndex && x.LogIndex <= toLogIndex);

                        //var query = originalBattery.BatteryLogCollection.Where(x => x.LogIndex >= fromLogIndex && x.LogIndex <= toLogIndex);

                        int logCount = query.Count();

                        long? latestLogIndex = null;
                        if (logCount > 0)
                            latestLogIndex = query.Max(x => x.LogIndex);

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedInternalBatteryLogsInformation)} called with Battery SN: {serialNumberCode} completed.");

                        return new PublishedInternalLogsInformation()
                        {
                            Count = logCount,
                            LatestEntryIndex = latestLogIndex
                        };
                    }
                    else
                    {
                        return new PublishedInternalLogsInformation();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PublishedEventsInformation GetPublishedDeviceEventsInformation(string serialNumber, DateTime fromDate, DateTime toDate)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedDeviceEventsInformation)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var query = db.Entry(originalDevice).Collection(x => x.DeviceEventCollection)
                                                            .Query()
                                                            .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        //var query = originalDevice.GetDeviceEventsInformation.Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

                        int dataCount = query.Count();

                        DateTime? latestEventDate = null;
                        if (dataCount > 0)
                            latestEventDate = DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc); // Set kind to UTC without changing value.

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetPublishedDeviceEventsInformation)} called with Device SN: {serialNumber} completed.");

                        return new PublishedEventsInformation()
                        {
                            Count = dataCount,
                            LatestEntryDate = latestEventDate
                        };
                    }
                    else
                    {
                        return new PublishedEventsInformation();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DateTime> GetMissingAgentDeviceLogDates(string serialNumber, IEnumerable<DateTime> availableDataDates)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingAgentDeviceLogDates)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                if (availableDataDates == null)
                    throw new ArgumentNullException("List of available data entry dates from the Agent must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var existingDataDates = db.Entry(originalDevice).Collection(x => x.AgentDeviceLogCollection)
                                                                        .Query()
                                                                        .Where(x => availableDataDates.Contains(x.Timestamp))
                                                                        .Select(x => x.Timestamp);
                        var missingDataDates = availableDataDates.Except(existingDataDates).ToList();
                        //var missingDataDates = availableDataDates.Where(x => originalDevice.DeviceDataCollection.Any(y => y.Timestamp == x) == false).ToList();

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingAgentDeviceLogDates)} called with Device SN: {serialNumber} completed.");

                        return missingDataDates;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DateTime> GetMissingAgentBatteryLogDates(long serialNumberCode, IEnumerable<DateTime> availableDataDates)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingAgentBatteryLogDates)} called with Battery SN: {serialNumberCode}.");

            try
            {
                if (availableDataDates == null)
                    throw new ArgumentNullException("List of available data entry dates from the Agent must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Battery originalBattery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);

                    if (originalBattery != null)
                    {
                        var existingDataDates = db.Entry(originalBattery).Collection(x => x.AgentBatteryLogCollection)
                                                                         .Query()
                                                                         .Where(x => availableDataDates.Contains(x.Timestamp))
                                                                         .Select(x => x.Timestamp);
                        var missingDataDates = availableDataDates.Except(existingDataDates).ToList();

                        // var missingDataDates = availableDataDates.Where(x => originalBattery.BatteryDataCollection.Any(y => y.Timestamp == x) == false).ToList();

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingAgentBatteryLogDates)} called with Battery SN: {serialNumberCode} completed.");

                        return missingDataDates;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DateTime> GetMissingDeviceSettingsDates(string serialNumber, IEnumerable<DateTime> availableSettingsDates)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingDeviceSettingsDates)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                if (availableSettingsDates == null)
                    throw new ArgumentNullException("List of available settings dates from the Agent must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var existingSettingsDates = db.Entry(originalDevice).Collection(x => x.DeviceSettingsCollection)
                                                                            .Query()
                                                                            .Where(x => availableSettingsDates.Contains(x.Timestamp))
                                                                            .Select(x => x.Timestamp);
                        var missingSettingsDates = availableSettingsDates.Except(existingSettingsDates).ToList();
                        //var missingSettingsDates = availableSettingsDates.Where(x => originalDevice.DeviceSettingsCollection.Any(y => y.Timestamp == x) == false).ToList();

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingDeviceSettingsDates)} called with Device SN: {serialNumber} completed.");

                        return missingSettingsDates;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DateTime> GetMissingBatterySettingsDates(long serialNumberCode, IEnumerable<DateTime> availableSettingsDates)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingBatterySettingsDates)} called with Battery SN: {serialNumberCode}.");

            try
            {
                if (availableSettingsDates == null)
                    throw new ArgumentNullException("List of available settings dates from the Agent must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Battery originalBattery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);

                    if (originalBattery != null)
                    {
                        var existingSettingsDates = db.Entry(originalBattery).Collection(x => x.BatterySettingsCollection)
                                                                             .Query()
                                                                             .Where(x => availableSettingsDates.Contains(x.Timestamp))
                                                                             .Select(x => x.Timestamp);
                        var missingSettingsDates = availableSettingsDates.Except(existingSettingsDates).ToList();

                        //var missingSettingsDates = availableSettingsDates.Where(x => originalBattery.BatterySettingsCollection.Any(y => y.Timestamp == x) == false).ToList();

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingBatterySettingsDates)} called with Battery SN: {serialNumberCode} completed.");

                        return missingSettingsDates;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<long> GetMissingInternalDeviceLogIndices(string serialNumber, IEnumerable<long> availableLogIndices)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingInternalDeviceLogIndices)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                if (availableLogIndices == null)
                    throw new ArgumentNullException("List of available log indices from the Agent must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var existingLogIndices = db.Entry(originalDevice).Collection(x => x.InternalDeviceLogCollection)
                                                                         .Query()
                                                                         .Where(x => availableLogIndices.Contains(x.LogIndex))
                                                                         .Select(x => x.LogIndex);
                        var missingLogIndices = availableLogIndices.Except(existingLogIndices).ToList();
                        // var missingLogIndices = availableLogIndices.Where(x => originalDevice.DeviceLogCollection.Any(y => y.LogIndex == x) == false).ToList();

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingInternalDeviceLogIndices)} called with Device SN: {serialNumber} completed.");

                        return missingLogIndices;
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Because <see cref="InternalBatteryLog.LogIndex"/> is not unique (see <see cref="InternalBatteryLog.LogIndex"/>), use <see cref="InternalBatteryLog.UniqueLogIndex"/>
        /// wherever uniqueness is required. Here, if former were used, it would cause many missing battery logs due to former not being unique.
        /// </summary>
        public IEnumerable<string> GetMissingInternalBatteryLogUniqueIndices(long serialNumberCode, IEnumerable<string> availableUniqueLogIndices)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingInternalBatteryLogUniqueIndices)} called with Battery SN: {serialNumberCode}.");

            try
            {
                if (availableUniqueLogIndices == null)
                    throw new ArgumentNullException("List of available log indices from the Agent must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Battery originalBattery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);

                    if (originalBattery != null)
                    {
                        var existingLogIndices = db.Entry(originalBattery).Collection(x => x.InternalBatteryLogCollection)
                                                                          .Query()
                                                                          .Where(x => availableUniqueLogIndices.Contains(x.UniqueLogIndex))
                                                                          .Select(x => x.UniqueLogIndex);
                        var missingLogIndices = availableUniqueLogIndices.Except(existingLogIndices).ToList();

                        //var missingLogIndices = availableLogIndices.Where(x => originalBattery.BatteryLogCollection.Any(y => y.LogIndex == x) == false).ToList();

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingInternalBatteryLogUniqueIndices)} called with Battery SN: {serialNumberCode} completed.");

                        return missingLogIndices;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<DateTime> GetMissingDeviceEventDates(string serialNumber, IEnumerable<DateTime> availableEventDates)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingDeviceEventDates)} called with Device SN: {serialNumber}.");

            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                    throw new ArgumentNullException("Device serial number must be provided.");

                if (availableEventDates == null)
                    throw new ArgumentNullException("List of available event entry dates from the Agent must be provided.");

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // For performance reasons, get only what we need.
                    

                    Device originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);

                    if (originalDevice != null)
                    {
                        var existingEventDates = db.Entry(originalDevice).Collection(x => x.DeviceEventCollection)
                                                                         .Query()
                                                                         .Where(x => availableEventDates.Contains(x.Timestamp))
                                                                         .Select(x => x.Timestamp);
                        var missingEventDates = availableEventDates.Except(existingEventDates).ToList();
                        //var missingEventDates = availableEventDates.Where(x => originalDevice.DeviceEventsCollection.Any(y => y.Timestamp == x) == false).ToList();

                        System.Diagnostics.Debug.WriteLine($"{nameof(GetMissingDeviceEventDates)} called with Device SN: {serialNumber} completed.");

                        return missingEventDates;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void EnsureAgentExists(Agent agent)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(EnsureAgentExists)} called with Agent ID: {agent.AgentID}.");

            if (agent == null)
                throw new ArgumentNullException("Agent information must be provided.");

            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                // If agent is not yet in db, add it.
                var originalAgent = db.Agents.FirstOrDefault(x => x.AgentID == agent.AgentID);

                if (originalAgent == null)
                {
                    db.Agents.Add(agent);
                }
                else
                {
                    // Update all relevant properties.
                    originalAgent.Name = agent.Name;
                    originalAgent.Description = agent.Description;
                    originalAgent.OperatingSystem = agent.OperatingSystem;
                    originalAgent.AgentVersion = agent.AgentVersion;
                    originalAgent.AgentAddress = agent.AgentAddress;

                    agent = originalAgent;
                }

                // Update last seen date.
                agent.Timestamp = DateTime.UtcNow;

                // TODO Phase 7: obtain client IP from HttpContext.Connection.RemoteIpAddress in REST API controller.
                // OperationContext (WCF) is not available in ASP.NET Core.
                agent.AgentAddress = "0.0.0.0";

                db.SaveChanges();
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(EnsureAgentExists)} called with Agent ID: {agent.AgentID} completed.");
        }

        private void EnsureDeviceExists(Device device)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(EnsureDeviceExists)} called with Device SN: {device.SerialNumber}.");

            if (device == null)
                throw new ArgumentNullException("Device information must be provided.");

            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                var originalDevice = db.Devices.FirstOrDefault(d => d.SerialNumber == device.SerialNumber);

                if (originalDevice != null)
                {
                    // Update all relevant properties.
                    originalDevice.FirmwareVersion = device.FirmwareVersion;
                    originalDevice.Manufacturer = device.Manufacturer;
                    originalDevice.ProductName = device.ProductName;
                    originalDevice.DeviceType = device.DeviceType;
                }
                else
                {
                    // Clear navigation properties.
                    device.ClearNavigationProperties();

                    db.Devices.Add(device);
                }

                db.SaveChanges();
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(EnsureDeviceExists)} called with Device SN: {device.SerialNumber} completed.");
        }

        private void EnsureBatteryExists(Battery battery)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(EnsureBatteryExists)} called with Battery SN: {battery.SerialNumber}.");

            if (battery == null)
                throw new ArgumentNullException("Battery information must be provided.");

            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                // Battery data
                Battery originalBattery = db.Batteries.FirstOrDefault(b => b.SerialNumber == battery.SerialNumber);

                if (originalBattery != null)
                {
                    // Update all relevant properties.
                    originalBattery.DesignCapacity = battery.DesignCapacity;
                    originalBattery.DesignVoltage = battery.DesignVoltage;
                    originalBattery.FirmwareVersion = battery.FirmwareVersion;
                    originalBattery.BatteryConfiguration = battery.BatteryConfiguration;
                    originalBattery.BatteryPackFirmwareVersion = battery.BatteryPackFirmwareVersion;
                    originalBattery.BatteryChemistry = battery.BatteryChemistry;
                    originalBattery.BatteryTechnology = battery.BatteryTechnology;
                }
                else
                {
                    // Clear navigation properties.
                    battery.ClearNavigationProperties();

                    db.Batteries.Add(battery);
                }

                db.SaveChanges();
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(EnsureBatteryExists)} called with Battery SN: {battery.SerialNumber} completed.");
        }

        public async void PublishDeviceData(Device device, IEnumerable<Battery> batteries, Agent agent)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(PublishDeviceData)} called with Device SN: {device.SerialNumber}.");

            var devicejson = JsonConvert.SerializeObject(device);
            var batteryjson = JsonConvert.SerializeObject(batteries);

            //var devicejson = JsonConvert.SerializeObject(device,
            //    Newtonsoft.Json.Formatting.None,
            //    new JsonSerializerSettings
            //    {
            //        NullValueHandling = NullValueHandling.Ignore
            //    });
            //var batteryjson = JsonConvert.SerializeObject(batteries,
            //                Newtonsoft.Json.Formatting.None,
            //                new JsonSerializerSettings
            //                {
            //                    NullValueHandling = NullValueHandling.Ignore
            //                });

            try
            {
                // Ensure Device, Batteries, and Agent exist before the ops below.
                // This is to reduce concurrency problems when data is published using multiple threads.
               var t1 = OperationWithBasicRetryAsync(async () => EnsureAgentExists(agent));
                var t2 = OperationWithBasicRetryAsync(async () => EnsureDeviceExists(device));
                var t3 = OperationWithBasicRetryAsync(async () =>
                {
                    if (batteries != null)
                    {
                        foreach (var battery in batteries)
                        {
                            EnsureBatteryExists(battery);
                        }
                    }
                });

                await Task.WhenAll(t1, t2, t3);

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // Note : When inserting new item, looks like EF either inserts new items for each navigation property
                    //        or fails to save due to navigation property coming from outside context without tracking.
                    //        Getting the latest navigation properties within the current context appears to solve problem....

                    // Obtain the agent from DB, it MUST exist before this point.
                    var originalAgent = db.Agents.First(x => x.AgentID == agent.AgentID);


                    // For performance, use explicit loading.
                    // TODO: Check if automatic change detection improves performance for loop inserts.
                    
                    // This needs to be re-enabled before saving. Should help in loops.
                    db.ChangeTracker.AutoDetectChangesEnabled = false;

                    // Device event mail notification.
                    // Call mail notification after adding events to DB. Mail notifier expects these to be in DB to mark handled.
                    DeviceEventMailNotifier mailNotifier = new Models.DeviceEventMailNotifier();
                    List<DeviceEvent> eventsForMailNotification = new List<DeviceEvent>();

                    // Obtain the device from DB, it MUST exist before this point.
                    var originalDevice = db.Devices.First(d => d.SerialNumber == device.SerialNumber);

                    // Add device data.
                    if (device.AgentDeviceLogCollection != null)
                    {
                        // Insert appears to be loading the whole collection which is problematic for large data.
                        // Load only the data matching what is to be inserted for PK violation (existing data) checks, then insert what is new.
                        var newDataDates = device.AgentDeviceLogCollection.Select(x => x.Timestamp);
                        originalDevice.AgentDeviceLogCollection = db.Entry(originalDevice).Collection(x => x.AgentDeviceLogCollection)
                                                                                          .Query()
                                                                                          .Where(x => newDataDates.Contains(x.Timestamp))
                                                                                          .ToList();

                        foreach (var data in device.AgentDeviceLogCollection)
                        {
                            if (originalDevice.AgentDeviceLogCollection.Any(x => x.Timestamp == data.Timestamp) == false)
                            {
                                // Need to clear all navigation properties.
                                data.ClearNavigationProperties();

                                // Add agent info for each data entry.
                                data.Agent = originalAgent;
                                originalDevice.AgentDeviceLogCollection.Add(data);
                            }
                            else
                            {

                            }
                        }
                    }

                    // Add device settings.
                    if (device.DeviceSettingsCollection != null)
                    {
                        // Insert appears to be loading the whole collection which is problematic for large data.
                        // Load only the data matching what is to be inserted for PK violation (existing data) checks, then insert what is new.
                        var newDataDates = device.DeviceSettingsCollection.Select(x => x.Timestamp);
                        originalDevice.DeviceSettingsCollection = db.Entry(originalDevice).Collection(x => x.DeviceSettingsCollection)
                                                                                          .Query()
                                                                                          .Where(x => newDataDates.Contains(x.Timestamp))
                                                                                          .ToList();

                        foreach (var settings in device.DeviceSettingsCollection)
                        {
                            if (originalDevice.DeviceSettingsCollection.Any(x => x.Timestamp == settings.Timestamp) == false)
                            {
                                // Need to clear all navigation properties.
                                settings.ClearNavigationProperties();

                                // Add agent info for each data entry.
                                settings.Agent = originalAgent;

                                originalDevice.DeviceSettingsCollection.Add(settings);
                            }
                        }
                    }

                    // Add device logs.
                    if (device.InternalDeviceLogCollection != null)
                    {
                        // Insert appears to be loading the whole collection which is problematic for large data.
                        // Load only the data matching what is to be inserted for PK violation (existing data) checks, then insert what is new.
                        var newDataIndices = device.InternalDeviceLogCollection.Select(x => x.LogIndex);
                        originalDevice.InternalDeviceLogCollection = db.Entry(originalDevice).Collection(x => x.InternalDeviceLogCollection)
                                                                                             .Query()
                                                                                             .Where(x => newDataIndices.Contains(x.LogIndex))
                                                                                             .ToList();

                        foreach (var log in device.InternalDeviceLogCollection)
                        {
                            if (originalDevice.InternalDeviceLogCollection.Any(x => x.LogIndex == log.LogIndex) == false)
                            {
                                // Need to clear all navigation properties.
                                log.ClearNavigationProperties();

                                // Add agent info for each data entry.
                                log.Agent = originalAgent;

                                originalDevice.InternalDeviceLogCollection.Add(log);
                            }
                        }
                    }

                    // Add device events.
                    if (device.DeviceEventCollection != null)
                    {
                        // Insert appears to be loading the whole collection which is problematic for large data.
                        // Load only the data matching what is to be inserted for PK violation (existing data) checks, then insert what is new.
                        var newEventDates = device.DeviceEventCollection.Select(x => x.Timestamp);
                        originalDevice.DeviceEventCollection = db.Entry(originalDevice).Collection(x => x.DeviceEventCollection)
                                                                                       .Query()
                                                                                       .Where(x => newEventDates.Contains(x.Timestamp))
                                                                                       .ToList();

                        foreach (var deviceEvent in device.DeviceEventCollection)
                        {
                            if (originalDevice.DeviceEventCollection.Any(x => x.Timestamp == deviceEvent.Timestamp) == false)
                            {
                                // Need to clear all navigation properties.
                                deviceEvent.ClearNavigationProperties();

                                // Add agent info for each data entry.
                                deviceEvent.Agent = originalAgent;
                                originalDevice.DeviceEventCollection.Add(deviceEvent);

                                // Add event to mail notification list.
                                eventsForMailNotification.Add(deviceEvent);
                            }
                        }
                    }

                    // Add batteries. Note it's not saved since we need to make sure all info are available at the same time.
                    if (batteries != null)
                    {
                        foreach (var battery in batteries)
                        {
                            PublishBatteryData(battery, originalAgent, db);
                        }
                    }

                    // This needs to be re-enabled before saving. Should help in loops.
                    db.ChangeTracker.AutoDetectChangesEnabled = true;

                    // Commit to DB.
                    db.SaveChanges();

                    // Send mail for events. Sort by oldest to newest first.
                    await mailNotifier.SendMessageAsync(eventsForMailNotification.OrderBy(x => x.Timestamp).AsEnumerable());
                }
            }
            catch (Exception)
            {
                throw;
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(PublishDeviceData)} called with Device SN: {device.SerialNumber} completed.");
        }

        public async void PublishBatteryData(Battery battery, Agent agent)
        {
            try
            {
                // Ensure Battery and Agent exist before the ops below.
                // This is to reduce concurrency problems when data is published using multiple threads.
                var t1 = OperationWithBasicRetryAsync(async () => EnsureAgentExists(agent));
                var t2 = OperationWithBasicRetryAsync(async () => EnsureBatteryExists(battery));

                await Task.WhenAll(t1, t2);

                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    // Note : When inserting new item, looks like EF either inserts new items for each navigation property
                    //        or fails to save due to navigation property coming from outside context without tracking.
                    //        Getting the latest navigation properties within the current context appears to solve problem....

                    // Obtain the agent from DB, it MUST exist before this point.
                    var originalAgent = db.Agents.First(x => x.AgentID == agent.AgentID);

                    // For performance, use explicit loading.
                    // TODO: Check if automatic change detection improves performance for loop inserts.
                    
                    // This needs to be re-enabled before saving. Should help in loops.
                    db.ChangeTracker.AutoDetectChangesEnabled = false;

                    // Add to DB, not saved since we need transaction for other calls.
                    PublishBatteryData(battery, originalAgent, db);

                    // This needs to be re-enabled before saving. Should help in loops.
                    db.ChangeTracker.AutoDetectChangesEnabled = true;
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void PublishBatteryData(Battery battery, Agent agent, GeniViewCloudDataRepository db)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(PublishBatteryData)} called with Battery SN: {battery.SerialNumber}.");

            //var batteryjson = JsonConvert.SerializeObject(battery);
            //var agentjson = JsonConvert.SerializeObject(agent);

            // Obtain the device from DB, it MUST exist before this point.
            Battery originalBattery = db.Batteries.First(b => b.SerialNumber == battery.SerialNumber);

            // Add battery data.
            if (battery.AgentBatteryLogCollection != null)
            {
                // Insert appears to be loading the whole collection which is problematic for large data.
                // Load only the data matching what is to be inserted for PK violation (existing data) checks, then insert what is new.
                var newDataDates = battery.AgentBatteryLogCollection.Select(x => x.Timestamp);
                originalBattery.AgentBatteryLogCollection = db.Entry(originalBattery).Collection(x => x.AgentBatteryLogCollection)
                                                                                     .Query()
                                                                                     .Where(x => newDataDates.Contains(x.Timestamp))
                                                                                     .ToList();

                foreach (var data in battery.AgentBatteryLogCollection)
                {
                    if (originalBattery.AgentBatteryLogCollection.Any(x => x.Timestamp == data.Timestamp) == false)
                    {
                        // Need to clear all navigation properties.
                        data.ClearNavigationProperties();

                        // Add agent info for each data entry.
                        data.Agent = agent;
                        originalBattery.AgentBatteryLogCollection.Add(data);
                    }
                }
            }

            // Add battery settings.
            if (battery.BatterySettingsCollection != null)
            {
                // Insert appears to be loading the whole collection which is problematic for large data.
                // Load only the data matching what is to be inserted for PK violation (existing data) checks, then insert what is new.
                var newDataDates = battery.BatterySettingsCollection.Select(x => x.Timestamp);
                originalBattery.BatterySettingsCollection = db.Entry(originalBattery).Collection(x => x.BatterySettingsCollection)
                                                                                     .Query()
                                                                                     .Where(x => newDataDates.Contains(x.Timestamp))
                                                                                     .ToList();

                foreach (var settings in battery.BatterySettingsCollection)
                {
                    if (originalBattery.BatterySettingsCollection.Any(x => x.Timestamp == settings.Timestamp) == false)
                    {
                        // Need to clear all navigation properties.
                        settings.ClearNavigationProperties();

                        // Add agent info for each data entry.
                        settings.Agent = agent;

                        originalBattery.BatterySettingsCollection.Add(settings);
                    }
                }
            }

            // Add battery logs.
            if (battery.InternalBatteryLogCollection != null)
            {
                // Insert appears to be loading the whole collection which is problematic for large data.
                // Load only the data matching what is to be inserted for PK violation (existing data) checks, then insert what is new.
                var newDataIndices = battery.InternalBatteryLogCollection.Select(x => x.LogIndex);
                originalBattery.InternalBatteryLogCollection = db.Entry(originalBattery).Collection(x => x.InternalBatteryLogCollection)
                                                                                        .Query()
                                                                                        .Where(x => newDataIndices.Contains(x.LogIndex))
                                                                                        .ToList();

                foreach (var log in battery.InternalBatteryLogCollection)
                {
                    // NOTE: Originally, we only add battery log to db if log with same index does not exist for given battery.
                    //       It appears that device logs can have same battery log embedded, but with different device log indices obviosly.
                    //       Due to this, many battery logs would be "missing" in joint device log + battery log queries.
                    //       To fix this, we allow battery log index to be non-unique, but batterylog index + device log index should be unique.
                    if (originalBattery.InternalBatteryLogCollection.Any(x => x.LogIndex == log.LogIndex && x.DeviceLogIndex == log.DeviceLogIndex) == false)
                    {
                        // Need to clear all navigation properties.
                        log.ClearNavigationProperties();

                        // Add agent info for each data entry.
                        log.Agent = agent;

                        originalBattery.InternalBatteryLogCollection.Add(log);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(PublishBatteryData)} called with Battery SN: {battery.SerialNumber} completed.");
        }


        #region Azure Helpers

        /// <summary>
        /// Wrapper for the generic method for async operations that don't return a value
        /// </summary>
        /// <param name="asyncOperation"></param>
        /// <returns></returns>
        public static async Task OperationWithBasicRetryAsync(Func<Task> asyncOperation)
        {
            await OperationWithBasicRetryAsync<object>(async () =>
            {
                await asyncOperation();
                return null;
            });
        }

        /// <summary>
        /// Main generic method to perform the supplied async method with multiple retires on transient exceptions/errors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncOperation"></param>
        /// <returns></returns>
        public static async Task<T> OperationWithBasicRetryAsync<T>(Func<Task<T>> asyncOperation, int retryCount = 2)
        {
            int currentRetry = 0;

            while (true)
            {
                try
                {
                    return await asyncOperation();
                }
                catch (Exception ex)
                {
                    currentRetry++;

                    if (currentRetry > retryCount)
                    {
                        // If this is not a transient error or we should not retry re-throw the exception. 
                        throw;
                    }
                }

                // Wait to retry the operation.  
                await Task.Delay(100 * currentRetry);
            }
        }

        #endregion
    }
}
