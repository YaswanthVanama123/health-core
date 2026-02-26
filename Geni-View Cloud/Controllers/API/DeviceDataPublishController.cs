using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Cloud.Services;
using GeniView.Data.Agent;
using GeniView.Data.Hardware;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeniView.Cloud.Controllers.API
{
    [ApiController]
    [Route("api/devicedata")]
    public class DeviceDataPublishController : ControllerBase
    {
        // ── Query endpoints (GET) ──────────────────────────────────────────────

        [HttpGet("agentdevicelogsinfo")]
        public IActionResult GetPublishedAgentDeviceLogsInformation(
            string serialNumber, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok(new PublishedAgentDataInformation());

            var query = db.Entry(device).Collection(x => x.AgentDeviceLogCollection)
                          .Query()
                          .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

            int count = query.Count();
            DateTime? latest = count > 0
                ? DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc)
                : (DateTime?)null;

            return Ok(new PublishedAgentDataInformation { Count = count, LatestEntryDate = latest });
        }

        [HttpGet("agentbatterylogsinfo")]
        public IActionResult GetPublishedAgentBatteryLogsInformation(
            long serialNumberCode, DateTime fromDate, DateTime toDate)
        {
            using var db = new GeniViewCloudDataRepository();
            var battery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);
            if (battery == null)
                return Ok(new PublishedAgentDataInformation());

            var query = db.Entry(battery).Collection(x => x.AgentBatteryLogCollection)
                          .Query()
                          .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

            int count = query.Count();
            DateTime? latest = count > 0
                ? DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc)
                : (DateTime?)null;

            return Ok(new PublishedAgentDataInformation { Count = count, LatestEntryDate = latest });
        }

        [HttpGet("devicesettingsinfo")]
        public IActionResult GetPublishedDeviceSettingsInformation(
            string serialNumber, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok(new PublishedSettingsInformation());

            var query = db.Entry(device).Collection(x => x.DeviceSettingsCollection)
                          .Query()
                          .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

            int count = query.Count();
            DateTime? latest = count > 0
                ? DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc)
                : (DateTime?)null;

            return Ok(new PublishedSettingsInformation { Count = count, LatestEntryDate = latest });
        }

        [HttpGet("batterysettingsinfo")]
        public IActionResult GetPublishedBatterySettingsInformation(
            long serialNumberCode, DateTime fromDate, DateTime toDate)
        {
            using var db = new GeniViewCloudDataRepository();
            var battery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);
            if (battery == null)
                return Ok(new PublishedSettingsInformation());

            var query = db.Entry(battery).Collection(x => x.BatterySettingsCollection)
                          .Query()
                          .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

            int count = query.Count();
            DateTime? latest = count > 0
                ? DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc)
                : (DateTime?)null;

            return Ok(new PublishedSettingsInformation { Count = count, LatestEntryDate = latest });
        }

        [HttpGet("internaldevicelogsinfo")]
        public IActionResult GetPublishedInternalDeviceLogsInformation(
            string serialNumber, long fromLogIndex, long toLogIndex)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok(new PublishedInternalLogsInformation());

            var query = db.Entry(device).Collection(x => x.InternalDeviceLogCollection)
                          .Query()
                          .Where(x => x.LogIndex >= fromLogIndex && x.LogIndex <= toLogIndex);

            int count = query.Count();
            long? latestIndex = count > 0 ? query.Max(x => x.LogIndex) : (long?)null;

            return Ok(new PublishedInternalLogsInformation { Count = count, LatestEntryIndex = latestIndex });
        }

        [HttpGet("internalbatterylogsinfo")]
        public IActionResult GetPublishedInternalBatteryLogsInformation(
            long serialNumberCode, long fromLogIndex, long toLogIndex)
        {
            using var db = new GeniViewCloudDataRepository();
            var battery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);
            if (battery == null)
                return Ok(new PublishedInternalLogsInformation());

            var query = db.Entry(battery).Collection(x => x.InternalBatteryLogCollection)
                          .Query()
                          .Where(x => x.LogIndex >= fromLogIndex && x.LogIndex <= toLogIndex);

            int count = query.Count();
            long? latestIndex = count > 0 ? query.Max(x => x.LogIndex) : (long?)null;

            return Ok(new PublishedInternalLogsInformation { Count = count, LatestEntryIndex = latestIndex });
        }

        [HttpGet("deviceeventsinfo")]
        public IActionResult GetPublishedDeviceEventsInformation(
            string serialNumber, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok(new PublishedEventsInformation());

            var query = db.Entry(device).Collection(x => x.DeviceEventCollection)
                          .Query()
                          .Where(x => x.Timestamp >= fromDate && x.Timestamp <= toDate);

            int count = query.Count();
            DateTime? latest = count > 0
                ? DateTime.SpecifyKind(query.Max(x => x.Timestamp), DateTimeKind.Utc)
                : (DateTime?)null;

            return Ok(new PublishedEventsInformation { Count = count, LatestEntryDate = latest });
        }

        [HttpPost("missingdevicelogdates")]
        public IActionResult GetMissingAgentDeviceLogDates(
            string serialNumber, [FromBody] IEnumerable<DateTime> availableDataDates)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");
            if (availableDataDates == null)
                return BadRequest("List of available data entry dates must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok((IEnumerable<DateTime>)null);

            var existing = db.Entry(device).Collection(x => x.AgentDeviceLogCollection)
                             .Query()
                             .Where(x => availableDataDates.Contains(x.Timestamp))
                             .Select(x => x.Timestamp);
            var missing = availableDataDates.Except(existing).ToList();
            return Ok(missing);
        }

        [HttpPost("missingbatterylogdates")]
        public IActionResult GetMissingAgentBatteryLogDates(
            long serialNumberCode, [FromBody] IEnumerable<DateTime> availableDataDates)
        {
            if (availableDataDates == null)
                return BadRequest("List of available data entry dates must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var battery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);
            if (battery == null)
                return Ok((IEnumerable<DateTime>)null);

            var existing = db.Entry(battery).Collection(x => x.AgentBatteryLogCollection)
                             .Query()
                             .Where(x => availableDataDates.Contains(x.Timestamp))
                             .Select(x => x.Timestamp);
            var missing = availableDataDates.Except(existing).ToList();
            return Ok(missing);
        }

        [HttpPost("missingdevicesettingsdates")]
        public IActionResult GetMissingDeviceSettingsDates(
            string serialNumber, [FromBody] IEnumerable<DateTime> availableSettingsDates)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");
            if (availableSettingsDates == null)
                return BadRequest("List of available settings dates must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok((IEnumerable<DateTime>)null);

            var existing = db.Entry(device).Collection(x => x.DeviceSettingsCollection)
                             .Query()
                             .Where(x => availableSettingsDates.Contains(x.Timestamp))
                             .Select(x => x.Timestamp);
            var missing = availableSettingsDates.Except(existing).ToList();
            return Ok(missing);
        }

        [HttpPost("missingbatterysettingsdates")]
        public IActionResult GetMissingBatterySettingsDates(
            long serialNumberCode, [FromBody] IEnumerable<DateTime> availableSettingsDates)
        {
            if (availableSettingsDates == null)
                return BadRequest("List of available settings dates must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var battery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);
            if (battery == null)
                return Ok((IEnumerable<DateTime>)null);

            var existing = db.Entry(battery).Collection(x => x.BatterySettingsCollection)
                             .Query()
                             .Where(x => availableSettingsDates.Contains(x.Timestamp))
                             .Select(x => x.Timestamp);
            var missing = availableSettingsDates.Except(existing).ToList();
            return Ok(missing);
        }

        [HttpPost("missinginternaldevicelogindices")]
        public IActionResult GetMissingInternalDeviceLogIndices(
            string serialNumber, [FromBody] IEnumerable<long> availableLogIndices)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");
            if (availableLogIndices == null)
                return BadRequest("List of available log indices must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok((IEnumerable<long>)null);

            var existing = db.Entry(device).Collection(x => x.InternalDeviceLogCollection)
                             .Query()
                             .Where(x => availableLogIndices.Contains(x.LogIndex))
                             .Select(x => x.LogIndex);
            var missing = availableLogIndices.Except(existing).ToList();
            return Ok(missing);
        }

        [HttpPost("missinginternalbatteryloguniqueindices")]
        public IActionResult GetMissingInternalBatteryLogUniqueIndices(
            long serialNumberCode, [FromBody] IEnumerable<string> availableUniqueLogIndices)
        {
            if (availableUniqueLogIndices == null)
                return BadRequest("List of available log indices must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var battery = db.Batteries.FirstOrDefault(b => b.SerialNumberCode == serialNumberCode);
            if (battery == null)
                return Ok((IEnumerable<string>)null);

            var existing = db.Entry(battery).Collection(x => x.InternalBatteryLogCollection)
                             .Query()
                             .Where(x => availableUniqueLogIndices.Contains(x.UniqueLogIndex))
                             .Select(x => x.UniqueLogIndex);
            var missing = availableUniqueLogIndices.Except(existing).ToList();
            return Ok(missing);
        }

        [HttpPost("missingdeviceeventdates")]
        public IActionResult GetMissingDeviceEventDates(
            string serialNumber, [FromBody] IEnumerable<DateTime> availableEventDates)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return BadRequest("Device serial number must be provided.");
            if (availableEventDates == null)
                return BadRequest("List of available event dates must be provided.");

            using var db = new GeniViewCloudDataRepository();
            var device = db.Devices.FirstOrDefault(d => d.SerialNumber == serialNumber);
            if (device == null)
                return Ok((IEnumerable<DateTime>)null);

            var existing = db.Entry(device).Collection(x => x.DeviceEventCollection)
                             .Query()
                             .Where(x => availableEventDates.Contains(x.Timestamp))
                             .Select(x => x.Timestamp);
            var missing = availableEventDates.Except(existing).ToList();
            return Ok(missing);
        }

        // ── Publish endpoints (POST) ───────────────────────────────────────────

        [HttpPost("publishdevicedata")]
        public async Task<IActionResult> PublishDeviceData([FromBody] PublishDeviceDataRequest request)
        {
            if (request?.Device == null)
                return BadRequest("Device information must be provided.");
            if (request.Agent == null)
                return BadRequest("Agent information must be provided.");

            string agentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            request.Agent.AgentAddress = agentIp;

            var svc = new GeniViewDeviceDataPublishService();
            svc.PublishDeviceData(request.Device, request.Batteries, request.Agent);
            return Ok();
        }

        [HttpPost("publishbatterydata")]
        public IActionResult PublishBatteryData([FromBody] PublishBatteryDataRequest request)
        {
            if (request?.Battery == null)
                return BadRequest("Battery information must be provided.");
            if (request.Agent == null)
                return BadRequest("Agent information must be provided.");

            string agentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            request.Agent.AgentAddress = agentIp;

            var svc = new GeniViewDeviceDataPublishService();
            svc.PublishBatteryData(request.Battery, request.Agent);
            return Ok();
        }
    }

    // ── Request body DTOs ──────────────────────────────────────────────────────

    public class PublishDeviceDataRequest
    {
        public Device Device { get; set; }
        public IEnumerable<Battery> Batteries { get; set; }
        public Agent Agent { get; set; }
    }

    public class PublishBatteryDataRequest
    {
        public Battery Battery { get; set; }
        public Agent Agent { get; set; }
    }
}
