using GeniView.Data.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GeniView.Cloud.Common
{
	public class DataDefine
	{
		public class G3BatteryDockLog
		{
			public long battery_id { get; set; }
			public string firmware_version { get; set; }
			public string wifi_ssid { get; set; }
			public int wifi_rssi { get; set; }
			public string guid { get; set; }
			public Event_Log event_log { get; set; }

			public List<G3BatteryDockLog> Fack(long id , int qty)
			{
				List<G3BatteryDockLog> result = new List<G3BatteryDockLog>();


				for (int i = 0; i < qty; i++)
				{
					var datetime = DateTime.UtcNow.AddDays(-1).AddSeconds(i);
					var sec = datetime.Second;
					ushort val = ((ushort)(101 * sec));

					var eventLog = new Event_Log()
					{
						Index = (uint)i,
						EventCode = 50,
						TimeStampHi = 1,
						TimeStampLow = 1,
						DockEvent = 50,
						DockSerial = "GTDM005509",
						DockFirmware = 0,
						DockIndexLow = 61635,
						DockIndexHi = 1,
						DockStatus = 1,
						DockVoltage = 1,
						DockCurrent = 1,
						DockTemp = 1,
						//BatteryVoltage = 22002,
						//BatteryCurrent = -27,
						//BatteryAverageCurrent = -27,
						//BatteryRelStateofCharge = 52,
						//BatteryRemainingCapacity = 5401,
						//BatteryCapacity = 10405,
						//BatteryEOLCapacity = 100,
						//BatteryCycleCount = 1,
						//BatteryStatus = 2049,
						//BatteryFlags = 4,
						//BatteryTemp = 3011,
						BatteryVoltage = val,
						BatteryCurrent = (Int16)val,
						BatteryAverageCurrent = (Int16)val,
						BatteryRelStateofCharge = val,
						BatteryRemainingCapacity = val,
						BatteryCapacity = val,
						BatteryEOLCapacity = val,
						BatteryCycleCount = 1,
						BatteryStatus = 2049,
						BatteryFlags = 4,
						BatteryTemp = val,
					};
					
					var log = new G3BatteryDockLog()
					{
						battery_id = id,
						firmware_version = "30891",
						wifi_ssid = "MyWiFiNetwork",
						wifi_rssi = -70,
						guid = "0123456789ABCDEF",
						event_log = eventLog
					};

					var unixTime = DeviceDataHelpers.DateTimeToUnixTime(datetime);
					UInt16 hi = 0;
					UInt16 low = 0;
					DeviceDataHelpers.Uint32ToUint16(unixTime, ref hi, ref low);

					//var test = DeviceDataHelpers.Uint16ToUint32( hi,  low);

					eventLog.TimeStampHi = hi;
					eventLog.TimeStampLow = low;

					result.Add(log);

				}

				return result;

			}
		}

		public class Event_Log
		{
			public UInt32 Index { get; set; }                       // Battery Index - Sequential number created each time a log is created
			public UInt16 EventCode { get; set; }                   // Battery Event Code - Event initiated by Battery
			public UInt16 DockEvent { get; set; }                   // Dock Event Code - Event Initiated by Dock

			public UInt16 TimeStampLow { get; set; }                // Time and date Low
			public UInt16 TimeStampHi { get; set; }                 // Time and date High

			public string DockSerial { get; set; }                 // Dock or Charger Serial Number
			public UInt16 DockFirmware { get; set; }                // Dock Firmware Version
			public UInt16 DockIndexLow { get; set; }                // Dock Index Low - Sequential number created in the dock when the event was triggered
			public UInt16 DockIndexHi { get; set; }                 // Dock Index High - Sequential number created in the dock when the event was triggered
			public UInt16 DockStatus { get; set; }                  // Dock Status
			public UInt16 DockVoltage { get; set; }                 // Dock Output Voltage (in mA)
			public UInt16 DockCurrent { get; set; }                 // Dock Output Current (in mA)
			public UInt16 DockTemp { get; set; }                    // Dock Temperature (in °C)

			public UInt16 BatteryVoltage { get; set; }              // Battery Voltage (Fuel Gauge reading, in mV)
			public Int16 BatteryCurrent { get; set; }              // Battery Current (Fuel Gauge reading, in mA)
			public Int16 BatteryAverageCurrent { get; set; }       // Average Battery Current (Fuel Gauge reading, in mA)
			public UInt16 BatteryRelStateofCharge { get; set; }     // Battery Relative State of Charge (Fuel Gauge reading, in %)
			public UInt16 BatteryRemainingCapacity { get; set; }    // Battery Remaining Capacity (Fuel Gauge reading, in mAh)
			public UInt16 BatteryCapacity { get; set; }             // Battery Calculated Capacity (Fuel Gauge reading, in mAh)
			public UInt16 BatteryEOLCapacity { get; set; }          // Battery End of Life Capacity (Fuel Gauge reading, in %)
			public UInt16 BatteryCycleCount { get; set; }           // Battery Cycle count (Fuel Gauge reading)
			public UInt16 BatteryStatus { get; set; }               // Battery Status (Fuel Gauge reading)
			public UInt16 BatteryFlags { get; set; }                // Battery Flags (Fuel Gauge reading)
			public UInt16 BatteryTemp { get; set; }                 // Battery Temperature (Thermistor, in °C)
			public UInt32 SerialNumber { get; set; }                 // Battery SerailNumber
			public UInt16 FirmwareVersion { get; set; }                 // Battery Firmware Version
		}

		public class G3DockLog
		{

		}

	}
}