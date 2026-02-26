using Microsoft.EntityFrameworkCore;
using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public sealed class DashboardPopupRepository : IDisposable
    {
        private const string PopupDashboardStoredProcedureName = "dbo.sp_popupDashboard";

        public (List<DashboardPopupRowModel> Items, int Total) GetPopupDashboardRows(
            HashSet<long> batteryIds,
            string search,
            int pageNumber,
            int pageSize)
        {
            if (batteryIds == null || batteryIds.Count == 0)
            {
                return (new List<DashboardPopupRowModel>(), 0);
            }

            using (var db = new GeniViewCloudDataRepository())
            {
                db.Database.SetCommandTimeout(500);

                var sql = @"
CREATE TABLE #PopupDashboard
(
    Battery_ID BIGINT NOT NULL,
    [Power Modules] NVARCHAR(255) NULL,
    [Attached to] NVARCHAR(255) NULL,
    [Device Type] NVARCHAR(255) NULL,
    SoC INT NULL,
    [Cycle Count] INT NULL,
    Temp INT NULL,
    Status NVARCHAR(255) NULL,
    [Last Attached] NVARCHAR(255) NULL,
    LastCharged NVARCHAR(50) NULL,
    LastDischarged NVARCHAR(50) NULL
);

INSERT INTO #PopupDashboard
EXEC " + PopupDashboardStoredProcedureName + @";

SELECT
    Battery_ID,
    [Power Modules] AS PowerModules,
    [Attached to] AS AttachedTo,
    [Device Type] AS DeviceType,
    SoC,
    [Cycle Count] AS CycleCount,
    Temp AS Temperature,
    Status,
    [Last Attached] AS LastAttached,
    TRY_CONVERT(datetime, LastCharged, 103) AS LastCharged,
    TRY_CONVERT(datetime, LastDischarged, 103) AS LastDischarged
FROM #PopupDashboard;

DROP TABLE #PopupDashboard;
";

                var allScopeRows = db.Database
                    .SqlQueryRaw<DashboardPopupRowModel>(sql)
                    .ToList();

                IEnumerable<DashboardPopupRowModel> filtered = allScopeRows
                    .Where(r => batteryIds.Contains(r.Battery_ID));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    filtered = filtered.Where(r =>
                        (!string.IsNullOrEmpty(r.PowerModules) && r.PowerModules.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(r.AttachedTo) && r.AttachedTo.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(r.DeviceType) && r.DeviceType.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(r.Status) && r.Status.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                filtered = filtered.OrderBy(r => r.PowerModules);

                var total = filtered.Count();

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var items = filtered
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (items, total);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}