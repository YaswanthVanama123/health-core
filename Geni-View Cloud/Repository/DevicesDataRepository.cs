using GeniView.Cloud.Common;
using Microsoft.EntityFrameworkCore;
using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Hardware;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using NLog;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class DevicesDataRepository : IDisposable
    {
        public DBHelper _dBHelp = new DBHelper();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        #region Devices

        public Device Create(Device devices, GeniViewCloudDataRepository db)
        {
            Device device = new Device();
            
            if (devices != null )
            {

                db.Devices.Add(devices);

                db.SaveChanges();
            }

            return device;
        }

        public List<Device> CreateBatch(List<Device> devices , GeniViewCloudDataRepository db)
        {
            List<string> snCode = devices.Select(x => x.SerialNumber).ToList();
            var exist = FindBySN(snCode, db).ToList();

            var remove = devices.RemoveAll(x => exist.Any(e => e.SerialNumber == x.SerialNumber));

            if (devices != null && devices.Count >= 1 )
            {
                _dBHelp.BatchInsert(db, db.Devices, devices);
            }

            if (devices.Any() == true)
            {
                var log = devices.Select(x => new { x.SerialNumber });
                _logger.Info($"Device CreateBatch {devices.Count}  Remove {remove} log {JsonConvert.SerializeObject(log)}");

            }

            return devices;
        }

        public List<Device> CreateBatch(Dictionary<string, Device> devices, GeniViewCloudDataRepository db)
        {
            List<Device> result = new List<Device>();

            foreach (var item in devices)
            {
                //var device = item.Value;
                var device = new Device(item.Value);

                result.Add(device);

            }

            result = CreateBatch(result, db);

            return result;
        }

        public List<Device> UpdateBatch(List<Device> devices, GeniViewCloudDataRepository db)
        {
            List<string> snCode = devices.Select(x => x.SerialNumber).ToList();
            var exist = FindBySN(snCode, db).ToList();

            //Remove exist
            var ret = devices.Where(x => exist.Any(e => e.SerialNumber == x.SerialNumber) == false);

            var remove = devices.RemoveAll(x => exist.Any(e => e.SerialNumber == x.SerialNumber) == false);

            if (devices != null && devices.Count >= 1)
            {

                foreach (var device in devices)
                {
                    var orgion = exist.Where(x => x.SerialNumber == device.SerialNumber).FirstOrDefault();

                    if (orgion != null)
                    {
                        device.ID = orgion.ID;
                    }
                }

                _dBHelp.UpdateAll(db, db.Devices, devices);
            }

            if (devices.Any() == true)
            {
                var log = devices.Select(x => new { x.ID, x.SerialNumber });
                _logger.Info($"Device UpdateBatch {devices.Count}  Remove {remove} log {JsonConvert.SerializeObject(log)}");
            }


            return devices;
        }

        public List<Device> UpdateBatch(Dictionary<string, Device> devices, GeniViewCloudDataRepository db)
        {
            List<Device> result = new List<Device>();

            foreach (var item in devices)
            {
                //var device = item.Value;
                var device = new Device(item.Value);

                result.Add(device);

            }

            result = UpdateBatch(result, db);

            return result;
        }

        public List<Device> CreateAndUpdateBatch(Dictionary<string, Device> devices, GeniViewCloudDataRepository db)
        {
            List<Device> result = new List<Device>();

            var update = UpdateBatch(devices,db);
            var create = CreateBatch(devices, db);

            result.AddRange(update);
            result.AddRange(create);

            return result;
        }


        public List<DeviceListViewModel> GetDevices(long? communityID, long? groupID, bool includeAllSubGroups, string customFilter = null)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                var mainQuery = (from d in db.Devices
                                             .Include(x => x.AgentDeviceLogCollection)
                                             .Include(x => x.DeviceSettingsCollection)
                                             .Include(x => x.Group)
                                             .Include(x => x.Community)
                                             .Where(x => (communityID == null ? true : x.Community.ID == communityID) && x.IsDeactivated == false)
                                             .AsNoTracking()
                                 let row = d.AgentDeviceLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                                 let set = d.DeviceSettingsCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                                 select new DeviceListViewModel()
                                 {
                                     ID = d.ID,
                                     SerialNumber = d.SerialNumber,
                                     FirstSeenOn = d.AgentDeviceLogCollection.Min(t => t.Timestamp),
                                     LastSeenOn = d.AgentDeviceLogCollection.Max(t => t.Timestamp),
                                     Community = d.Community,
                                     Group = d.Group,
                                     LastAgentDeviceLog = row,
                                     LastDeviceSetting = set,
                                     isOnline = (row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes) ? true : false,
                                     //Status = (row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.IsExternalPowerInputApplied == false) ? new ExtraInfo { Name = "On Battery", Color = GlobalSettings.SuccessColor } :
                                     //         (row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.IsExternalPowerInputApplied == true) ? new ExtraInfo { Name = "Plugged In", Color = "#90EE90" } :
                                     //         (row != null && row.Timestamp >= GlobalSettings.OfflineRangeInDays && row.Timestamp < GlobalSettings.OnlineRangeInMinutes) 
                                     //         ? new ExtraInfo { Name = "Offline", Color = GlobalSettings.WarningColor } : new ExtraInfo { Name = "Unknown", Color = GlobalSettings.AlertColor },
                                     Status = (row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes) ? new ExtraInfo { Name = "Online", Color = GlobalSettings.SuccessColor } :
                                              (row != null && row.Timestamp >= GlobalSettings.OfflineRangeInDays && row.Timestamp < GlobalSettings.OnlineRangeInMinutes)? new ExtraInfo { Name = "Offline", Color = GlobalSettings.WarningColor } : new ExtraInfo { Name = "Unknown", Color = GlobalSettings.AlertColor },

                                     Capacity = (row != null && row.DeviceCapacity > GlobalSettings.SuccessChargingLVL) ? new ExtraInfo { Name = row.DeviceCapacity.ToString(), Color = GlobalSettings.SuccessColor } :
                                                (row != null && row.DeviceCapacity > GlobalSettings.AlertChargingLVL) ? new ExtraInfo { Name = row.DeviceCapacity.ToString(), Color = GlobalSettings.WarningColor } : new ExtraInfo { Name = row.DeviceCapacity.ToString(), Color = GlobalSettings.AlertColor },
                                     Temperature = (row != null && row.Status.Temperature < GlobalSettings.SuccessTemperature) ? new ExtraInfo { Name = row.Status.Temperature.ToString(), Color = GlobalSettings.SuccessColor } :
                                                   (row != null && row.Status.Temperature >= GlobalSettings.AlertTemperature) ? new ExtraInfo { Name = row.Status.Temperature.ToString(), Color = GlobalSettings.AlertColor } : new ExtraInfo { Name = row.Status.Temperature.ToString(), Color = GlobalSettings.WarningColor },
                                     Alert = (row != null && row.Status.Temperature >= GlobalSettings.AlertTemperature || row.DeviceCapacity <= GlobalSettings.AlertChargingLVL) ? new ExtraInfo { Name = "Alert", Color = GlobalSettings.AlertColor } :
                                                                                                                          new ExtraInfo { Name = "Normal", Color = GlobalSettings.SuccessColor },
                                 }).AsEnumerable();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    mainQuery = (from m in mainQuery.Where(x => x.Group != null)
                                 join ch in allChildrenGroups on m.Group.ID equals ch.ID
                                 select m).AsEnumerable();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    mainQuery = mainQuery.Where(x => x.Group != null && x.Group.ID == groupID);
                }

                // Optional Filter
                if (customFilter != null)
                {
                    switch (customFilter)
                    {
                        case "OnBattery":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentDeviceLog
                                        where row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.IsExternalPowerInputApplied == false
                                        select m;
                            break;
                        case "PluggedIn":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentDeviceLog
                                        where row != null && row.Timestamp >= GlobalSettings.OnlineRangeInMinutes && row.IsExternalPowerInputApplied == true
                                        select m;
                            break;
                        case "Offline":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentDeviceLog
                                        where row != null && row.Timestamp >= GlobalSettings.OfflineRangeInDays && row.Timestamp < GlobalSettings.OnlineRangeInMinutes
                                        select m;
                            break;
                        case "Unknown":
                            mainQuery = from m in mainQuery
                                        let row = m.LastAgentDeviceLog
                                        where row != null && row.Timestamp < GlobalSettings.OfflineRangeInDays
                                        select m;
                            break;
                        default:
                            mainQuery = Enumerable.Empty<DeviceListViewModel>();
                            break;
                    }
                }
                return mainQuery.OrderByDescending(x => x.LastSeenOn).ToList();
            }
        }

        public DeviceDetailsViewModel GetDeviceDetails(string serialNumber)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                var mainQuery = (from d in db.Devices.Include(x => x.AgentDeviceLogCollection)  // Load Device Data
                                                     .Include(x => x.DeviceSettingsCollection)  // Load Settings
                                                     .Include(x => x.Group)                     // Load Groups
                                                     .Include(x => x.Community)                 // Load Communities
                                                     .Where(x => x.SerialNumber == serialNumber && x.IsDeactivated == false)// Condition
                                                     .AsNoTracking()                            // No Tracking Better performance
                                 let lastLog = d.AgentDeviceLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                                 let lastSetting = d.DeviceSettingsCollection.OrderByDescending(x => x.Timestamp).FirstOrDefault()
                                 select new DeviceDetailsViewModel()
                                 {
                                     Device = d,
                                     Community = d.Community,
                                     Group = d.Group,
                                     LastSettings = lastSetting,
                                     LastAgentDeviceLog = lastLog,
                                     FirstSeenOn = d.AgentDeviceLogCollection.Min(t => t.Timestamp),
                                     LastSeenOn = d.AgentDeviceLogCollection.Max(t => t.Timestamp),
                                     PowerOutput = (lastLog != null && lastSetting != null && lastSetting.PowerOutputSettings.Power != null && lastSetting.PowerOutputSettings.Power != 0) ?
                                                                                         new ExtraInfo { Name = Math.Round(lastLog.PowerOutput.Current * lastLog.PowerOutput.Voltage / lastSetting.PowerOutputSettings.Power * 100, 0).ToString(), Color = GlobalSettings.SuccessColor, Value = Math.Round(lastLog.PowerOutput.Current * lastLog.PowerOutput.Voltage, 0) }
                                                                                       : new ExtraInfo { Name = "100", Color = GlobalSettings.AlertColor, Value = 0 }, // IF Power Calculation Error Draw Circle With full percent with alert Color
                                     PowerInput = (lastLog != null && lastSetting != null) ?
                                                                                         new ExtraInfo { Name = Math.Round(lastLog.PowerInput.Current * lastLog.PowerInput.Voltage / 210 * 100, 0).ToString(), Color = GlobalSettings.SuccessColor, Value = Math.Round(lastLog.PowerInput.Current * lastLog.PowerInput.Voltage, 0) }
                                                                                       : new ExtraInfo { Name = "100", Color = GlobalSettings.AlertColor, Value = 0 }, // IF Power Calculation Error Draw Circle With full percent with alert Color
                                 }).FirstOrDefault();
                return mainQuery;
            }
        }

        public DeviceBayDataModel GetDeviceBayData(string serialNumber)
        {
            DeviceBayDataModel model = new DeviceBayDataModel();
            model.BayCount = 0;
            model.Capacity = new List<string>();
            model.SerialNumber = new List<string>();

            using (var db = new GeniViewCloudDataRepository())
            {

                var query = (from a in db.Devices.Include(x => x.AgentDeviceLogCollection)
                                        .Include(x => x.DeviceSettingsCollection)
                                        .Where(x => x.SerialNumber == serialNumber && x.IsDeactivated == false)
                                        .AsNoTracking()
                             let lastrow = a.AgentDeviceLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                             let lastsetting = a.DeviceSettingsCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                             select new
                             {
                                 LastData = lastrow,
                                 bayCnt = lastsetting != null ? lastsetting.Bays : 0
                             }).FirstOrDefault();

                var row = query.LastData;
                model.BayCount = query.bayCnt;

                var batteryData = db.AgentBatteryLog.Include(b => b.Battery)
                                                .Where(b => b.Timestamp == row.Timestamp && b.DeviceSerialNumber == serialNumber);

                #region Get Battery Bay Information and Status
                if (row != null)
                {
                    if ((row.BatteriesPresent & Batteries.BatteryOne) == Batteries.BatteryOne)
                    {
                        var result = (from b in batteryData.Where(b => b.Bay == 1)
                                      select new
                                      {
                                          Capacity = b.SlowChangingDataA.RelativeStateOfCharge,
                                          Battery = b.Battery
                                      }).FirstOrDefault();
                        if (result != null)
                        {
                            model.Capacity.Add(result.Capacity.ToString() + "%");
                            model.SerialNumber.Add(result.Battery.SerialNumber);
                        }
                        else
                        {
                            model.Capacity.Add("Sync Error");
                            model.SerialNumber.Add("Bay1");
                        }
                    }
                    else
                    {
                        model.Capacity.Add("Empty");
                        model.SerialNumber.Add("Bay1");
                    }

                    if ((row.BatteriesPresent & Batteries.BatteryTwo) == Batteries.BatteryTwo)
                    {
                        var result = (from b in batteryData.Where(b => b.Bay == 2)
                                      select new
                                      {
                                          Capacity = b.SlowChangingDataA.RelativeStateOfCharge,
                                          Battery = b.Battery
                                      }).FirstOrDefault();
                        if (result != null)
                        {
                            model.Capacity.Add(result.Capacity.ToString() + "%");
                            model.SerialNumber.Add(result.Battery.SerialNumber);
                        }
                        else
                        {
                            model.Capacity.Add("Sync Error");
                            model.SerialNumber.Add("Bay2");
                        }
                    }
                    else
                    {
                        model.Capacity.Add("Empty");
                        model.SerialNumber.Add("Bay2");
                    }

                    if ((row.BatteriesPresent & Batteries.BatteryThree) == Batteries.BatteryThree)
                    {
                        var result = (from b in batteryData.Where(b => b.Bay == 3)
                                      select new
                                      {
                                          Capacity = b.SlowChangingDataA.RelativeStateOfCharge,
                                          Battery = b.Battery
                                      }).FirstOrDefault();
                        if (result != null)
                        {
                            model.Capacity.Add(result.Capacity.ToString() + "%");
                            model.SerialNumber.Add(result.Battery.SerialNumber);
                        }
                        else
                        {
                            model.Capacity.Add("Sync Error");
                            model.SerialNumber.Add("Bay3");
                        }
                    }
                    else
                    {
                        model.Capacity.Add("Empty");
                        model.SerialNumber.Add("Bay3");
                    }

                    if ((row.BatteriesPresent & Batteries.BatteryFour) == Batteries.BatteryFour)
                    {
                        var result = (from b in batteryData.Where(b => b.Bay == 4)
                                      select new
                                      {
                                          Capacity = b.SlowChangingDataA.RelativeStateOfCharge,
                                          Battery = b.Battery
                                      }).FirstOrDefault();
                        if (result != null)
                        {
                            model.Capacity.Add(result.Capacity.ToString() + "%");
                            model.SerialNumber.Add(result.Battery.SerialNumber);
                        }
                        else
                        {
                            model.Capacity.Add("Sync Error");
                            model.SerialNumber.Add("Bay4");
                        }
                    }
                    else
                    {
                        model.Capacity.Add("Empty");
                        model.SerialNumber.Add("Bay4");
                    }

                    if ((row.BatteriesPresent & Batteries.BatteryFive) == Batteries.BatteryFive)
                    {
                        var result = (from b in batteryData.Where(b => b.Bay == 5)
                                      select new
                                      {
                                          Capacity = b.SlowChangingDataA.RelativeStateOfCharge,
                                          Battery = b.Battery
                                      }).FirstOrDefault();
                        if (result != null)
                        {
                            model.Capacity.Add(result.Capacity.ToString() + "%");
                            model.SerialNumber.Add(result.Battery.SerialNumber);
                        }
                        else
                        {
                            model.Capacity.Add("Sync Error");
                            model.SerialNumber.Add("Bay5");
                        }
                    }
                    else
                    {
                        model.Capacity.Add("Empty");
                        model.SerialNumber.Add("Bay5");
                    }
                }
                else
                {
                    model.BayCount = 1;
                    model.Capacity.Add("No Info");
                    model.SerialNumber.Add("Bay1");
                }
            }
            #endregion

            return model;
        }

        public DeviceBayDataModel GetDeviceBayDataG3(string serialNumber)
        {
            DeviceBayDataModel model = new DeviceBayDataModel();
            model.BayCount = 0;
            model.Capacity = new List<string>();
            model.SerialNumber = new List<string>();

            using (var db = new GeniViewCloudDataRepository())
            {

                var query = (from a in db.Devices.Include(x => x.AgentDeviceLogCollection)
                                        .Include(x => x.DeviceSettingsCollection)
                                        .Where(x => x.SerialNumber == serialNumber && x.IsDeactivated == false)
                                        .AsNoTracking()
                             let lastrow = a.AgentDeviceLogCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                             let lastsetting = a.DeviceSettingsCollection.OrderByDescending(t => t.Timestamp).FirstOrDefault()
                             select new
                             {
                                 LastData = lastrow,
                                 bayCnt = lastsetting != null ? lastsetting.Bays : 0
                             }).FirstOrDefault();

                var row = query.LastData;
                model.BayCount = 0;
                
                var batteryData = db.AgentBatteryLog.Include(b => b.Battery)
                                                .Where(b => b.Timestamp == row.Timestamp && b.DeviceSerialNumber == serialNumber);

                #region Get Battery Bay Information and Status
                if (row != null && batteryData!= null && batteryData.Any() == true)
                {
                    foreach (var item in batteryData)
                    {
                        model.Capacity.Add(item.SlowChangingDataA.RelativeStateOfCharge.ToString() + "%");
                        model.SerialNumber.Add(item.Battery.SerialNumber);
                        model.BayCount++;
                    }
                }
                else
                {
                    model.BayCount = 1;
                    model.Capacity.Add("No Info");
                    model.SerialNumber.Add("Bay1");
                }
            }
            #endregion

            return model;
        }

        public Device FindDeviceByID(long id, long? communityID = null, long? groupID = null)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                Device device = db.Devices.Include(x => x.DeviceSettingsCollection)
                                          .Include(x => x.Community)
                                          .Include(x => x.Group)
                                .Where(x => (communityID == null ? true : x.Community.ID == communityID)
                                         && (groupID == null ? true : x.Group.ID == groupID)
                                         && (x.ID == id)
                                         && (x.IsDeactivated == false))
                                .FirstOrDefault();
                return device;
            }
        }

        public Device FindBySN(string serialNumber)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                Device device = db.Devices.Include(x => x.Community)
                                          .Include(x => x.Group)
                                          .Where(x => x.SerialNumber == serialNumber && x.IsDeactivated == false)
                                          .FirstOrDefault();
                return device;
            }
        }

        public Device FindBySN(string serialNumber , GeniViewCloudDataRepository db)
        {
            Device device = db.Devices.Include(x => x.Community)
                                        .Include(x => x.Group)
                                        .Where(x => x.SerialNumber == serialNumber && x.IsDeactivated == false)
                                        .FirstOrDefault();
            return device;
        }

        public IEnumerable<Device> FindBySN(List<string> serialNumber, GeniViewCloudDataRepository db)
        {

            var result = db.Devices.Include(x => x.Community)
                                        .Include(x => x.Group)
                                        .Where(x=> x.IsDeactivated == false && serialNumber.Any(s=>s == x.SerialNumber) );
            return result;
        }

        public IEnumerable<Device> FindBySN(List<Device> devices, GeniViewCloudDataRepository db)
        {
            var sn = devices
                .Select(y => y.SerialNumber).ToList();

            var result = FindBySN(sn, db);

            return result;
        }

        public int GetDeviceBayCount(long deviceID)
        {
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {

                int count = 0;
                Device device = db.Devices
                                  .Include(x => x.DeviceSettingsCollection)
                                  .SingleOrDefault(x => x.ID == deviceID);
                if (device != null)
                {
                    count = device.DeviceSettingsCollection.Count > 0 ? device.DeviceSettingsCollection.FirstOrDefault().Bays : 0;
                }

                return count;
            }
        }

        public void UpdateDevice(DeviceListViewModel device, long? groupID)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                var originaldevice = db.Devices.Include(x => x.Group).Where(x => x.ID == device.ID && x.IsDeactivated == false).FirstOrDefault();
                if (originaldevice != null)
                {
                    if (groupID != null)
                        originaldevice.Group = db.Groups.Find(groupID);
                    else
                        originaldevice.Group = null;
                }

                db.Entry(originaldevice).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public IEnumerable<BatteryModel> GetDeviceMasterChartModel(string serialNumber, int? bayNo, DateTime beginDate, DateTime endDate, int pointCount = 500)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = (ApplicationUser?)null;
                }

                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);

                int index = 1;
                var query = db.AgentBatteryLog
                               .Where(x => x.DeviceSerialNumber == serialNumber
                                        //&& x.Bay == bayNo 
                                        && x.Timestamp >= convertedBeginDate
                                        && x.Timestamp <= convertedEndDate)
                               .OrderBy(x => x.Timestamp)
                               .AsNoTracking()
                               .AsEnumerable()
                               .Select(sel => new BatteryModel()
                               {
                                   LogIndex = index++,
                                   Voltage = sel.OperatingData.Voltage,
                                   AverageCurrent = sel.OperatingData.AverageCurrent,
                                   RelativeStateofCharge = sel.SlowChangingDataA.RelativeStateOfCharge,
                                   Power = Math.Round(sel.OperatingData.Voltage * sel.OperatingData.Current, 2),
                                   Current = sel.OperatingData.Current,
                                   RemainingCapacity = sel.SlowChangingDataA.RemainingCapacity,
                                   Temperature = sel.SlowChangingDataB.BatteryInternalTemperature,
                                   Bay = sel.Bay,
                                   TimeStampt = sel.Timestamp,
                               }).ToList();

                int count = query.Count();
                count = count < pointCount ? 1 : count / pointCount;

                var model = query.GroupBy(g => new { index = g.LogIndex / count, bay = g.Bay })
                                     .Select(sel => new BatteryModel()
                                     {
                                         LogIndex = sel.Key.index,
                                         Voltage = sel.Average(x => x.Voltage),
                                         AverageCurrent = sel.Average(x => x.AverageCurrent),
                                         RelativeStateofCharge = (int)sel.Average(x => x.RelativeStateofCharge),
                                         Power = sel.Average(x => x.Power),
                                         Current = sel.Average(x => x.Current),
                                         RemainingCapacity = sel.Average(x => x.RemainingCapacity),
                                         Temperature = (int)sel.Average(x => x.Temperature),
                                         Bay = sel.Key.bay,
                                         TimeStampt = sel.Max(x => x.TimeStampt)
                                     }).ToList();
                model.ForEach(x => x.LogDate = TimeZoneHelper.GetLocalDateTime(x.TimeStampt, currentUser));

                return model;
            }
        }

        public IEnumerable<BatteryModel> GetDeviceMasterChartModelByLog(string serialNumber, int? bayNo, DateTime beginDate, DateTime endDate, int pointCount = 500)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = (ApplicationUser?)null;
                }

                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);
                var query = db.InternalBatteryLog
                               .Where(x => x.DeviceSerialNumber == serialNumber && x.Bay == bayNo && x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate)
                               .OrderBy(x => x.Timestamp)
                               .AsNoTracking()
                               .AsEnumerable()
                               .Select(sel => new BatteryModel()
                               {
                                   LogIndex = sel.DeviceLogIndex,
                                   Voltage = sel.Voltage,
                                   AverageCurrent = sel.AverageCurrent,
                                   RelativeStateofCharge = sel.RelativeStateOfCharge,
                                   Power = Math.Round(sel.Voltage * sel.Current, 2),
                                   Current = sel.Current,
                                   RemainingCapacity = sel.RemainingCapacity,
                                   Temperature = sel.Temperature,
                                   Bay = sel.Bay,
                                   TimeStampt = sel.Timestamp,
                               }).ToList();

                int count = query.Count();

                count = count < pointCount ? 1 : count / pointCount;

                var model = query.GroupBy(g => new { index = g.LogIndex / count, bay = g.Bay })
                                     .Select(sel => new BatteryModel()
                                     {
                                         LogIndex = sel.Key.index,
                                         Voltage = sel.Average(x => x.Voltage),
                                         AverageCurrent = sel.Average(x => x.AverageCurrent),
                                         RelativeStateofCharge = (int)sel.Average(x => x.RelativeStateofCharge),
                                         Power = sel.Average(x => x.Power),
                                         Current = sel.Average(x => x.Current),
                                         RemainingCapacity = sel.Average(x => x.RemainingCapacity),
                                         Temperature = (int)sel.Average(x => x.Temperature),
                                         Bay = sel.Key.bay,
                                         TimeStampt = sel.Max(x => x.TimeStampt)
                                     }).ToList();

                model.ForEach(x => x.LogDate = TimeZoneHelper.GetLocalDateTime(x.TimeStampt, currentUser));
                return model;
            }
        }

        public IEnumerable<DeviceModel> GetDeviceChartModel(long deviceID, DateTime beginDate, DateTime endDate, int pointCount = 500)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = (ApplicationUser?)null;
                }

                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);

                var originalDevice = db.Devices.FirstOrDefault(x => x.ID == deviceID && x.IsDeactivated == false);
                if (originalDevice != null)
                {
                    int index = 1;
                    var query = db.Entry(originalDevice)
                                  .Collection(x => x.AgentDeviceLogCollection)
                                  .Query()
                                  .Where(x => x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate)
                                  .OrderBy(x => x.Timestamp)
                                  .AsNoTracking()
                                  .AsEnumerable()
                                  .Select(x => new DeviceModel()
                                  {
                                      LogIndex = index++,
                                      PowerCurrentInput = x.PowerInput.Current,
                                      PowerCurrentOutput = x.PowerOutput.Current,
                                      PowerVoltageInput = x.PowerInput.Voltage,
                                      PowerVoltageOutput = x.PowerOutput.Voltage,
                                      PowerTemperatureOutput = x.Status.Temperature,
                                      PowerInput = x.PowerInput.Power,
                                      PowerOutput = x.PowerOutput.Power,
                                      TimeStampt = x.Timestamp,
                                  }).ToList();

                    int count = query.Count();

                    count = count < pointCount ? 1 : count / pointCount;

                    var model = query.GroupBy(g => new { idx = g.LogIndex / count })
                                     .Select(s => new DeviceModel()
                                     {
                                         LogIndex = s.Key.idx,
                                         PowerCurrentInput = s.Average(x => x.PowerCurrentInput),
                                         PowerCurrentOutput = s.Average(x => x.PowerCurrentOutput),
                                         PowerVoltageInput = s.Average(x => x.PowerVoltageInput),
                                         PowerVoltageOutput = s.Average(x => x.PowerVoltageOutput),
                                         PowerTemperatureOutput = s.Average(x => x.PowerTemperatureOutput),
                                         PowerInput = s.Average(x => x.PowerInput),
                                         PowerOutput = s.Average(x => x.PowerOutput),
                                         TimeStampt = s.Max(x => x.TimeStampt)
                                     }).ToList();

                    model.ForEach(x => x.LogDate = TimeZoneHelper.GetLocalDateTime(x.TimeStampt, currentUser));
                    return model;
                }

                return null;
            }
        }

        public IEnumerable<DeviceModel> GetDeviceChartModelByLog(long deviceID, DateTime beginDate, DateTime endDate, int pointCount = 500)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = (ApplicationUser?)null;
                }

                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);

                var originalDevice = db.Devices.FirstOrDefault(x => x.ID == deviceID && x.IsDeactivated == false);
                if (originalDevice != null)
                {
                    var query = db.Entry(originalDevice)
                                  .Collection(x => x.InternalDeviceLogCollection)
                                  .Query()
                                  .Where(x => x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate)
                                  .OrderBy(x => x.LogIndex)
                                  .AsNoTracking()
                                  .AsEnumerable()
                                  .Select(x => new DeviceModel()
                                  {
                                      LogIndex = x.LogIndex,
                                      PowerCurrentOutput = x.PowerOutput.Current,
                                      PowerVoltageOutput = x.PowerOutput.Voltage,
                                      PowerTemperatureOutput = x.Status.Temperature,
                                      PowerOutput = x.PowerOutput.Power,
                                      TimeStampt = x.Timestamp
                                  }).ToList();

                    int count = query.Count();

                    count = count < pointCount ? 1 : count / pointCount;

                    var model = query.GroupBy(g => new { idx = g.LogIndex / count })
                                     .Select(s => new DeviceModel()
                                     {
                                         LogIndex = s.Key.idx,
                                         PowerCurrentOutput = s.Average(x => x.PowerCurrentOutput),
                                         PowerVoltageOutput = s.Average(x => x.PowerVoltageOutput),
                                         PowerTemperatureOutput = s.Average(x => x.PowerTemperatureOutput),
                                         PowerOutput = s.Average(x => x.PowerOutput),
                                         TimeStampt = s.Max(x => x.TimeStampt)
                                     }).ToList();

                    model.ForEach(x => x.LogDate = TimeZoneHelper.GetLocalDateTime(x.TimeStampt, currentUser));

                    return model;
                }
                return null;
            }
        }
        
        public List<DeviceHistoryViewModel> GetDeviceHistoryLog(long id, DateTime beginDate, DateTime endDate, ApplicationUser currentUser, bool isPeriodicDataTriggerIncluded, int count = 100)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(60);

                List<DeviceHistoryViewModel> model = new List<DeviceHistoryViewModel>();
                
                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(beginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(endDate, currentUser);

                var originalDevice = db.Devices.Where(x => x.ID == id && x.IsDeactivated == false).FirstOrDefault();
                if (originalDevice != null)
                {
                    var query = db.Entry(originalDevice)
                                 .Collection(x => x.InternalDeviceLogCollection)
                                 .Query()
                                 .Where(x => x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate && (isPeriodicDataTriggerIncluded ? true : x.EventCodeRaw != 50))
                                 .OrderByDescending(x => x.LogIndex)
                                 .AsNoTracking()
                                 .Take(count)
                                 .Select(x => x).ToList();

                    var batteryLogs = (
                                      from d in query
                                      join b in db.InternalBatteryLog.Include(x => x.Battery)
                                                  .Where(b => b.DeviceSerialNumber == originalDevice.SerialNumber && b.Timestamp >= convertedBeginDate && b.Timestamp <= convertedEndDate)
                                       on d.LogIndex equals b.DeviceLogIndex
                                      select b)
                                      .GroupBy(row => row.DeviceLogIndex)
                                      .Select(g => new
                                      {
                                          DeviceLogIndex = g.Key,
                                          Battery = g.Select(x => x.Battery),
                                          OemIds = g.Select(r => r.OemIdentifier)
                                      });

                    model = (from dl in query
                             join bl in batteryLogs on dl.LogIndex equals bl.DeviceLogIndex
                             select new DeviceHistoryViewModel
                             {
                                 InternalDeviceLog = dl,
                                 Batteries = bl.Battery,
                                 OemIds = bl.OemIds,
                             }).ToList();

                }
                return model;
            }
        }
        
        public List<DeviceEvent> GetDeviceEventHistoryList(DeviceEventLogFilter filter, ApplicationUser currentUser)
        {
            List<DeviceEvent> model = new List<DeviceEvent>();
            using (var db = new GeniViewCloudDataRepository())
            {

                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(filter.BeginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(filter.EndDate, currentUser);

                var originalDevice = db.Devices.Include(x => x.DeviceEventCollection)
                                               .FirstOrDefault(x => x.ID == filter.DeviceID && x.IsDeactivated == false);
                if (originalDevice != null)
                {
                    model = (from de in originalDevice.DeviceEventCollection
                                                      .Where(x => x.Timestamp >= convertedBeginDate &&
                                                                  x.Timestamp <= convertedEndDate).OrderBy(x => x.Timestamp)
                             select de).Take(filter.Count).ToList();
                }

                return model;
            }
        }

        public List<DeviceEvent> GetDeviceEventHistoryList(long? communityID, long? groupID, bool includeAllSubGroups, DeviceEventLogFilter filter, ApplicationUser currentUser)
        {
            List<DeviceEvent> model = new List<DeviceEvent>();
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {

                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(filter.BeginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(filter.EndDate, currentUser);

                var mainBatteriesQuery = db.DeviceEvents
                                           .Include(x => x.Device)
                                           .Include(x => x.Agent)
                                           .Where(x => (communityID != null ? x.Device.Community.ID == communityID : true) &&
                                                        x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate &&
                                                        x.Device.IsDeactivated == false)
                                           .AsNoTracking()
                                           .Select(t => new
                                           {
                                               DeviceEvent = t,
                                               DeviceID = t.Device.ID,
                                               Group = t.Device.Group
                                           }).AsEnumerable();

                // Global Filter
                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups = new List<Group>();
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }
                    mainBatteriesQuery = (from m in mainBatteriesQuery.Where(x => x.Group != null)
                                          join ch in allChildrenGroups on m.Group.ID equals ch.ID
                                          select m).AsEnumerable();
                }
                else if (groupID != null && !includeAllSubGroups)//Get Exact one Group assigned Devices
                {
                    mainBatteriesQuery = mainBatteriesQuery.Where(x => x.Group != null && x.Group.ID == groupID);
                }

                model = mainBatteriesQuery.Select(x => x.DeviceEvent).OrderByDescending(x => x.Timestamp).Take(filter.Count).ToList();

                return model;
            }
        }

        public virtual int Clear(GeniViewCloudDataRepository db)
        {
            int ret = db.Database.ExecuteSqlRaw("TRUNCATE TABLE InternalDeviceLogs");
            return ret;
        }
        #endregion

        #region Assign and Remove Devices
        public List<AssignRemoveModel> GetAssignedDevices(long communityID)
        {
            var model = new List<AssignRemoveModel>();

            using (var db = new GeniViewCloudDataRepository())
            {

                model = (from m in db.Devices.Include(x => x.Community)
                                             .Where(x => x.Community.ID == communityID)
                         select new AssignRemoveModel()
                         {
                             IsChecked = false,
                             SerialNumber = m.SerialNumber,
                             CommunityName = m.Community.Name
                         }).ToList();
            }
            return model;
        }

        public void RemoveDevices(AssignRemoveListModel model)
        {
            var setList = new List<AssignRemoveModel>();
            if (model.DeviceList != null)
            {
                setList = model.DeviceList.Where(x => x.IsChecked == true).ToList();
            }

            using (var db = new GeniViewCloudDataRepository())
            {

                foreach (var item in setList)
                {
                    var device = db.Devices.Include(x => x.Community)
                                           .Where(x => x.SerialNumber == item.SerialNumber)
                                           .FirstOrDefault();
                    device.Community = null;
                    db.Entry(device).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }

        public List<AssignRemoveModel> GetUnassignedDevices()
        {
            var model = new List<AssignRemoveModel>();

            using (var db = new GeniViewCloudDataRepository())
            {

                model = (from m in db.Devices.Include(x => x.Community)
                                             .Where(x => x.Community.ID == null)
                         select new AssignRemoveModel()
                         {
                             IsChecked = false,
                             SerialNumber = m.SerialNumber
                         }).ToList();
            }
            return model;
        }

        public void AssignDevices(AssignRemoveListModel model)
        {
            var setList = new List<AssignRemoveModel>();
            if (model.DeviceList != null)
            {
                setList = model.DeviceList.Where(x => x.IsChecked == true).ToList();
            }

            using (var db = new GeniViewCloudDataRepository())
            {

                foreach (var item in setList)
                {
                    var device = db.Devices.Include(x => x.Community)
                                           .Where(x => x.SerialNumber == item.SerialNumber)
                                           .FirstOrDefault();
                    device.Community = db.Communities.Find(model.CommunityID);
                    db.Entry(device).State = EntityState.Modified;
                }
                db.SaveChanges();
            }

        }
        #endregion

        #region Activate / Deactivate Devices
        public List<ActivateDeactivatedModel> GetDeactivatedDevices()
        {
            List<ActivateDeactivatedModel> model = new List<ActivateDeactivatedModel>();
            using (var db = new GeniViewCloudDataRepository())
            {

                model = (from m in db.Devices
                                     .Include(x => x.Community)
                                     .Include(x => x.Group)
                                     .Where(x => x.IsDeactivated == true)
                         select new ActivateDeactivatedModel()
                         {
                             IsChecked = false,
                             SerialNumber = m.SerialNumber,
                             CommunityName = m.Community != null ? m.Community.Name : "",
                             GroupName = m.Group != null ? m.Group.Name : ""
                         }).ToList();
            }
            return model;
        }

        public List<ActivateDeactivatedModel> GetActivedDevices()
        {
            List<ActivateDeactivatedModel> model = new List<ActivateDeactivatedModel>();
            using (var db = new GeniViewCloudDataRepository())
            {

                model = (from m in db.Devices
                                     .Include(x => x.Community)
                                     .Include(x => x.Group)
                                     .Where(x => x.IsDeactivated == false)
                         select new ActivateDeactivatedModel()
                         {
                             IsChecked = false,
                             SerialNumber = m.SerialNumber,
                             CommunityName = m.Community != null ? m.Community.Name : "",
                             GroupName = m.Group != null ? m.Group.Name : ""
                         }).ToList();
            }
            return model;
        }

        public void ActivateDeactivateDevices(List<ActivateDeactivatedModel> model, bool isDeactivated)
        {
            var setList = new List<ActivateDeactivatedModel>();
            if (model != null)
            {
                setList = model.Where(x => x.IsChecked == true).ToList();
            }

            using (var db = new GeniViewCloudDataRepository())
            {

                foreach (var item in setList)
                {
                    var device = db.Devices.Where(x => x.SerialNumber == item.SerialNumber).FirstOrDefault();
                    device.IsDeactivated = isDeactivated;
                    db.Entry(device).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}