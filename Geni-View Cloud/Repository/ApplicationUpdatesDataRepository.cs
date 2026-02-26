using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class ApplicationUpdatesDataRepository : IDisposable
    {
        public List<ApplicationUpdate> GetUpdates()
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                return db.ApplicationUpdates.ToList();
            }
        }

        public void Update(ApplicationUpdate model)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                ApplicationUpdate originalAppUpdate = db.ApplicationUpdates.Find(model.ID);

                originalAppUpdate.HasUpdate = model.HasUpdate;
                originalAppUpdate.LatestVersion = model.LatestVersion;
                originalAppUpdate.ReleaseDate = model.ReleaseDate;
                originalAppUpdate.Description = model.Description;
                originalAppUpdate.ReleaseNotes = model.ReleaseNotes;
                originalAppUpdate.DownloadAddress = model.DownloadAddress;

                db.Entry(originalAppUpdate).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public ApplicationUpdate FindById(long appId)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                return db.ApplicationUpdates.Find(appId);
            }
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}