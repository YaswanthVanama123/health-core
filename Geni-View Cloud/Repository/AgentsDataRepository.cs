using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Agent;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class AgentsDataRepository : IDisposable
    {
        #region Agents
        public List<AgentViewModel> GetAgents()
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                var mainQuery = (from a in db.Agents
                                 select new AgentViewModel()
                                 {
                                     Agent = a,
                                     Status = (a.Timestamp >= GlobalSettings.OnlineRangeInMinutes) ? new ExtraInfo { Name = "Online", Color = GlobalSettings.SuccessColor } :
                                              (a.Timestamp >= GlobalSettings.OfflineRangeInDays && a.Timestamp < GlobalSettings.OnlineRangeInMinutes) ? new ExtraInfo { Name = "Offline", Color = GlobalSettings.WarningColor } :
                                              new ExtraInfo { Name = "Unknown", Color = GlobalSettings.AlertColor },
                                 }).ToList();
                return mainQuery;
            }

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}