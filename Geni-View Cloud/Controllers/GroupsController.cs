using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using GeniView.Cloud.Models;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using System.Collections.Generic;
using GeniView.Cloud.Repository;
using NLog;

namespace GeniView.Cloud.Controllers
{
    [Authorize(Roles = "Community Admin,Community Group Admin")]
    public class GroupsController : Controller
    {
        private GroupsDataRepository groupsRepo = new GroupsDataRepository();
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            try
            {
                List<Group> model = new List<Group>();

                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                if (User.Identity.IsAuthenticated)
                {
                    if (User.IsInRole("Community Admin"))
                    {
                        model = groupsRepo.GetGroups(currentUser.CommunityID);
                    }
                    else if (User.IsInRole("Community Group Admin"))
                    {
                        model = groupsRepo.GetGroups(currentUser.CommunityID, currentUser.GroupID);
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        public ActionResult GroupsHierarchy()
        {
            return View();
        }

        public JsonResult GetChartData()
        {
            List<GroupHeirarchyModel> model = new List<GroupHeirarchyModel>();
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                if (User.IsInRole("Community Admin"))
                {
                    model = groupsRepo.GetGroupHeirarchy(currentUser.CommunityID.Value);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = groupsRepo.GetGroupHeirarchy(currentUser.CommunityID.Value, currentUser.GroupID);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
            }
            return Json(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ParentGroupID,CommunityID,Group")] GroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ApplicationUser currentUser = new ApplicationUser();
                    using (var identityRepo = new IdentityDataRepository())
                    {
                        currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                        ViewBag.CurrentUser = currentUser;
                    }

                    if (User.IsInRole("Community Group Admin"))
                    {
                        model.ParentGroupID = model.ParentGroupID == null ? currentUser.GroupID : model.ParentGroupID;
                    }
                    groupsRepo.InsertGroup(model);
                    userAHM.AddActivity("Create new group", ActivityObjectType.Group, model.Group.Name);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    ModelState.AddModelError("DbFail", ex.Message);
                }
            }
            return View(model);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);

            GroupViewModel model = new GroupViewModel();

            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }

                if (User.IsInRole("Community Admin"))
                {
                    model = groupsRepo.FindGroupByID(id.Value, currentUser.CommunityID);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = groupsRepo.FindGroupByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                }

                if (model == null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GroupViewModel model)
        {
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }

                if (ModelState.IsValid)
                {
                    if (model.Group.ID == model.ParentGroupID)
                    {
                        ModelState.AddModelError("DbFail", "Group cannot be own parent Group");
                        return View(model);
                    }

                    if (User.IsInRole("Community Group Admin"))
                    {
                        if (model.ParentGroupID == null)
                            model.ParentGroupID = currentUser.GroupID;
                    }

                    groupsRepo.UpdateGroup(model);
                    userAHM.AddActivity("Edit group", ActivityObjectType.Group, groupsRepo.FindGroupByID(model.Group.ID).Group.Name);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }
            return View(model);
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }
            Group model = new Group();
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }

                if (User.IsInRole("Community Admin"))
                {
                    model = groupsRepo.FindGroupByID(id.Value, currentUser.CommunityID).Group;
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = groupsRepo.FindGroupByID(id.Value, currentUser.CommunityID, currentUser.GroupID).Group;
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = groupsRepo.FindGroupByID(id.Value, currentUser.CommunityID.Value, currentUser.GroupID.Value).Group;
                    }
                    else
                        model = groupsRepo.FindGroupByID(id.Value, currentUser.CommunityID.Value).Group;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Group group = groupsRepo.FindGroupByID(id).Group;
            using (var userdb = new IdentityDataRepository())
            {

                if (groupsRepo.GetGroups(group.Community.ID, id).Count() == 1)
                {
                    if (userdb.GetUsersByGroupID(id).Count() == 0)
                    {
                        try
                        {
                            groupsRepo.DeleteGroup(id);
                            userAHM.AddActivity("Delete group", ActivityObjectType.Group, group.Name);
                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Geni-View Cloud encountered an error.");
                            ModelState.AddModelError("DbFail", ex.Message);
                        }
                    }
                    else
                    {
                        string log = "Can't delete group, group has assigned ssers,  please delete ssers first or assign to a new group.";
                        _logger.Warn(log);
                        ModelState.AddModelError("DbFail", log);
                    }
                }
                else
                {
                    string log = "Can't delete group, group has children groups.";
                    _logger.Warn(log);
                    ModelState.AddModelError("DbFail", log);
                }
            }

            return View(group);
        }

        protected override void Dispose(bool disposing)
        {
            groupsRepo.Dispose();
            base.Dispose(disposing);
        }
    }
}