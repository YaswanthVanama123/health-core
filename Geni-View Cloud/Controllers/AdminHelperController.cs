using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Web;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Controllers
{
    [Authorize(Roles = "Application Admin,Community Admin,Community Group Admin")]
    public class AdminHelperController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        #region DropDown Logic
        public JsonResult LoadCommunitiesList()
        {
            List<Community> model = new List<Community>();
            var emptyList = Enumerable.Empty<SelectList>();
            try
            {
                var currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }

                if (User.Identity.IsAuthenticated)
                {
                    using (var db = new CommunitiesDataRepository())
                    {
                        model = db.GetCommunities();
                    }

                    if (User.IsInRole("Application Admin"))
                    {
                        return Json(new SelectList(model, "ID", "Name"));
                    }
                    else if (User.IsInRole("Community Admin") || User.IsInRole("Community Group Admin"))
                    {
                        if (currentUser == null)
                            return Json(emptyList);
                        model = model.Where(x => x.ID == currentUser.CommunityID).ToList();
                        return Json(new SelectList(model, "ID", "Name"));
                    }
                    return Json(emptyList);
                }
                else
                    return Json(emptyList);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                return Json(emptyList);
            }
        }

        public JsonResult LoadGroupsList(long communityID)
        {
            List<Group> model = new List<Group>();
            var emptyList = Enumerable.Empty<SelectList>();

            try
            {
                var currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }

                if (User.Identity.IsAuthenticated)
                {

                    using (var db = new GroupsDataRepository())
                    {
                        if (User.IsInRole("Application Admin") || User.IsInRole("Community Admin"))
                        {
                            model = db.GetGroups(communityID);
                            return Json(new SelectList(model, "ID", "Name"));
                        }
                        if (User.IsInRole("Community Group Admin"))
                        {
                            model = db.GetGroups(communityID, currentUser.GroupID);
                            return Json(new SelectList(model, "ID", "Name"));
                        }
                    }
                    return Json(emptyList);
                }
                else
                    return Json(emptyList);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                return Json(emptyList);
            }
        }

        public JsonResult LoadRolesList()
        {
            var emptyList = Enumerable.Empty<SelectList>();

            try
            {
                var currentUser = new ApplicationUser();
                using (var db = new IdentityDataRepository())
                {
                    currentUser = db.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

                    if (User.Identity.IsAuthenticated)
                    {
                        if (User.IsInRole("Application Admin"))
                        {
                            return Json(new SelectList(db.GetRoles(), "Name", "Name"));
                        }
                        else if (User.IsInRole("Community Admin"))
                        {
                            return Json(new SelectList(db.GetRoles().Where(x => x.Name.Contains("Community")), "Name", "Name"));
                        }
                        else if (User.IsInRole("Community Group Admin"))
                        {
                            return Json(new SelectList(db.GetRoles().Where(x => x.Name.Contains("Community") && !x.Name.Contains("Community Admin")), "Name", "Name"));
                        }
                        return Json(emptyList);
                    }
                    else
                        return Json(emptyList);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                return Json(emptyList);
            }
        }
        #endregion

        #region Session
        public JsonResult SaveFilterState(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            SessionHelper.CommunityID = communityID;
            SessionHelper.GroupID = groupID;
            SessionHelper.IncludeAllSubGroups = includeAllSubGroups;

            return Json(new { success = true });
        }

        public JsonResult GetFilterState()
        {
            return Json(
                new
                {
                    communityID = SessionHelper.CommunityID,
                    groupID = SessionHelper.GroupID,
                    includeAllSubGroups = SessionHelper.IncludeAllSubGroups != null ? SessionHelper.IncludeAllSubGroups : false
                });
        }
        #endregion
    }
}