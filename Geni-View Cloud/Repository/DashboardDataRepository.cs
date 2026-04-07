using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Hardware;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using GeniView.Cloud.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace GeniView.Cloud.Repository
{
    public class DashboardDataRepository : IDisposable
    {
        private const string CycleStatusStoredProcedureName = "dbo.usp_GetLatestBatteryCycleCount";
        private const string StateOfChargeStoredProcedureName = "dbo.usp_GetLatestBatteryStateOfCharge";
        private const string EffectiveRotationStoredProcedureName = "dbo.usp_GetLatestBatteryTimestamp";
        private const string TemperatureStoredProcedureName = "dbo.usp_GetLatestBatteryTemperature";
        private const string BatteryEfficiencyStoredProcedureName = "dbo.usp_GetLatestBatteryEfficiency";
        private const string BatteryActivityHistoryStoredProcedureName = "dbo.usp_GetBatteryActivityHistory";
        private const string DeviceActivityHistoryStoredProcedureName = "dbo.usp_GetDeviceActivityHistory";
        private const string LatestBatteryStatusStoredProcedureName = "dbo.usp_GetLatestBatteryStatus";


        



        private sealed class CycleStatusRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public int? OperatingData_CycleCount { get; set; }
        }

        private sealed class StateOfChargeRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public int SlowChangingDataA_RelativeStateOfCharge { get; set; }
        }

        private sealed class EffectiveRotationRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private sealed class TemperatureRow
        {
            public long Battery_ID { get; set; }

            public int SlowChangingDataB_BatteryInternalTemperature { get; set; }

            public int EventCode { get; set; }
        }

        private sealed class BatteryEfficiencyRow
        {
            public long Battery_ID { get; set; }
            public double? SlowChangingDataA_RemainingCapacity { get; set; }
            public double? Remaining_Capacity { get; set; }
            public int EventCode { get; set; }
        }

        private sealed class BatteryActivityHistoryRow
        {
            public DateTime ActivityDate { get; set; }
            public int TotalBatteriesInScope { get; set; }
            public int BatteriesOnline { get; set; }
            public int BatteriesOffline { get; set; }
        }

        private sealed class DeviceActivityHistoryRow
        {
            public DateTime ActivityDate { get; set; }
            public int TotalDevicesInScope { get; set; }
            public int DevicesOnline { get; set; }
            public int DevicesOffline { get; set; }
        }

        private sealed class LatestBatteryStatusRow
        {
            public long Battery_ID { get; set; }
            public DateTime Timestamp { get; set; }
            public string DeviceSerialNumber { get; set; }
            public double? OperatingData_Current { get; set; }
            public string BatteryStatus { get; set; }
        }

        private Random randomDouble = new Random();

        #region Dashboard

        public CycleStatusModel GetCycleStatus(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 1️⃣ Get latest cycle count per Battery_ID (NO filters here)
                var spRows = db.Database
                    .SqlQueryRaw<CycleStatusRow>("EXEC " + CycleStatusStoredProcedureName)
                    .ToList();

                // 2️⃣ Resolve batteries that belong to current filter scope
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = b.CommunityID,
                        GroupID = b.GroupID
                    })
                    .ToList();

                if (communityID != null)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.CommunityID == communityID)
                        .ToList();
                }

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups
                        .Select(g => g.ID)
                        .ToHashSet();

                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null && !includeAllSubGroups)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID == groupID)
                        .ToList();
                }

                var allowedBatteryIds = batteryQuery
                    .Select(x => x.BatteryID)
                    .ToHashSet();

                // 3️⃣ STRICT filtering — no fallback
                var rows = spRows
                    .Where(r => allowedBatteryIds.Contains(r.Battery_ID))
                    .ToList();

                // 4️⃣ If no batteries in scope → ZERO widget
                if (allowedBatteryIds.Count == 0 || rows.Count == 0)
                {
                    return new CycleStatusModel
                    {
                        LowCount = 0,
                        HighCount = 0,
                        EndOfLifeCount = 0,
                        TotalCount = 0,
                        PowerModulesCount = allowedBatteryIds.Count,
                        AverageCycleCount = 0,
                        LowPercent = 0,
                        HighPercent = 0,
                        EndOfLifePercent = 0
                    };
                }

                // 5️⃣ Existing logic + capture Battery_IDs per bucket
                var low = 0;
                var high = 0;
                var eol = 0;

                var lowIds = new List<long>();
                var highIds = new List<long>();
                var eolIds = new List<long>();

                var validCycleSum = 0;
                var validCycleCount = 0;

                foreach (var row in rows)
                {
                    if (!row.OperatingData_CycleCount.HasValue)
                    {
                        continue;
                    }

                    validCycleSum += row.OperatingData_CycleCount.Value;
                    validCycleCount++;

                    var cycleCount = row.OperatingData_CycleCount.Value;

                    if (cycleCount < 500)
                    {
                        low++;
                        lowIds.Add(row.Battery_ID);
                    }
                    else if (cycleCount <= 1000)
                    {
                        high++;
                        highIds.Add(row.Battery_ID);
                    }
                    else
                    {
                        eol++;
                        eolIds.Add(row.Battery_ID);
                    }
                }

                var total = low + high + eol;

                var result = new CycleStatusModel
                {
                    LowCount = low,
                    HighCount = high,
                    EndOfLifeCount = eol,
                    TotalCount = total,
                    PowerModulesCount = allowedBatteryIds.Count,
                    AverageCycleCount = validCycleCount > 0
                        ? (int)Math.Round((decimal)validCycleSum / validCycleCount, MidpointRounding.AwayFromZero)
                        : 0,

                    LowBatteryIds = lowIds,
                    HighBatteryIds = highIds,
                    EndOfLifeBatteryIds = eolIds
                };

                if (total > 0)
                {
                    result.LowPercent = Math.Round((decimal)low * 100m / total, 2);
                    result.HighPercent = Math.Round((decimal)high * 100m / total, 2);
                    result.EndOfLifePercent = Math.Round((decimal)eol * 100m / total, 2);
                }

                return result;
            }
        }


        public StateOfChargeModel GetStateOfCharge(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 1️⃣ SP returns latest SoC per Battery_ID (GLOBAL)
                var spRows = db.Database
                    .SqlQueryRaw<StateOfChargeRow>("EXEC " + StateOfChargeStoredProcedureName)
                    .ToList();

                // 2️⃣ Resolve batteries IN SCOPE (Community / Group / SubGroups)
                var batteryQuery = db.Batteries
                    .AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = b.CommunityID,
                        GroupID = b.GroupID
                    })
                    .ToList();

                if (communityID != null)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.CommunityID == communityID)
                        .ToList();
                }

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupDb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupDb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups
                        .Select(g => g.ID)
                        .ToHashSet();

                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID == groupID)
                        .ToList();
                }

                // 3️⃣ Battery IDs allowed by filters
                var allowedBatteryIds = batteryQuery
                    .Select(x => x.BatteryID)
                    .ToHashSet();

                // 🚨 CRITICAL FIX:
                // NO fallback to global SP rows
                var rows = spRows
                    .Where(r => allowedBatteryIds.Contains(r.Battery_ID))
                    .ToList();

                // 4️⃣ Widget calculations (UNCHANGED LOGIC) + capture Battery_IDs per bucket
                int high = 0;
                int low = 0;
                int chargeNow = 0;
                int validSocSum = 0;
                int validSocCount = 0;

                var highIds = new List<long>();
                var lowIds = new List<long>();
                var chargeNowIds = new List<long>();

                foreach (var row in rows)
                {
                    int soc = row.SlowChangingDataA_RelativeStateOfCharge;

                    if (soc >= 0 && soc <= 100)
                    {
                        validSocSum += soc;
                        validSocCount++;
                    }

                    if (soc > 70)
                    {
                        high++;
                        highIds.Add(row.Battery_ID);
                    }
                    else if (soc >= 30)
                    {
                        low++;
                        lowIds.Add(row.Battery_ID);
                    }
                    else
                    {
                        chargeNow++;
                        chargeNowIds.Add(row.Battery_ID);
                    }
                }

                int total = high + low + chargeNow;

                var result = new StateOfChargeModel
                {
                    HighSoCCount = high,
                    LowSoCCount = low,
                    ChargeNowCount = chargeNow,
                    TotalCount = total,

                    // ✅ CORRECT: scope-based count ONLY
                    PowerModulesCount = allowedBatteryIds.Count,

                    AverageSoC = validSocCount > 0
                        ? (int)Math.Round(
                            (decimal)validSocSum / validSocCount,
                            MidpointRounding.AwayFromZero)
                        : 0,

                    HighSoCBatteryIds = highIds,
                    LowSoCBatteryIds = lowIds,
                    ChargeNowBatteryIds = chargeNowIds
                };

                if (total > 0)
                {
                    result.HighSoCPercent = Math.Round((decimal)high * 100m / total, 2);
                    result.LowSoCPercent = Math.Round((decimal)low * 100m / total, 2);
                    result.ChargeNowPercent = Math.Round((decimal)chargeNow * 100m / total, 2);
                }
                else
                {
                    // Explicit zeroing for empty scope
                    result.HighSoCPercent = 0;
                    result.LowSoCPercent = 0;
                    result.ChargeNowPercent = 0;
                }

                return result;
            }
        }



        public EffectiveRotationModel GetEffectiveRotation(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 1️⃣ Get latest timestamp per Battery_ID (NO filters here)
                var spRows = db.Database
                    .SqlQueryRaw<EffectiveRotationRow>("EXEC " + EffectiveRotationStoredProcedureName)
                    .ToList();

                // 2️⃣ Build battery scope using filters
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = b.CommunityID,
                        GroupID = b.GroupID
                    })
                    .ToList();

                if (communityID != null)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.CommunityID == communityID)
                        .ToList();
                }

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    List<Group> allChildrenGroups;
                    using (var groupdb = new GroupsDataRepository())
                    {
                        allChildrenGroups = groupdb.GetGroups(communityID, groupID);
                    }

                    var includedGroupIds = allChildrenGroups
                        .Select(g => g.ID)
                        .ToHashSet();

                    includedGroupIds.Add(groupID.Value);

                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID != null && includedGroupIds.Contains(x.GroupID.Value))
                        .ToList();
                }
                else if (groupID != null)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID == groupID)
                        .ToList();
                }

                var allowedBatteryIds = batteryQuery
                    .Select(x => x.BatteryID)
                    .ToHashSet();

                // 🚨 STRICT SCOPE RULE
                if (allowedBatteryIds.Count == 0)
                {
                    return new EffectiveRotationModel
                    {
                        GoodCount = 0,
                        AverageCount = 0,
                        PoorCount = 0,
                        TotalCount = 0,
                        PowerModulesCount = 0,
                        EfficiencyScorePercent = 0,
                        GoodPercent = 0,
                        AveragePercent = 0,
                        PoorPercent = 0
                    };
                }

                // 3️⃣ Apply scope AFTER SP
                var rows = spRows
                    .Where(r => allowedBatteryIds.Contains(r.Battery_ID))
                    .ToList();

                var nowUtc = DateTime.UtcNow;

                var good = 0;
                var average = 0;
                var poor = 0;

                var goodIds = new List<long>();
                var averageIds = new List<long>();
                var poorIds = new List<long>();

                foreach (var row in rows)
                {
                    var rowUtc = row.Timestamp.Kind == DateTimeKind.Utc
                        ? row.Timestamp
                        : row.Timestamp.ToUniversalTime();

                    var days = (nowUtc - rowUtc).TotalDays;

                    if (days < 5d)
                    {
                        good++;
                        goodIds.Add(row.Battery_ID);
                    }
                    else if (days <= 10d)
                    {
                        average++;
                        averageIds.Add(row.Battery_ID);
                    }
                    else
                    {
                        poor++;
                        poorIds.Add(row.Battery_ID);
                    }
                }

                var total = good + average + poor;
                var powerModulesCount = allowedBatteryIds.Count;

                var result = new EffectiveRotationModel
                {
                    GoodCount = good,
                    AverageCount = average,
                    PoorCount = poor,
                    TotalCount = total,
                    PowerModulesCount = powerModulesCount,
                    EfficiencyScorePercent = powerModulesCount > 0
                        ? (int)Math.Round(((decimal)good * 100m) / powerModulesCount, MidpointRounding.AwayFromZero)
                        : 0,

                    GoodBatteryIds = goodIds,
                    AverageBatteryIds = averageIds,
                    PoorBatteryIds = poorIds
                };

                if (total > 0)
                {
                    result.GoodPercent = Math.Round((decimal)good * 100m / total, 2);
                    result.AveragePercent = Math.Round((decimal)average * 100m / total, 2);
                    result.PoorPercent = Math.Round((decimal)poor * 100m / total, 2);
                }

                return result;
            }
        }


        public TemperatureModel GetTemperature(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 1️⃣ SP → latest temperature per Battery_ID
                var spRows = db.Database
                    .SqlQueryRaw<TemperatureRow>("EXEC " + TemperatureStoredProcedureName)
                    .ToList();

                // 2️⃣ Build battery scope
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = b.CommunityID,
                        GroupID = b.GroupID
                    })
                    .ToList();

                if (communityID != null)
                    batteryQuery = batteryQuery.Where(x => x.CommunityID == communityID).ToList();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    using (var groupdb = new GroupsDataRepository())
                    {
                        var allGroups = groupdb.GetGroups(communityID, groupID)
                                       .Select(g => g.ID)
                                       .ToHashSet();
                        allGroups.Add(groupID.Value);

                        batteryQuery = batteryQuery
                            .Where(x => x.GroupID != null && allGroups.Contains(x.GroupID.Value))
                            .ToList();
                    }
                }
                else if (groupID != null)
                {
                    batteryQuery = batteryQuery.Where(x => x.GroupID == groupID).ToList();
                }

                var allowedBatteryIds = batteryQuery.Select(x => x.BatteryID).ToHashSet();

                // 🚨 STRICT SCOPE
                if (allowedBatteryIds.Count == 0)
                {
                    return new TemperatureModel
                    {
                        PowerModulesCount = 0,
                        ChargingNormalCount = 0,
                        ChargingWarningCount = 0,
                        DischargingNormalCount = 0,
                        DischargingWarningCount = 0,
                        TotalValidTempCount = 0,
                        EfficiencyScorePercent = 0,
                        NormalPercent = 0,
                        WarningPercent = 0
                    };
                }

                // 3️⃣ Apply scope AFTER SP
                var rows = spRows
                    .Where(r => allowedBatteryIds.Contains(r.Battery_ID))
                    .ToList();

                var chargingNormal = 0;
                var chargingWarning = 0;
                var dischargingNormal = 0;
                var dischargingWarning = 0;
                var validTempCount = 0;
                var totalNormalCount = 0;

                var chargingNormalIds = new List<long>();
                var chargingWarningIds = new List<long>();
                var dischargingNormalIds = new List<long>();
                var dischargingWarningIds = new List<long>();

                foreach (var row in rows)
                {
                    var isCharging = row.EventCode == 3;
                    var temp = (double)row.SlowChangingDataB_BatteryInternalTemperature;

                    validTempCount++;

                    if (isCharging)
                    {
                        if (temp <= 35)
                        {
                            chargingNormal++;
                            totalNormalCount++;
                            chargingNormalIds.Add(row.Battery_ID);
                        }
                        else
                        {
                            chargingWarning++;
                            chargingWarningIds.Add(row.Battery_ID);
                        }
                    }
                    else
                    {
                        if (temp <= 30)
                        {
                            dischargingNormal++;
                            totalNormalCount++;
                            dischargingNormalIds.Add(row.Battery_ID);
                        }
                        else
                        {
                            dischargingWarning++;
                            dischargingWarningIds.Add(row.Battery_ID);
                        }
                    }
                }

                var totalWarningCount = chargingWarning + dischargingWarning;

                var result = new TemperatureModel
                {
                    PowerModulesCount = allowedBatteryIds.Count,
                    ChargingNormalCount = chargingNormal,
                    ChargingWarningCount = chargingWarning,
                    DischargingNormalCount = dischargingNormal,
                    DischargingWarningCount = dischargingWarning,
                    TotalValidTempCount = validTempCount,
                    EfficiencyScorePercent = validTempCount > 0
                        ? (int)Math.Round((decimal)totalNormalCount * 100m / validTempCount, MidpointRounding.AwayFromZero)
                        : 0,

                    ChargingNormalBatteryIds = chargingNormalIds,
                    ChargingWarningBatteryIds = chargingWarningIds,
                    DischargingNormalBatteryIds = dischargingNormalIds,
                    DischargingWarningBatteryIds = dischargingWarningIds
                };

                if (validTempCount > 0)
                {
                    result.NormalPercent = Math.Round((decimal)totalNormalCount * 100m / validTempCount, 2);
                    result.WarningPercent = Math.Round((decimal)totalWarningCount * 100m / validTempCount, 2);
                }

                return result;
            }
        }


        public BatteryEfficiencyModel GetBatteryEfficiency(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 1️⃣ SP → latest efficiency data per Battery_ID
                var spRows = db.Database
                    .SqlQueryRaw<BatteryEfficiencyRow>("EXEC " + BatteryEfficiencyStoredProcedureName)
                    .ToList();

                // 2️⃣ Build battery scope (same as all other widgets)
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = b.CommunityID,
                        GroupID = b.GroupID
                    })
                    .ToList();

                if (communityID != null)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.CommunityID == communityID)
                        .ToList();
                }

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    using (var groupdb = new GroupsDataRepository())
                    {
                        var allGroupIds = groupdb
                            .GetGroups(communityID, groupID)
                            .Select(g => g.ID)
                            .ToHashSet();

                        allGroupIds.Add(groupID.Value);

                        batteryQuery = batteryQuery
                            .Where(x => x.GroupID != null && allGroupIds.Contains(x.GroupID.Value))
                            .ToList();
                    }
                }
                else if (groupID != null)
                {
                    batteryQuery = batteryQuery
                        .Where(x => x.GroupID == groupID)
                        .ToList();
                }

                var allowedBatteryIds = batteryQuery
                    .Select(x => x.BatteryID)
                    .ToHashSet();

                // 🚨 STRICT SCOPE RULE
                if (allowedBatteryIds.Count == 0)
                {
                    return new BatteryEfficiencyModel
                    {
                        PowerModulesCount = 0,
                        TotalRemainingCapacitySum = 0m,
                        InUseRemainingCapacitySum = 0m,
                        EfficiencyScorePercent = 0,
                        InUsePercent = 0m,
                        IdlePercent = 0m
                    };
                }

                // 3️⃣ Apply scope AFTER SP
                var rows = spRows
                    .Where(r => r != null && allowedBatteryIds.Contains(r.Battery_ID))
                    .ToList();

                decimal utilizedPowerSum = 0m;
                decimal totalPowerSum = 0m;

                foreach (var row in rows)
                {
                    // Total capacity (all batteries)
                    if (row.SlowChangingDataA_RemainingCapacity.HasValue)
                    {
                        var totalCap = row.SlowChangingDataA_RemainingCapacity.Value;
                        if (!double.IsNaN(totalCap) && !double.IsInfinity(totalCap))
                        {
                            totalPowerSum += (decimal)totalCap;
                        }
                    }

                    // In-use batteries only (EventCode == 18)
                    if (row.EventCode != 18 || !row.Remaining_Capacity.HasValue)
                        continue;

                    var usedCap = row.Remaining_Capacity.Value;
                    if (!double.IsNaN(usedCap) && !double.IsInfinity(usedCap))
                    {
                        utilizedPowerSum += (decimal)usedCap;
                    }
                }

                var efficiency = totalPowerSum > 0m
                    ? (int)Math.Round((utilizedPowerSum * 100m) / totalPowerSum, MidpointRounding.AwayFromZero)
                    : 0;

                if (efficiency < 0) efficiency = 0;
                if (efficiency > 100) efficiency = 100;

                return new BatteryEfficiencyModel
                {
                    PowerModulesCount = allowedBatteryIds.Count,

                    TotalRemainingCapacitySum = totalPowerSum,
                    InUseRemainingCapacitySum = utilizedPowerSum,

                    EfficiencyScorePercent = efficiency,
                    InUsePercent = efficiency,
                    IdlePercent = 100m - efficiency
                };
            }
        }


        public BatteryActivityHistoryModel GetBatteryActivityHistory(
    long? communityID,
    long? groupID,
    bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 🔹 SP already applies Community / Group / SubGroup filtering
                var spRows = db.Database
                    .SqlQueryRaw<BatteryActivityHistoryRow>(
                        "EXEC " + BatteryActivityHistoryStoredProcedureName +
                        " @CommunityID, @GroupID, @IncludeAllSubGroups",
                        new SqlParameter("@CommunityID", (object)communityID ?? DBNull.Value),
                        new SqlParameter("@GroupID", (object)groupID ?? DBNull.Value),
                        new SqlParameter("@IncludeAllSubGroups", includeAllSubGroups)
                    )
                    .ToList();

                // Build last 7 UTC calendar days (oldest → newest)
                var endDayUtc = DateTime.UtcNow.Date;
                var startDayUtc = endDayUtc.AddDays(-6);

                var byDay = spRows
                    .Where(r => r != null)
                    .GroupBy(r => r.ActivityDate.Date)
                    .ToDictionary(g => g.Key, g => g.First());

                var days = new List<BatteryActivityHistoryDay>(7);

                for (int i = 0; i < 7; i++)
                {
                    var day = startDayUtc.AddDays(i);

                    BatteryActivityHistoryRow row;
                    byDay.TryGetValue(day, out row);

                    days.Add(new BatteryActivityHistoryDay
                    {
                        ActivityDateUtc = day,
                        TotalBatteriesInScope = row?.TotalBatteriesInScope ?? 0,
                        BatteriesOnline = row?.BatteriesOnline ?? 0,
                        BatteriesOffline = row?.BatteriesOffline ?? 0,
                        Label = day.ToString("dd/MM/yyyy")
                    });
                }

                return new BatteryActivityHistoryModel
                {
                    Days = days
                };
            }
        }


        public DeviceActivityHistoryModel GetDeviceActivityHistory(
    long? communityID,
    long? groupID,
    bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 🔹 SP already applies Community / Group / SubGroup filtering
                var spRows = db.Database
                    .SqlQueryRaw<DeviceActivityHistoryRow>(
                        "EXEC " + DeviceActivityHistoryStoredProcedureName +
                        " @CommunityID, @GroupID, @IncludeAllSubGroups",
                        new SqlParameter("@CommunityID", (object)communityID ?? DBNull.Value),
                        new SqlParameter("@GroupID", (object)groupID ?? DBNull.Value),
                        new SqlParameter("@IncludeAllSubGroups", includeAllSubGroups)
                    )
                    .ToList();

                // Build last 7 UTC calendar days (oldest → newest)
                var endDayUtc = DateTime.UtcNow.Date;
                var startDayUtc = endDayUtc.AddDays(-6);

                // Index SP rows by calendar date
                var byDay = spRows
                    .Where(r => r != null)
                    .GroupBy(r => r.ActivityDate.Date)
                    .ToDictionary(g => g.Key, g => g.First());

                var days = new List<DeviceActivityHistoryDay>(7);

                for (int i = 0; i < 7; i++)
                {
                    var day = startDayUtc.AddDays(i);

                    DeviceActivityHistoryRow row;
                    byDay.TryGetValue(day, out row);

                    days.Add(new DeviceActivityHistoryDay
                    {
                        ActivityDateUtc = day,
                        TotalDevicesInScope = row?.TotalDevicesInScope ?? 0,
                        DevicesOnline = row?.DevicesOnline ?? 0,
                        DevicesOffline = row?.DevicesOffline ?? 0,
                        Label = day.ToString("dd/MM/yyyy")
                    });
                }

                return new DeviceActivityHistoryModel
                {
                    Days = days
                };
            }
        }


        private static string NormalizeStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // Normalize unicode dashes to hyphen and collapse whitespace.
            var v = value
                .Replace('\u2013', '-')  // en dash
                .Replace('\u2014', '-')  // em dash
                .Replace('\u2212', '-')  // minus
                .Replace('–', '-')
                .Replace('—', '-')
                .Trim();

            // Collapse multiple spaces.
            while (v.Contains("  "))
            {
                v = v.Replace("  ", " ");
            }

            return v;
        }

        private static bool ContainsToken(string haystack, string token)
        {
            if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(token)) return false;
            return haystack.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public BatteryStatusModel GetBatteryStatus(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                // 1️⃣ SP → latest status per Battery_ID
                var spRows = db.Database
                    .SqlQueryRaw<LatestBatteryStatusRow>("EXEC " + LatestBatteryStatusStoredProcedureName)
                    .ToList();

                // 2️⃣ Battery scope
                var batteryQuery = db.Batteries.AsNoTracking()
                    .Where(b => !b.IsDeactivated)
                    .Select(b => new
                    {
                        BatteryID = b.ID,
                        CommunityID = b.CommunityID,
                        GroupID = b.GroupID
                    })
                    .ToList();

                if (communityID != null)
                    batteryQuery = batteryQuery.Where(x => x.CommunityID == communityID).ToList();

                if (communityID != null && groupID != null && includeAllSubGroups)
                {
                    using (var groupdb = new GroupsDataRepository())
                    {
                        var allGroups = groupdb.GetGroups(communityID, groupID)
                                               .Select(g => g.ID)
                                               .ToHashSet();
                        allGroups.Add(groupID.Value);

                        batteryQuery = batteryQuery
                            .Where(x => x.GroupID != null && allGroups.Contains(x.GroupID.Value))
                            .ToList();
                    }
                }
                else if (groupID != null)
                {
                    batteryQuery = batteryQuery.Where(x => x.GroupID == groupID).ToList();
                }

                var allowedBatteryIds = batteryQuery.Select(x => x.BatteryID).ToHashSet();

                // 🚨 STRICT SCOPE
                if (allowedBatteryIds.Count == 0)
                {
                    return new BatteryStatusModel
                    {
                        PowerModulesCount = 0,
                        EfficiencyScorePercent = 0
                    };
                }

                // 3️⃣ Apply scope AFTER SP
                var rows = spRows
                    .Where(r => allowedBatteryIds.Contains(r.Battery_ID))
                    .ToList();

                var onDeviceCharging = 0;
                var onDeviceDischarging = 0;
                var onDeviceIdle = 0;
                var offDeviceCharging = 0;
                var offDeviceIdle = 0;

                var onDeviceChargingIds = new List<long>();
                var onDeviceDischargingIds = new List<long>();
                var onDeviceIdleIds = new List<long>();
                var offDeviceChargingIds = new List<long>();
                var offDeviceIdleIds = new List<long>();

                foreach (var row in rows)
                {
                    var status = NormalizeStatus(row.BatteryStatus);

                    var isOn = ContainsToken(status, "On Device");
                    var isOff = ContainsToken(status, "Off Device");

                    if (isOn)
                    {
                        if (ContainsToken(status, "Discharging"))
                        {
                            onDeviceDischarging++;
                            onDeviceDischargingIds.Add(row.Battery_ID);
                        }
                        else if (ContainsToken(status, "Charging"))
                        {
                            onDeviceCharging++;
                            onDeviceChargingIds.Add(row.Battery_ID);
                        }
                        else
                        {
                            onDeviceIdle++;
                            onDeviceIdleIds.Add(row.Battery_ID);
                        }
                    }
                    else if (isOff)
                    {
                        if (ContainsToken(status, "Charging"))
                        {
                            offDeviceCharging++;
                            offDeviceChargingIds.Add(row.Battery_ID);
                        }
                        else
                        {
                            offDeviceIdle++;
                            offDeviceIdleIds.Add(row.Battery_ID);
                        }
                    }
                    else
                    {
                        offDeviceIdle++;
                        offDeviceIdleIds.Add(row.Battery_ID);
                    }
                }

                var total = rows.Count;
                var onTotal = onDeviceCharging + onDeviceDischarging + onDeviceIdle;
                var offTotal = offDeviceCharging + offDeviceIdle;

                var efficiency = total > 0
                    ? (int)Math.Round((decimal)onTotal * 100m / total, MidpointRounding.AwayFromZero)
                    : 0;

                return new BatteryStatusModel
                {
                    PowerModulesCount = allowedBatteryIds.Count,

                    OnDeviceChargingCount = onDeviceCharging,
                    OnDeviceDischargingCount = onDeviceDischarging,
                    OnDeviceIdleCount = onDeviceIdle,

                    OffDeviceChargingCount = offDeviceCharging,
                    OffDeviceIdleCount = offDeviceIdle,

                    OnDeviceTotalCount = onTotal,
                    OffDeviceTotalCount = offTotal,

                    EfficiencyScorePercent = efficiency,

                    OnDeviceChargingBatteryIds = onDeviceChargingIds,
                    OnDeviceDischargingBatteryIds = onDeviceDischargingIds,
                    OnDeviceIdleBatteryIds = onDeviceIdleIds,
                    OffDeviceChargingBatteryIds = offDeviceChargingIds,
                    OffDeviceIdleBatteryIds = offDeviceIdleIds
                };
            }
        }




        #endregion

        #region AdminDashboard
        public AdminDashboardModel GetAdminDashboard()
        {
            AdminDashboardModel model = new AdminDashboardModel();
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {

                model.Communities = db.Communities.Count();
                model.Groups = db.Groups.Count();
                using (var userdb = new ApplicationDbContext())
                {
                    model.Users = userdb.Users.Count();
                }
                model.RegDevices = db.Devices.Where(x => x.Community != null).Count();
                model.RemDevices = db.Devices.Where(x => x.Community == null).Count();

                model.RegBatteries = db.Batteries.Where(x => x.Community != null).Count();
                model.RemBatteries = db.Batteries.Where(x => x.Community == null).Count();
                return model;
            }
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}