using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware.Event;
using NLog;
using System;
using System.Collections.Generic;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class DeviceEventsController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly DeviceEventsDataRepository _db = new DeviceEventsDataRepository();

        // GET: DeviceEvents
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetDeviceEventHistory()
        {
            try
            {
                ApplicationUser currentUser;
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }
                ViewBag.CurrentUser = currentUser;

                var count = 50;

                List<DeviceEvent> model;

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = _db.GetLatestDeviceEvents(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, count);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = _db.GetLatestDeviceEvents(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, count);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = _db.GetLatestDeviceEvents(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, count);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = _db.GetLatestDeviceEvents(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, count);
                    }
                    else
                    {
                        model = _db.GetLatestDeviceEvents(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, count);
                    }
                }
                else
                {
                    model = new List<DeviceEvent>();
                }

                return PartialView("~/Views/Dashboard/_DashboardDeviceEventHistory.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetDeviceEventHistory failed.");
                return StatusCode((int)500, ex.GetBaseException().Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}