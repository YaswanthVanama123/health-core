using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Hubs;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeniView.Cloud.Controllers
{

    [Authorize]
    public class DashboardController : Controller
    {
        #region constructor
        public DashboardDataRepository db = new DashboardDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private ApplicationUser GetCurrentUser()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            using var repo = new IdentityDataRepository();
            return repo.FindUserByID(id);
        }
        #endregion

        public ActionResult Index()
        {
            var displayName = User.Identity.Name;

            try
            {
                using (var identityRepo = new Repository.IdentityDataRepository())
                {
                    var currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.FullName))
                    {
                        displayName = currentUser.FullName;
                    }
                }
            }
            catch
            {
                // Keep fallback (User.Identity.Name) if repo fails.
            }

            ViewBag.DashboardGreetingName = displayName;

            return View();
        }

public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            try
            {
                var currentUser = new ApplicationUser();
                using (var userDb = new IdentityDataRepository())
                {
                    currentUser = userDb.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                Community community = new Community();

                using (var comDb = new CommunitiesDataRepository())
                {
                    if (currentUser.CommunityID != null)
                        community = comDb.FindByID(currentUser.CommunityID.Value);
                }

                return View(community);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        // Add in DashboardController class (anywhere near other JsonResult endpoints)
        public JsonResult GetCycleStatus()
        {
            var model = new CycleStatusModel();
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetCycleStatus(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetCycleStatus(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetCycleStatus(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetCycleStatus(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetCycleStatus(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetCycleStatus failed.");
                return Json(model);
            }
        }

        public JsonResult GetStateOfCharge()
        {
            var model = new StateOfChargeModel();
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetStateOfCharge(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetStateOfCharge(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetStateOfCharge(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetStateOfCharge(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetStateOfCharge(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetStateOfCharge failed.");
                return Json(model);
            }
        }

        public JsonResult GetEffectiveRotation()
        {
            var model = new EffectiveRotationModel();
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetEffectiveRotation(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetEffectiveRotation(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetEffectiveRotation(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetEffectiveRotation(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetEffectiveRotation(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetEffectiveRotation failed.");
                return Json(model);
            }
        }

        public JsonResult GetTemperature()
        {
            var model = new TemperatureModel();
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetTemperature(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetTemperature(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetTemperature(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetTemperature(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetTemperature(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetTemperature failed.");
                return Json(model);
            }
        }

        public ActionResult RedirectFilteredPage(string selectedItem, string category)
        {
            string redirectLink;

            if (category == "battery")
            {
                switch (selectedItem)
                {
                    case "Idle, Needs Charging":
                        selectedItem = "IdleNeedsCharging";
                        break;
                    case "Idle, Ready to Use":
                        selectedItem = "IdleReadytoUse";
                        break;
                }
                redirectLink = Url.Action("Index", "Batteries", new { id = selectedItem });
            }
            else
            {
                switch (selectedItem)
                {
                    case "Online, On Battery":
                        selectedItem = "OnBattery";
                        break;
                    case "Online, Plugged in":
                        selectedItem = "PluggedIn";
                        break;
                }
                redirectLink = Url.Action("Index", "Devices", new { id = selectedItem });
            }
            return Json(new { Url = redirectLink });
        }

        public JsonResult GetBingMapKey()
        {
            string result = GlobalSettings.BingMapKey;
            return Json(result);
        }

        public JsonResult GetBatteryEfficiency()
        {
            var model = new BatteryEfficiencyModel();
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetBatteryEfficiency(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetBatteryEfficiency(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetBatteryEfficiency(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetBatteryEfficiency(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetBatteryEfficiency(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            //catch (Exception ex)
            //{
            //    _logger.Error(ex, "GetBatteryEfficiency failed.");
            //    return Json(model);
            //}



            catch (Exception ex)
            {
                _logger.Error(ex, "GetBatteryEfficiency failed.");

                return Json(new
                {
                    Success = false,
                    ErrorMessage = ex.GetBaseException().Message
                });
            }


        }

        public JsonResult GetBatteryActivityHistory()
        {
            var model = new BatteryActivityHistoryModel { Days = new List<BatteryActivityHistoryDay>() };
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetBatteryActivityHistory(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetBatteryActivityHistory(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetBatteryActivityHistory(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetBatteryActivityHistory(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetBatteryActivityHistory(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetBatteryActivityHistory failed.");
                return Json(model);
            }
        }

        public JsonResult GetDeviceActivityHistory()
        {
            var model = new DeviceActivityHistoryModel { Days = new List<DeviceActivityHistoryDay>() };
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetDeviceActivityHistory(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetDeviceActivityHistory(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetDeviceActivityHistory(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetDeviceActivityHistory(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetDeviceActivityHistory(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetDeviceActivityHistory failed.");
                return Json(model);
            }
        }

        public JsonResult GetBatteryStatus()
        {
            var model = new BatteryStatusModel();
            var currentUser = GetCurrentUser();

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetBatteryStatus(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetBatteryStatus(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetBatteryStatus(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetBatteryStatus(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                    else
                    {
                        model = db.GetBatteryStatus(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true);
                    }
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetBatteryStatus failed.");
                return Json(model);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}