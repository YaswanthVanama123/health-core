using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeniView.Cloud.Services
{
    public class GeniViewApplicationUpdateService : IApplicationUpdateService
    {
        public ApplicationUpdate CheckForUpdate(Guid appId, Version currentVersion)
        {
            try
            {
                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    var updateInfo = db.ApplicationUpdates.FirstOrDefault(x => x.AppId == appId);

                    if (updateInfo?.HasUpdate == true && updateInfo.HasValidUpdateInfo == true)
                    {
                        if (new Version(updateInfo.LatestVersion) > currentVersion)
                            return updateInfo;
                    }

                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
