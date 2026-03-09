using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using GeniView.Cloud.Models;
using System.Globalization;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using NLog;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class DevicesController : Controller
    {
        private DevicesDataRepository db = new DevicesDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private UserActivityHistory userAHM = new UserActivityHistory();

        public ActionResult Index(string id)
        {
            List<DeviceListViewModel> model = new List<DeviceListViewModel>();
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetDevices(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetDevices(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetDevices(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                        model = db.GetDevices(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                    else
                        model = db.GetDevices(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }

        }

        [Authorize(Roles = "Application Admin")]
        public ActionResult History(long? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var model = db.FindDeviceByID(id.Value);

                if (model == null)
                    return NotFound();

                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                ViewBag.DeviceSerialNumber = model.SerialNumber;

                var endDate = DateTime.Now;
                var beginDate = endDate.AddHours(-2);

                var query = new DeviceHistoryLogFilter()
                {
                    ID = id.Value,
                    BeginDate = beginDate,
                    EndDate = endDate,
                    Count = 50,
                    isPeriodicDataTriggerIncluded = true,
                    LogList = null,
                };

                return View(query);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        [Authorize(Roles = "Application Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult History(DeviceHistoryLogFilter model)
        {
            try
            {
                ViewBag.DeviceSerialNumber = db.FindDeviceByID(model.ID, null, null).SerialNumber;
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                var query = new DeviceHistoryLogFilter()
                {
                    ID = model.ID,
                    BeginDate = model.BeginDate,
                    EndDate = model.EndDate,
                    Count = model.Count < 0 ? 100 : model.Count,
                    LogList = db.GetDeviceHistoryLog(model.ID, model.BeginDate, model.EndDate, currentUser, model.isPeriodicDataTriggerIncluded, model.Count)
                };

                return View(query);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                var query = new DeviceHistoryLogFilter()
                {
                    ID = model.ID,
                    BeginDate = model.BeginDate,
                    EndDate = model.EndDate,
                    Count = model.Count < 0 ? 100 : model.Count,
                    LogList = null,
                };
                return View(query);
            }
        }

        [Authorize(Roles = "Community Admin, Community Group Admin")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);

            DeviceListViewModel model = new DeviceListViewModel();
            Device _device = new Device();
            long? nullableLong = null;

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
                    _device = db.FindDeviceByID(id.Value, currentUser.CommunityID);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    _device = db.FindDeviceByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                }
                else { _device = null; }

                if (_device == null)
                {
                    return NotFound();
                }

                model = new DeviceListViewModel()
                {
                    ID = _device.ID,
                    CommunityID = _device.Community == null ? nullableLong : _device.Community.ID,
                    GroupID = _device.Group == null ? nullableLong : _device.Group.ID
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DeviceListViewModel model)
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
                        if (model.GroupID != null)
                            db.UpdateDevice(model, model.GroupID);
                        else
                            db.UpdateDevice(model, currentUser.GroupID);
                    }
                    else
                        db.UpdateDevice(model, model.GroupID);
                    userAHM.AddActivity("Assign to group", ActivityObjectType.Device, db.FindDeviceByID(model.ID).SerialNumber);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                    ModelState.AddModelError("DbFail", ex.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult Graphs(long? id)
        {
            if (id == null)
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);

            Device model = new Device();
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.FindDeviceByID(id.Value, SessionHelper.CommunityID);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.FindDeviceByID(id.Value, currentUser.CommunityID);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.FindDeviceByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.FindDeviceByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                    }
                    else
                        model = db.FindDeviceByID(id.Value, currentUser.CommunityID);
                }
                if (model == null)
                {
                    return NotFound();
                }
                var ListOfBay = new List<SelectListItem>();
                ListOfBay.Add(new SelectListItem { Text = "All", Value = "-1" });
                int count = db.GetDeviceBayCount(model.ID);

                for (int i = 0; i < count; i++)
                {
                    ListOfBay.Add(new SelectListItem { Text = "Bay " + (i + 1).ToString(), Value = (i + 1).ToString() });
                }

                ViewBag.ListOfBay = ListOfBay;
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
            return View(model);
        }

        public ActionResult Events(long? id)
        {
            Device model = new Device();

            var endDate = DateTime.Now;
            var beginDate = endDate.AddHours(-2);

            var query = new DeviceEventLogFilter()
            {
                BeginDate = beginDate,
                EndDate = endDate,
                Count = 50,
                EventList = null
            };

            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                if (id != null)
                {
                    if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                    {
                        model = db.FindDeviceByID(id.Value, SessionHelper.CommunityID);
                    }
                    else if (User.IsInRole("Community Admin"))
                    {
                        model = db.FindDeviceByID(id.Value, currentUser.CommunityID);
                    }
                    else if (User.IsInRole("Community Group Admin"))
                    {
                        model = db.FindDeviceByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                    }
                    else if (User.IsInRole("Community User"))
                    {
                        if (currentUser.GroupID != null)
                        {
                            model = db.FindDeviceByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                        }
                        else
                            model = db.FindDeviceByID(id.Value, currentUser.CommunityID);
                    }

                    if (model == null)
                    {
                        return NotFound();
                    }

                    ViewBag.DeviceSerialNumber = model.SerialNumber;
                }
                return View(query);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View(query);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Events(long? id, DeviceEventLogFilter model)
        {
            DeviceEventLogFilter query = new DeviceEventLogFilter()
            {
                DeviceID = id != null ? id.Value : 0,
                BeginDate = model.BeginDate,
                EndDate = model.EndDate,
                Count = model.Count < 0 ? 100 : model.Count,
            };

            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                if (id != null)
                    query.EventList = db.GetDeviceEventHistoryList(query, currentUser);
                else
                {
                    if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                    {
                        query.EventList = db.GetDeviceEventHistoryList(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, query, currentUser);
                    }
                    else if (User.IsInRole("Community Admin"))
                    {
                        query.EventList = db.GetDeviceEventHistoryList(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, query, currentUser);
                    }
                    else if (User.IsInRole("Community Group Admin"))
                    {
                        query.EventList = db.GetDeviceEventHistoryList(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, query, currentUser);
                    }
                    else if (User.IsInRole("Community User"))
                    {
                        if (currentUser.GroupID != null)
                            query.EventList = db.GetDeviceEventHistoryList(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, query, currentUser);
                        else
                            query.EventList = db.GetDeviceEventHistoryList(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, query, currentUser);
                    }

                    var deviceModel = db.FindDeviceByID(id.Value);

                    ViewBag.DeviceSerialNumber = deviceModel.SerialNumber;
                }

                return View(query);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View(query);
            }
        }
        public JsonResult GetDeviceMasterChartData(long ID, int LogType, string SerialNumber, int? bayNo, DateTime beginDate, DateTime endDate)
        {
            List<IEnumerable<BatteryModel>> model = new List<IEnumerable<BatteryModel>>();
            IEnumerable<BatteryModel> device = Enumerable.Empty<BatteryModel>();
            int pointCount = 500;
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }

                // BayNo = -1 get all Devices
                if (bayNo < 0)
                {
                    var setting = db.FindDeviceByID(ID).DeviceSettingsCollection.OrderByDescending(x => x.Timestamp).FirstOrDefault();
                    int count = setting == null ? 1:setting.Bays;

                    for (int i = 0; i < count; i++)
                    {
                        if (LogType == 1) // Device Log
                        {
                            device = db.GetDeviceMasterChartModel(SerialNumber, i + 1, beginDate, endDate, currentUser, pointCount);
                        }
                        else
                        {
                            device = db.GetDeviceMasterChartModelByLog(SerialNumber, i + 1, beginDate, endDate, currentUser);
                        }
                        model.Add(device);
                    }
                }
                else
                {
                    if (LogType == 1) // Device Log
                    {
                        device = db.GetDeviceMasterChartModel(SerialNumber, bayNo, beginDate, endDate, currentUser, pointCount);
                    }
                    else
                    {
                        device = db.GetDeviceMasterChartModelByLog(SerialNumber, bayNo, beginDate, endDate, currentUser);
                    }
                    model.Add(device);
                }
                var jsonResult = Json(model);
                // MaxJsonLength is not available in ASP.NET Core; large responses are unrestricted by default.
                return jsonResult;
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return Json(model);
            }
        }

        public JsonResult GetDeviceChartData(long ID, int logType, DateTime beginDate, DateTime endDate)
        {
            int pointCount = 500;
            IEnumerable<DeviceModel> model = Enumerable.Empty<DeviceModel>();
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }

                if (logType == 1)
                    model = db.GetDeviceChartModel(ID, beginDate, endDate, currentUser, pointCount);
                else
                    model = db.GetDeviceChartModelByLog(ID, beginDate, endDate, currentUser);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
            }
            return Json(model);
        }

        public ActionResult GetDeviceDetailsData(string ID)
        {
            DeviceDetailsViewModel model = new DeviceDetailsViewModel();
            try
            {
                model = db.GetDeviceDetails(ID);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
            }

            return PartialView("_DeviceDetailsPartialView", model);
        }

        public JsonResult GetDeviceBayData(string serialNumber)
        {
            DeviceBayDataModel model = new DeviceBayDataModel();
            try
            {
                model = db.GetDeviceBayDataG3(serialNumber);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
            }
            return Json(model);

        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
