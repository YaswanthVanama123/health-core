// IDeviceDataPublishService.cs — Phase 7 stub
// WCF [ServiceContract]/[OperationContract] attributes removed.
// In Phase 7, this interface will be implemented as a REST API controller
// at route /api/devicedata/...
using GeniView.Data.Agent;
using GeniView.Data.Hardware;
using System;
using System.Collections.Generic;

namespace GeniView.Cloud.Services
{
    public interface IDeviceDataPublishService
    {
        PublishedAgentDataInformation GetPublishedAgentDeviceLogsInformation(string serialNumber, DateTime fromDate, DateTime toDate);
        PublishedAgentDataInformation GetPublishedAgentBatteryLogsInformation(long serialNumber, DateTime fromDate, DateTime toDate);
        PublishedSettingsInformation GetPublishedDeviceSettingsInformation(string serialNumber, DateTime fromDate, DateTime toDate);
        PublishedSettingsInformation GetPublishedBatterySettingsInformation(long serialNumber, DateTime fromDate, DateTime toDate);
        PublishedInternalLogsInformation GetPublishedInternalDeviceLogsInformation(string serialNumber, long fromLogIndex, long toLogIndex);
        PublishedInternalLogsInformation GetPublishedInternalBatteryLogsInformation(long serialNumber, long fromLogIndex, long toLogIndex);
        PublishedEventsInformation GetPublishedDeviceEventsInformation(string serialNumber, DateTime fromDate, DateTime toDate);
        IEnumerable<DateTime> GetMissingAgentDeviceLogDates(string serialNumber, IEnumerable<DateTime> availableDataDates);
        IEnumerable<DateTime> GetMissingAgentBatteryLogDates(long serialNumber, IEnumerable<DateTime> availableDataDates);
        IEnumerable<DateTime> GetMissingDeviceSettingsDates(string serialNumber, IEnumerable<DateTime> availableSettingsDates);
        IEnumerable<DateTime> GetMissingBatterySettingsDates(long serialNumber, IEnumerable<DateTime> availableSettingsDates);
        IEnumerable<long> GetMissingInternalDeviceLogIndices(string serialNumber, IEnumerable<long> availableLogIndices);
        IEnumerable<string> GetMissingInternalBatteryLogUniqueIndices(long serialNumberCode, IEnumerable<string> availableUniqueLogIndices);
        IEnumerable<DateTime> GetMissingDeviceEventDates(string serialNumber, IEnumerable<DateTime> availableEventDates);
        void PublishDeviceData(Device device, IEnumerable<Battery> batteries, Agent agent);
        void PublishBatteryData(Battery battery, Agent agent);
    }

    public class PublishedAgentDataInformation
    {
        public int Count { get; set; }
        public DateTime? LatestEntryDate { get; set; }
    }

    public class PublishedSettingsInformation : PublishedAgentDataInformation { }

    public class PublishedEventsInformation : PublishedAgentDataInformation { }

    public class PublishedInternalLogsInformation
    {
        public int Count { get; set; }
        public long? LatestEntryIndex { get; set; }
    }
}
