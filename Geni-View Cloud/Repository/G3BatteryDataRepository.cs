using GeniView.Cloud.Common;
using Microsoft.EntityFrameworkCore;
using GeniView.Data;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Hardware;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static GeniView.Cloud.Common.DataDefine;

namespace GeniView.Cloud.Repository
{
    public class G3BatteryDataRepository : IDisposable
    {
        private DBHelper _dbHelper = new DBHelper();
        private GeniViewCloudDataRepository _db;

        public G3BatteryDataRepository()
        {

        }

        public G3BatteryDataRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        public static (Device device, Battery battery , List<DeviceEvent> deviceEvents) transfer(string batteryID, List<G3BatteryDockLog> g3Logs)
        {
            // Try to transfer G3BatteryDockLog to Device & Battery
            var device = new Device();
            device.InternalDeviceLogCollection = new List<InternalDeviceLog>();
            device.AgentDeviceLogCollection = new List<AgentDeviceLog>();

            var battery = new Battery();
            battery.InternalBatteryLogCollection = new List<InternalBatteryLog>();
            battery.AgentBatteryLogCollection = new List<AgentBatteryLog>();

            var deviceEvents = new List<DeviceEvent>();

            var dataRepository = new G3BatteryDataRepository();

            foreach (var g3Log in g3Logs)
            {
                //Device part
                //Confirmed

                //Not confirm
                device.SerialNumber = g3Log.event_log.DockSerial;
                device.FirmwareVersion = g3Log.event_log.DockFirmware.ToString();

                InternalDeviceLog internalDeviceLog = dataRepository.transferDeviceLog(g3Log);
                device.InternalDeviceLogCollection.Add(internalDeviceLog);

                AgentDeviceLog agentDeviceLog = dataRepository.transferAgentDeviceLog(g3Log);
                device.AgentDeviceLogCollection.Add(agentDeviceLog);

                //Battery part
                //Confirmed
                battery.SerialNumber = batteryID;

                //Not confirm
                battery.FirmwareVersion = -1;

                battery.SerialNumberCode = g3Log.battery_id;

                InternalBatteryLog internalBattery = dataRepository.transferBatteryLog(g3Log);
                battery.InternalBatteryLogCollection.Add(internalBattery);

                AgentBatteryLog agentBatteryLog = dataRepository.transferAgentBatteryLog(g3Log);
                battery.AgentBatteryLogCollection.Add(agentBatteryLog);

                var deviceEvent = dataRepository.transferDeviceEvent(g3Log);
                deviceEvents.Add(deviceEvent);
            }

            return (device, battery , deviceEvents);
        }

        public bool CreateBatch(List<InternalBatteryLog> logs, GeniViewCloudDataRepository db)
        {
            var ret = false;

            _dbHelper.BatchInsert(db, db.InternalBatteryLog, logs);

            ret = true;
            return ret;
        }

        public bool CreateBatch(List<InternalDeviceLog> logs, GeniViewCloudDataRepository db)
        {
            var ret = false;

            _dbHelper.BatchInsert(db, db.InternalDeviceLog, logs);

            ret = true;

            return ret;
        }


        public bool CreateBatch(List<AgentBatteryLog> logs, GeniViewCloudDataRepository db)
        {
            var ret = false;

            _dbHelper.BatchInsert(db, db.AgentBatteryLog, logs);

            ret = true;
            return ret;
        }

        public bool CreateBatch(List<AgentDeviceLog> logs, GeniViewCloudDataRepository db)
        {
            var ret = false;

            _dbHelper.BatchInsert(db, db.AgentDeviceLog, logs);

            ret = true;
            return ret;
        }


        public void CreateDevice(Device device, GeniViewCloudDataRepository db)
        {
            try
            {
                var result = db.Devices.Add(device);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateDevice(Device model, GeniViewCloudDataRepository db)
        {
            try
            {
                var fromDB = db.Devices.Find(model.SerialNumber);
                if (fromDB != null)
                {
                    db.Entry(fromDB).CurrentValues.SetValues(model); //Set Vaule
                    var result = db.SaveChanges();                   //Save
                    db.Entry(fromDB).Reload();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateBattery(Battery battery, GeniViewCloudDataRepository db)
        {
            try
            {
                var result = db.Batteries.Add(battery);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateBattery(Battery model, GeniViewCloudDataRepository db)
        {
            try
            {
                var fromDB = db.Batteries.Find(model.SerialNumber);
                if (fromDB != null)
                {
                    db.Entry(fromDB).CurrentValues.SetValues(model); //Set Vaule
                    var result = db.SaveChanges();                   //Save
                    db.Entry(fromDB).Reload();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual int Clear(GeniViewCloudDataRepository db)
        {
            int ret = db.Database.ExecuteSqlRaw("TRUNCATE TABLE InternalBatteryLogs");
            return ret;
        }

        public IEnumerable<InternalBatteryLog> Query(
            string bsn, string dsn, string startTime, string endTime, 
            long? idStart, long? idEnd, int? eventcode, string eventcodetxt, string batterystatus, 
            GeniViewCloudDataRepository db)
        {
            try
            {
                var ret = db.InternalBatteryLog.AsNoTracking().AsQueryable();

                var startCheck = DateTime.TryParse(startTime, out DateTime start);
                var endCheck = DateTime.TryParse(endTime, out DateTime end);

                if (bsn != null && bsn.Any() == true)
                {
                    ret = ret.Where(x => x.BatterySerialNumber == bsn);
                }

                if (dsn != null && dsn.Any() == true)
                {
                    ret = ret.Where(x => x.DeviceSerialNumber == dsn);
                }

                if (startCheck == true && endCheck == true)
                {
                    ret = ret.Where(x => x.Timestamp >= start && x.Timestamp <= end);
                }

                if (idStart > 0 && idEnd > 0)
                {
                    ret = ret.Where(x => x.ID >= idStart && x.ID <= idEnd);
                }

                if (eventcode > 0 )
                {
                    ret = ret.Where(x => (int)x.EventCode == eventcode);
                }

                if (string.IsNullOrEmpty(eventcodetxt) == false)
                {
                    ret = ret.Where(x => x.EventCodeText.Contains(eventcodetxt) == true);
                }

                if (string.IsNullOrEmpty(batterystatus) == false)
                {
                    ret = ret.Where(x => x.BatteryOperatingStatus.StatusAText.Contains(batterystatus) == true);
                }

                return ret.OrderByDescending(x => x.Timestamp);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void Dispose()
        {
            GC.Collect();
        }

        //private List<InternalBatteryLog> transferLog(List<G3BatteryDockLog> logs)
        //{
        //    var retLogs = new List<InternalBatteryLog>();
        //    foreach (var data in logs)
        //    {
        //        var log = new InternalBatteryLog();
        //        // Confirmed
        //        log.EventCode = Enum.IsDefined(typeof(InternalDeviceLogEventCodes), data.EventCode) ? (InternalDeviceLogEventCodes)data.EventCode : InternalDeviceLogEventCodes.Unknown;
        //        log.Voltage = data.BatteryVoltage;
        //        log.Current = data.BatteryCurrent;
        //        log.AverageCurrent = data.BatteryAverageCurrent;
        //        log.RelativeStateOfCharge = (int)data.BatteryRelStateofCharge;
        //        log.RemainingCapacity = data.BatteryRemainingCapacity;
        //        log.CycleCount = (int)data.BatteryCycleCount;
        //        log.Temperature = (int)data.BatteryTemp;



        //        // Not Confirm
        //        log.Timestamp = DateTime.Now;
        //        log.BatterySerialNumber = "A12346789";
        //        log.DeviceSerialNumber = data.DockSerial;


        //        log.DeviceLogIndex = (long)(((ulong)data.DockIndexHi << 32) | data.DockIndexLow);
        //        log.BatteryFirmwareVersion = -1;


        //        retLogs.Add(log);
        //    }

        //    return retLogs;
        //}


        private InternalBatteryLog transferBatteryLog(G3BatteryDockLog g3Log)
        {
            var log = new InternalBatteryLog();
            // Confirmed
            log.LogIndex = g3Log.event_log.Index;
            log.BatterySerialNumber = DataHelpers.BatterySerialNumberCodeToSerialNumber(g3Log.battery_id);
            log.BatterySerialNumberCode = (long)g3Log.battery_id;
            var bayIdx = (g3Log.event_log.BatteryStatus & 0x700) >> 8;
            log.Bay = bayIdx;
            var eventCode = Enum.IsDefined(typeof(InternalDeviceLogEventCodes), (int)g3Log.event_log.EventCode) ? (InternalDeviceLogEventCodes)(int)g3Log.event_log.EventCode : InternalDeviceLogEventCodes.Unknown;
            log.EventCode = eventCode;
            log.EventCodeRaw = (int)g3Log.event_log.EventCode;
            log.Voltage = (double)g3Log.event_log.BatteryVoltage / 1000;
            log.Current = (double)g3Log.event_log.BatteryCurrent / 1000;
            log.AverageCurrent = (double)g3Log.event_log.BatteryAverageCurrent / 1000;
            log.RelativeStateOfCharge = g3Log.event_log.BatteryRelStateofCharge;
            log.RemainingCapacity = (double)g3Log.event_log.BatteryRemainingCapacity / 1000;
            log.CalculatedCapacity = (double)g3Log.event_log.BatteryCapacity / 1000;
            log.CycleCount = g3Log.event_log.BatteryCycleCount;
            log.Temperature = g3Log.event_log.BatteryTemp / 100;
            log.BatteryOperatingStatus = new BatteryOperatingStatus(g3Log.event_log.BatteryStatus, g3Log.event_log.BatteryFlags);



            // Not Confirm
            UInt32 deviceLogIdx = ((UInt32)g3Log.event_log.DockIndexHi << 16) | g3Log.event_log.DockIndexLow;
            log.DeviceLogIndex = (long)deviceLogIdx;
            log.UniqueLogIndex = g3Log.event_log.Index.ToString() + deviceLogIdx.ToString() + "B"; //battery log index + device log index + "B";
            UInt32 timestamp = ((UInt32)g3Log.event_log.TimeStampHi << 16) | g3Log.event_log.TimeStampLow;
            log.Timestamp = DeviceDataHelpers.UnixTimeToDateTime(timestamp);
            //var strTime = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            log.DeviceSerialNumber = g3Log.event_log.DockSerial;
            log.EventCodeText = EnumHelper.GetFriendlyText(eventCode, ",");
            log.BatteryFirmwareVersion = g3Log.event_log.FirmwareVersion != null ? g3Log.event_log.FirmwareVersion : 0;

            var status = g3Log.event_log.BatteryStatus > 4 ? 0 : g3Log.event_log.BatteryStatus;
            log.Status = (BatteryStates)status; //DeviceDataHelpers.SetInternalBatteryLogStatus(BayIdx, BatteriesPoweringSystemRaw, BatteriesChargingRaw);
            log.StatusText = EnumHelper.GetFriendlyText(log.Status, ",");



            return log;
        }

        private InternalDeviceLog transferDeviceLog(G3BatteryDockLog g3Log)
        {
            var log = new InternalDeviceLog();
            // Confirmed
            UInt32 deviceLogIdx = ((UInt32)g3Log.event_log.DockIndexHi << 16) | g3Log.event_log.DockIndexLow;
            log.LogIndex = (long)deviceLogIdx;
            var eventCode = Enum.IsDefined(typeof(InternalDeviceLogEventCodes), (int)g3Log.event_log.DockEvent) ? (InternalDeviceLogEventCodes)(int)g3Log.event_log.DockEvent : InternalDeviceLogEventCodes.Unknown;
            log.EventCode = eventCode;
            log.EventCodeRaw = (int)g3Log.event_log.DockEvent;
            log.EventCodeText = EnumHelper.GetFriendlyText(eventCode, ",");
            log.PowerOutput = new DevicePower();
            log.PowerOutput.Voltage = (double)g3Log.event_log.DockVoltage/1000;
            log.PowerOutput.Current = (double)g3Log.event_log.DockCurrent/1000;
            log.Status = new DeviceStatus(g3Log.event_log.DockStatus, g3Log.event_log.DockTemp);




            // Not Confirm
            UInt32 timestamp = ((UInt32)g3Log.event_log.TimeStampHi << 16) | g3Log.event_log.TimeStampLow;
            log.Timestamp = DeviceDataHelpers.UnixTimeToDateTime(timestamp);
            //var strTime = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            log.DeviceSerialNumber = g3Log.event_log.DockSerial;

            //No value
            log.BatteriesChargingRaw = 0;
            log.BatteriesCharging = Batteries.None; 
            log.BatteriesChargingCount = 0;
            log.BatteriesChargingText = "";

            log.BatteriesPoweringSystemRaw = 0;
            log.BatteriesPoweringSystem = Batteries.None;
            log.BatteriesPoweringSystemCount = 0;
            log.BatteriesPoweringSystemText = "";

            log.BatteriesPresentRaw = 0;
            log.BatteriesPresent = Batteries.None;
            log.BatteriesPresentCount = 0;
            log.BatteriesPresentText = "";

            return log;
        }

        private AgentBatteryLog transferAgentBatteryLog(G3BatteryDockLog g3Log)
        {
            var log = new AgentBatteryLog();
            // Confirmed
            var bayIdx                       = (g3Log.event_log.BatteryStatus & 0x700) >> 8;
            log.Bay                          = bayIdx;
            log.OperatingData                = new BatteryOperatingData();
            log.OperatingData.Voltage        = (double)g3Log.event_log.BatteryVoltage / 1000;
            log.OperatingData.Current        = (double)g3Log.event_log.BatteryCurrent / 1000;
            log.OperatingData.AverageCurrent = (double)g3Log.event_log.BatteryAverageCurrent / 1000;
            log.OperatingData.CycleCount     = g3Log.event_log.BatteryCycleCount;

            BatteryOperatingStatus batteryOP         = new BatteryOperatingStatus(g3Log.event_log.BatteryStatus, g3Log.event_log.BatteryFlags);
            log.OperatingData.BatteryOperatingStatus = batteryOP;

            log.SlowChangingDataA                       = new BatterySlowChangingDataA();
            log.SlowChangingDataA.RelativeStateOfCharge = g3Log.event_log.BatteryRelStateofCharge;
            log.SlowChangingDataA.AbsoluteStateOfCharge = 0;
            log.SlowChangingDataA.RemainingCapacity     = (double)g3Log.event_log.BatteryRemainingCapacity / 1000;
            log.SlowChangingDataA.EndOfLifeCapacity     = g3Log.event_log.BatteryEOLCapacity;
            log.SlowChangingDataA.DischargeCapacity     = 0;
            log.SlowChangingDataA.CalculatedCapacity    = (double)g3Log.event_log.BatteryCapacity / 1000;


            log.SlowChangingDataB                            = new BatterySlowChangingDataB();
            log.SlowChangingDataB.PassedCharge               = 0;
            log.SlowChangingDataB.BatteryInternalTemperature = g3Log.event_log.BatteryTemp / 100;
            log.SlowChangingDataB.AverageTimeToEmpty         = 0;
            log.SlowChangingDataB.AverageTimeToFull          = 0;
            log.SlowChangingDataB.AvailableEnergy            = 0;
            log.SlowChangingDataB.AvailablePower             = 0;
            log.SlowChangingDataB.DisplayBoardTemperature    = 0;
            log.SlowChangingDataB.LifeTime                   = 0;


            log.SlowChangingDataB.DisplayBoardStatus                        = new BatteryDisplayBoardStatus();
            log.SlowChangingDataB.DisplayBoardStatus.PerceivedStateOfCharge = 0;
            log.SlowChangingDataB.DisplayBoardStatus.ActiveLEDs             = 0;
            log.SlowChangingDataB.DisplayBoardStatus.IsDisplayFlashing      = null;
            log.SlowChangingDataB.DisplayBoardStatus.IsDisplayRed           = null;
            log.SlowChangingDataB.DisplayBoardStatus.ChargeMode             = FuelGaugeChargeModes.Unknown;
            log.SlowChangingDataB.DisplayBoardStatus.StatusRaw              = 0;


            log.TimeEstimate                     = new BatteryTimeEstimate();
            log.TimeEstimate.Estimate            = 0;
            log.TimeEstimate.EstimateMode        = BatteryTimeEstimateModes.Discharging;
            log.TimeEstimate.EstimateDescription = "";


            var status     = g3Log.event_log.BatteryStatus > 4 ? 0 : g3Log.event_log.BatteryStatus;
            log.Status     = (BatteryStates)status; //DeviceDataHelpers.SetInternalBatteryLogStatus(BayIdx, BatteriesPoweringSystemRaw, BatteriesChargingRaw);
            log.StatusText = EnumHelper.GetFriendlyText(log.Status, ",");

            log.InternalBatteryLogCount = g3Log.event_log.Index;
            log.DeviceSerialNumber      = g3Log.event_log.DockSerial;

            UInt32 timestamp = ((UInt32)g3Log.event_log.TimeStampHi << 16) | g3Log.event_log.TimeStampLow;
            log.Timestamp    = DeviceDataHelpers.UnixTimeToDateTime(timestamp);


            // Not Confirm
            //UInt32 deviceLogIdx = ((UInt32)g3Log.event_log.DockIndexHi << 16) | g3Log.event_log.DockIndexLow;
            //log.DeviceLogIndex = (long)deviceLogIdx;
            //log.UniqueLogIndex = g3Log.event_log.Index.ToString() + deviceLogIdx.ToString() + "B"; //battery log index + device log index + "B";
           

            //var strTime = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            //log.EventCodeText = EnumHelper.GetFriendlyText(eventCode, ",");
            //log.BatteryFirmwareVersion = g3Log.event_log.FirmwareVersion != null ? g3Log.event_log.FirmwareVersion : 0;

            return log;
        }

        private AgentDeviceLog transferAgentDeviceLog(G3BatteryDockLog g3Log)
        {
            var log = new AgentDeviceLog();

            log.BayData = new List<DeviceBay>();
            log.Status = new DeviceStatus(g3Log.event_log.DockStatus, g3Log.event_log.DockTemp);
            log.Status.Temperature = g3Log.event_log.BatteryTemp / 100;

            var test = new BatteryOperatingStatus(g3Log.event_log.BatteryStatus, g3Log.event_log.BatteryFlags);
            var bayIdx = (g3Log.event_log.BatteryStatus & 0x700) >> 8;

            log.PowerInput = new DevicePower();

            log.PowerOutput = new DevicePower();
            log.PowerOutput.Voltage = (double)g3Log.event_log.DockVoltage / 1000;
            log.PowerOutput.Current = (double)g3Log.event_log.DockCurrent / 1000;

            log.DeviceCapacity = g3Log.event_log.BatteryCapacity / 1000;

            log.TimeEstimate = new DeviceTimeEstimate();
            log.TimeEstimate.Estimate = 0;
            log.TimeEstimate.EstimateMode = DeviceTimeEstimateModes.NoBattery;
            log.TimeEstimate.EstimateDescription = "";

            log.IsExternalPowerInputApplied = null;
            log.IsExternalPowerInputNotGood = null;
            log.BatteriesPresentText = "";
            log.BatteriesPresent = Batteries.None;
            log.BatteriesPresentCount = 0;
            log.BatteriesPoweringSystemText = "";

            log.BatteriesPoweringSystem = Batteries.None;

            log.BatteriesPoweringSystemCount = 0;
            log.BatteriesChargingText = "";
            log.BatteriesCharging = Batteries.None;
            log.BatteriesChargingCount = 0;

            UInt32 deviceLogIdx = ((UInt32)g3Log.event_log.DockIndexHi << 16) | g3Log.event_log.DockIndexLow;
            log.InternalDeviceLogCount = (long)deviceLogIdx;

            log.Location = new DeviceLocation();

            UInt32 timestamp = ((UInt32)g3Log.event_log.TimeStampHi << 16) | g3Log.event_log.TimeStampLow; // Not Confirm
            log.Timestamp = DeviceDataHelpers.UnixTimeToDateTime(timestamp);


            var eventCode = Enum.IsDefined(typeof(InternalDeviceLogEventCodes), (int)g3Log.event_log.DockEvent) ? (InternalDeviceLogEventCodes)(int)g3Log.event_log.DockEvent : InternalDeviceLogEventCodes.Unknown;

            return log;
        }

        private DeviceEvent transferDeviceEvent(G3BatteryDockLog g3Log)
        {
            DeviceEvent deviceEvent = new DeviceEvent();
            G3Definitions g3Definitions = new G3Definitions();

            deviceEvent = g3Definitions.GetEvent((InternalDeviceLogEventCodes)g3Log.event_log.EventCode);

            var DeviceSerialNumber = g3Log.event_log.DockSerial;
            var BatterySerialNumber = DataHelpers.BatterySerialNumberCodeToSerialNumber(g3Log.battery_id);
            //deviceEvent.DeviceSerialNumber = BatterySerialNumber.Length >= 1 ? BatterySerialNumber : DeviceSerialNumber;
            deviceEvent.DeviceSerialNumber = DeviceSerialNumber;

            deviceEvent.BatterySerialNumber = BatterySerialNumber;

            UInt32 timestamp = ((UInt32)g3Log.event_log.TimeStampHi << 16) | g3Log.event_log.TimeStampLow; // Not Confirm
            deviceEvent.Timestamp = DeviceDataHelpers.UnixTimeToDateTime(timestamp);
             

            return deviceEvent;
        }
    }
}