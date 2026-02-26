using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class ApplicationLogsDataRepository : IDisposable
    {
        public List<ApplicationLog> GetApplicationLogs(ApplicationLogsFilter filter, ApplicationUser currentUser)
        {
            List<ApplicationLog> model = new List<ApplicationLog>();
            using (var db = new GeniViewCloudDataRepository())
            {

                string searchLevel = filter.LogLevel == ApplicationLogLevel.ALL ? "" : filter.LogLevel.ToString();
                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(filter.BeginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(filter.EndDate, currentUser);

                model = db.ApplicationLogs.Where(x => x.Logged >= convertedBeginDate && 
                                                      x.Logged <= convertedEndDate &&
                                                      x.Level.Contains(searchLevel)
                                                ).OrderByDescending(x => x.Logged)
                                                 .Take(filter.Count)
                                                 .ToList();

                return model;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}