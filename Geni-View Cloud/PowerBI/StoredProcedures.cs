using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeniView.Cloud.PowerBI
{
    public static class StoredProcedures
    {
        public static string AgentBatteryLogsWithDurationView
        {
            get
            {
                return @"if exists(select 1 from sys.views where name = 'AgentBatteryLogsWithDuration' and type = 'v')
                        drop view AgentBatteryLogsWithDuration
                        exec('
                            Create view AgentBatteryLogsWithDuration as
                            select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					                    else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp)
                                        end as Duration, *
                                    from(
                                        select(
                                            Select MAX(Timestamp)maxTimestamp
                                                from dbo.AgentBatteryLogs t2
                                            where t2.Battery_ID = t1.battery_id

                                                and t2.Timestamp < t1.Timestamp
                                                ) prevRowTimeStamp
                                                , ROW_NUMBER()  OVER(PARTITION BY battery_id, bay order by timestamp)  rk
                                                , *
                                from dbo.AgentBatteryLogs t1
                            )Duration');".Replace("\r\n",String.Empty);
            }
            private set { }
        }

        public static string AgentDeviceLogsWithDurationView
        {
            get
            {
                return @"if exists(select 1 from sys.views where name = 'AgentDeviceLogsWithDuration' and type = 'v')
                         drop view AgentDeviceLogsWithDuration
	                         exec('
		                        Create view AgentDeviceLogsWithDuration as
		                        select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					                        else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) 
					                        end as Duration, *
			                         from (
				                         select (
						                        Select MAX(Timestamp)maxTimestamp
						                          from dbo.AgentDeviceLogs t2
						                        where t2.device_id = t1.device_id
							                        and t2.Timestamp < t1.Timestamp
							                        ) prevRowTimeStamp
							                        , ROW_NUMBER ( )  OVER (PARTITION BY device_id order by timestamp)  rk
							                        , * 
		                          from dbo.AgentDeviceLogs  t1
		                        )Duration');".Replace("\r\n", String.Empty); ;
            }
            private set { }
        }

        public static string InternalBatteryLogsWithDurationView
        {
            get
            {
                return @"if exists(select 1 from sys.views where name = 'InternalBatteryLogsWithDuration' and type = 'v')
                         drop view InternalBatteryLogsWithDuration
	                         exec('
		                        Create view InternalBatteryLogsWithDuration as
		                        select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					                        else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) 
					                        end as Duration, * 
			                         from (
				                         select (
						                        Select MAX(Timestamp)maxTimestamp
						                          from dbo.InternalBatteryLogs t2
						                        where t2.Battery_ID = t1.battery_id
							                        and t2.Timestamp < t1.Timestamp
							                        ) prevRowTimeStamp
							                        , ROW_NUMBER ( )  OVER (PARTITION BY battery_id, bay order by timestamp)  rk
							                        , * 
		                          from dbo.InternalBatteryLogs  t1
		                        )Duration');".Replace("\r\n", String.Empty); ;
            }
            private set { }
        }

        public static string InternalDeviceLogsWithDurationView
        {
            get
            {
                return @"if exists(select 1 from sys.views where name = 'InternalDeviceLogsWithDuration' and type = 'v')
                         drop view InternalDeviceLogsWithDuration
	                         exec('
		                        Create view InternalDeviceLogsWithDuration as
		                        select case when DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) > 3600 then null
					                        else DATEDIFF_BIG(SECOND, prevRowTimeStamp, Timestamp) 
					                        end as Duration, *
			                         from (
				                         select (
						                        Select MAX(Timestamp)maxTimestamp
						                          from dbo.InternalDeviceLogs t2
						                        where t2.device_id = t1.device_id
							                        and t2.Timestamp < t1.Timestamp
							                        ) prevRowTimeStamp
							                        , ROW_NUMBER ( )  OVER (PARTITION BY device_id order by timestamp)  rk
							                        , * 
		                          from dbo.InternalDeviceLogs  t1
		                        )Duration');".Replace("\r\n", String.Empty); ;
            }
            private set { }
        }
    }
}