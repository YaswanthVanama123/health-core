using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class GroupsDataRepository : IDisposable
    {
        #region Groups
        public List<Group> GetGroups(long? communityID = null, long? groupID = null)
        {
            using (var db = new GeniViewCloudDataRepository())
            {

                var model = db.Groups.Include(z => z.Community)
                                     .Where(x => communityID == null ? true : x.Community.ID == communityID)
                                     .ToList();

                if (groupID != null)
                {
                    model = RecursiveSearch(model, groupID.Value)
                            .Union(model.Where(y => y.ID == groupID)).ToList();
                }
                return model;
            }

        }
        
        private List<Group> RecursiveSearch(List<Group> allGroups, long groupID)
        {
            var hierarchy = new List<Group>();
            var query = allGroups.Where(x => x.ParentGroup != null && x.ParentGroup.ID == groupID).ToList();

            if (query != null)
            {
                foreach (var item in query)
                {
                    hierarchy.Add(item);
                    foreach (var recItem in RecursiveSearch(allGroups, item.ID))
                    {
                        hierarchy.Add(recItem);
                    }
                }
            }
            return hierarchy;
        }

        /// <summary>
        /// Get Data for Group Heirarchy
        /// </summary>
        /// <param name="communityID"></param>
        /// <param name="groupID"></param>
        /// <returns>Ienumerable<GroupHeirarchyModel></returns>
        public List<GroupHeirarchyModel> GetGroupHeirarchy(long communityID, long? groupID = null)
        {
            var mainQuery = GetGroups(communityID, groupID);
            var model = from m in mainQuery
                        select new GroupHeirarchyModel
                        {
                            GroupID = m.ID.ToString(),
                            GroupName = m.Name,
                            ParentGroupName = m.ParentGroup == null ? "Community" : m.ParentGroup.Name,
                            ParentGroupID = m.ParentGroup == null ? "Community" : m.ParentGroup.ID.ToString(),
                        };

            return model.ToList();
        }
    
        /// <summary>
        /// Find Group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="communityID"></param>
        /// <returns></returns>
        public GroupViewModel FindGroupByID(long id, long? communityID = null, long? groupID = null)
        {
            var query = GetGroups(communityID, groupID).Where(x => x.ID == id).FirstOrDefault();
            long? nulableLong = null;
            var model = new GroupViewModel()
            {
                Group = query,
                CommunityID = query.Community.ID,
                ParentGroupID = query.ParentGroup == null ? nulableLong : query.ParentGroup.ID
            };

            return model;
        }
     
        /// <summary>
        /// Inser Group (Add New Group)
        /// </summary>
        /// <param name="model"></param>
        public void InsertGroup(GroupViewModel model)
        {

            using (var db = new GeniViewCloudDataRepository())
            {

                model.Group.CreateDate = DateTime.UtcNow;
                model.Group.GroupID = Guid.NewGuid();
                model.Group.Community = db.Communities.Find(model.CommunityID);
                model.Group.ParentGroup = model.ParentGroupID == null ? null : db.Groups.Find(model.ParentGroupID);

                db.Groups.Add(model.Group);
                db.SaveChanges();
            }
        }

        public void DeleteGroup(long id)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                Group group = db.Groups.Find(id);

                db.Batteries.Where(x => x.Group.ID == group.ID).ToList().ForEach(u => u.Group = null);
                db.Devices.Where(x => x.Group.ID == group.ID).ToList().ForEach(u => u.Group = null);
                db.Groups.Remove(group);
                db.SaveChanges();
            }

        }

        public void UpdateGroup(GroupViewModel model)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                Group originalGroup = db.Groups.Find(model.Group.ID);

                var allChildrenGroups = GetGroups(model.CommunityID, model.Group.ID);

                Group originalChildGroup = allChildrenGroups.FirstOrDefault(x => x.ID == model.ParentGroupID);

                if (originalChildGroup != null)
                {
                    originalChildGroup = db.Groups.Find(originalChildGroup.ID);
                    originalChildGroup.ParentGroup = originalGroup.ParentGroup;
                    db.Entry(originalChildGroup).State = EntityState.Modified;
                };

                originalGroup.Name = model.Group.Name;
                originalGroup.Description = model.Group.Description;
                originalGroup.Community = db.Communities.Find(model.CommunityID);
                originalGroup.ParentGroup = db.Groups.Find(model.ParentGroupID);

                db.Entry(originalGroup).State = EntityState.Modified;
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