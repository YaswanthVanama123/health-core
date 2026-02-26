using GeniView.Cloud.Models;
using GeniView.Data.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class IdentityDataRepository : IDisposable
    {
        private readonly ApplicationDbContext _identityDb;
        private readonly GeniViewCloudDataRepository _dataDb;

        public IdentityDataRepository(ApplicationDbContext identityDb, GeniViewCloudDataRepository dataDb)
        {
            _identityDb = identityDb;
            _dataDb = dataDb;
        }

        // Parameterless constructor for use until DI is fully wired in Phase 3.
        // TODO Phase 3: remove and inject via IServiceProvider in all callers.
        public IdentityDataRepository()
            : this(new ApplicationDbContext(), new GeniViewCloudDataRepository())
        {
        }

        #region Users

        public List<UserViewModel> GetUsers(long? communityID = null, long? groupID = null)
        {
            List<UserViewModel> model = new List<UserViewModel>();

            var query = _identityDb.Users
                                   .Include(x => x.Roles)
                                   .Where(r => communityID == null || r.CommunityID == communityID)
                                   .ToList();

            var groups = new GroupsDataRepository().GetGroups(communityID, groupID);
            var communities = new CommunitiesDataRepository().GetCommunities();

            if (groupID != null)
            {
                model = (from u in query
                         join g in groups on u.GroupID equals g.ID into tmp_g
                         from grp in tmp_g
                         join c in communities on u.CommunityID equals c.ID into tmp_c
                         from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                         join role in _identityDb.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                         select new UserViewModel
                         {
                             GroupName = grp.Name,
                             CommunityName = community.Name,
                             RoleName = role.Name,
                             isUserLocked = u.LockoutEnd == null ? false : u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                             User = u
                         }).ToList();
            }
            else
            {
                model = (from u in query
                         join g in groups on u.GroupID equals g.ID into tmp_g
                         from grp in tmp_g.DefaultIfEmpty(new Group { Name = "" })
                         join c in communities on u.CommunityID equals c.ID into tmp_c
                         from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                         join role in _identityDb.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                         select new UserViewModel
                         {
                             GroupName = grp.Name,
                             CommunityName = community.Name,
                             RoleName = role.Name,
                             isUserLocked = u.LockoutEnd == null ? false : u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                             User = u
                         }).ToList();
            }

            return model;
        }

        public List<UserViewModel> GetUsersWhoHasAccess(long? communityID = null, long? groupID = null)
        {
            List<UserViewModel> model = new List<UserViewModel>();

            var query = (from u in _identityDb.Users.Include(x => x.Roles)
                         join r in _identityDb.Roles on u.Roles.FirstOrDefault()!.RoleId equals r.Id
                         where u.CommunityID == communityID || r.Name!.Contains("Application")
                         select u).ToList();

            var groups = new GroupsDataRepository().GetGroups(communityID, groupID);
            var communities = new CommunitiesDataRepository().GetCommunities();

            if (groupID != null)
            {
                model = (from u in query
                         join g in groups on u.GroupID equals g.ID into tmp_g
                         from grp in tmp_g
                         join c in communities on u.CommunityID equals c.ID into tmp_c
                         from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                         join role in _identityDb.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                         select new UserViewModel
                         {
                             GroupName = grp.Name,
                             CommunityName = community.Name,
                             RoleName = role.Name,
                             isUserLocked = u.LockoutEnd == null ? false : u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                             User = u
                         }).ToList();
            }
            else
            {
                model = (from u in query
                         join g in groups on u.GroupID equals g.ID into tmp_g
                         from grp in tmp_g.DefaultIfEmpty(new Group { Name = "" })
                         join c in communities on u.CommunityID equals c.ID into tmp_c
                         from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                         join role in _identityDb.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                         select new UserViewModel
                         {
                             GroupName = grp.Name,
                             CommunityName = community.Name,
                             RoleName = role.Name,
                             isUserLocked = u.LockoutEnd == null ? false : u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                             User = u
                         }).ToList();
            }

            return model;
        }

        // NOTE: GetCurrentUser() is intentionally removed from this repository.
        // In ASP.NET Core, the current user is resolved from IHttpContextAccessor
        // or directly from the controller's User property.
        // Use: await _userManager.GetUserAsync(User) in controllers instead.

        public List<ApplicationUser> GetUsersByGroupID(long groupID)
        {
            return _identityDb.Users
                              .AsNoTracking()
                              .Where(x => x.GroupID == groupID)
                              .ToList();
        }

        public ApplicationUser? FindUserByID(string id, long? communityID = null, long? groupID = null)
        {
            return _identityDb.Users
                              .AsNoTracking()
                              .Where(x => (communityID == null || x.CommunityID == communityID) &&
                                          (groupID == null || x.GroupID == groupID) &&
                                          x.Id == id)
                              .FirstOrDefault();
        }

        public List<IdentityRole> GetRoles()
        {
            return _identityDb.Roles
                              .AsNoTracking()
                              .ToList();
        }

        public void AddActivity(UserActivityHistory model)
        {
            _dataDb.UserActivityHistory.Add(model);
            _dataDb.SaveChanges();
        }

        public List<UserActivityHistory> GetActivities(UserActivityHistoryFilter filter, ApplicationUser currentUser)
        {
            var convertedBeginDate = TimeZoneHelper.ConvertToUTC(filter.BeginDate, currentUser);
            var convertedEndDate = TimeZoneHelper.ConvertToUTC(filter.EndDate, currentUser);

            return _dataDb.UserActivityHistory
                          .AsNoTracking()
                          .Where(x => x.Timestamp >= convertedBeginDate && x.Timestamp <= convertedEndDate)
                          .OrderByDescending(x => x.Timestamp)
                          .Take(filter.Count)
                          .ToList();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
