using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class CommunitiesDataRepository : IDisposable
    {
        #region Communities
        public List<Community> GetCommunities()
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                return db.Communities.AsNoTracking().ToList();
            }
        }

        public Community FindByID(long id)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                return db.Communities.Find(id);
            }
        }

        public void Insert(Community community)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                // Setting Creation date and new GUID
                community.CreateDate = DateTime.UtcNow;
                community.CommunityID = Guid.NewGuid();

                db.Communities.Add(community);
                db.SaveChanges();
            }
        }

        public void Update(Community community)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                var originalCommunity = db.Communities.Find(community.ID);
                
                originalCommunity.Name = community.Name;
                originalCommunity.Description = community.Description;
                originalCommunity.IsActive = community.IsActive;
                originalCommunity.Address = community.Address != null ? community.Address : originalCommunity.Address;

                db.Entry(originalCommunity).State = EntityState.Modified;
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