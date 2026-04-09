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
using GeniView.Data;
using NLog;
using GeniView.Cloud.Common.Queue;
using GeniView.Cloud.Common;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class BatteriesController : Controller
    {
        private GeniViewCloudDataRepository _db = new GeniViewCloudDataRepository();
        private BatteriesDataRepository batterydb = new BatteriesDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private UserActivityHistory userAHM = new UserActivityHistory();

        public ActionResult Index(string id)
        {
            IEnumerable<BatteriesListViewModel> model = Enumerable.Empty<BatteriesListViewModel>();

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
                    model = batterydb.GetBatteries(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);

                    //model = batterydb.BuildOptimizedQuery(SessionHelper.CommunityID, _db);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = batterydb.GetBatteries(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = batterydb.GetBatteries(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = batterydb.GetBatteries(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                    }
                    else
                        model = batterydb.GetBatteries(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups ?? true, id);
                }
                model = model.OrderByDescending(x => x.LastSeenOn);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
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
                var model = batterydb.FindBatteryByID(id.Value, null, null);
                if (model == null)
                {
                    return NotFound();
                }

                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }


                ViewBag.BatterySerialNumber = model.SerialNumber;

                var endDate = DateTime.Now;
                var beginDate = endDate.AddHours(-2);

                var query = new BatteryHistoryLogFilter()
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
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        [Authorize(Roles = "Application Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult History(BatteryHistoryLogFilter model)
        {
            try
            {
                ViewBag.BatterySerialNumber = batterydb.FindBatteryByID(model.ID, null, null).SerialNumber;
                ApplicationUser currentUser = new ApplicationUser();

                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                var query = new BatteryHistoryLogFilter()
                {
                    ID = model.ID,
                    BeginDate = model.BeginDate,
                    EndDate = model.EndDate,
                    Count = model.Count < 0 ? 100 : model.Count,
                    LogList = batterydb.GetBatteryHistoryLog(model.ID, model.BeginDate, model.EndDate, currentUser, model.isPeriodicDataTriggerIncluded, model.Count)
                };

                return View(query);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        [Authorize(Roles = "Community Admin, Community Group Admin")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }

            BatteriesListViewModel model = new BatteriesListViewModel();
            Battery _battery = new Battery();
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
                    _battery = batterydb.FindBatteryByID(id.Value, currentUser.CommunityID);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    _battery = batterydb.FindBatteryByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                }
                else { _battery = null; }

                if (_battery == null)
                {
                    return NotFound();
                }

                model = new BatteriesListViewModel()
                {
                    ID = _battery.ID,
                    CommunityID = _battery.Community == null ? nullableLong : _battery.Community.ID,
                    GroupID = _battery.Group == null ? nullableLong : _battery.Group.ID,
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BatteriesListViewModel model)
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
                            batterydb.UpdateBattery(model, model.GroupID);
                        else
                            batterydb.UpdateBattery(model, currentUser.GroupID);
                    }
                    else
                        batterydb.UpdateBattery(model, model.GroupID);

                    batterydb.UpdateBattery(model, model.GroupID);
                    userAHM.AddActivity("Assign to group", ActivityObjectType.Battery, batterydb.FindBatteryByID(model.ID).SerialNumber);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    ModelState.AddModelError("DbFail", ex.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult Graphs(long? id)
        {
            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }
            Battery model = new Battery();
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
                    model = batterydb.FindBatteryByID(id.Value, SessionHelper.CommunityID, SessionHelper.GroupID);
                    ViewBag.BatteryItems = new SelectList(batterydb.GetBatteries(null, null, true).Where(x => x.ID != id), "ID", "Battery.SerialNumber");
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = batterydb.FindBatteryByID(id.Value, currentUser.CommunityID, null);
                    ViewBag.BatteryItems = new SelectList(batterydb.GetBatteries(currentUser.CommunityID, null, true).Where(x => x.ID != id), "ID", "Battery.SerialNumber");
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = batterydb.FindBatteryByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                    ViewBag.BatteryItems = new SelectList(batterydb.GetBatteries(currentUser.CommunityID, currentUser.GroupID, true).Where(x => x.ID != id), "ID", "Battery.SerialNumber");
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = batterydb.FindBatteryByID(id.Value, currentUser.CommunityID, currentUser.GroupID);
                        ViewBag.BatteryItems = new SelectList(batterydb.GetBatteries(currentUser.CommunityID, currentUser.GroupID, true).Where(x => x.ID != id), "ID", "Battery.SerialNumber");
                    }
                    else
                        model = batterydb.FindBatteryByID(id.Value, currentUser.CommunityID, null);
                    ViewBag.BatteryItems = new SelectList(batterydb.GetBatteries(currentUser.CommunityID, null, true).Where(x => x.ID != id), "ID", "Battery.SerialNumber");
                }

                if (model == null)
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }

            return View(model);
        }

        public JsonResult GetBatteryChartData(long ID, int logType, DateTime beginDate, DateTime endDate)
        {

            IEnumerable<BatteryModel> model = Enumerable.Empty<BatteryModel>();
            try
            {
                if (logType == 1)
                    model = batterydb.GetBatteryChartModel(ID, beginDate, endDate);
                else
                    model = batterydb.GetBatteryChartModelByLog(ID, beginDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "fail while getting battery chart data.");
            }
            return Json(model);

        }

        public JsonResult GetBatteryMasterChartData(long[] IDArray, int? logType, DateTime beginDate, DateTime endDate)
        {

            List<IEnumerable<BatteryModel>> model = new List<IEnumerable<BatteryModel>>();
            IEnumerable<BatteryModel> battery = Enumerable.Empty<BatteryModel>();

            foreach (var id in IDArray)
            {
                if (id == 0)
                    continue;
                try
                {
                    if (logType == 1)
                        battery = batterydb.GetBatteryChartModel(id, beginDate, endDate);
                    else
                        battery = batterydb.GetBatteryChartModelByLog(id, beginDate, endDate);
                    if (battery != null)
                        model.Add(battery);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                }
            }
            return Json(model);
        }

        public ActionResult GetBatteryDetailData(string serialNumber)
        {
            BatteryDetailViewModel model = new BatteryDetailViewModel();
            try
            {
                model = batterydb.GetBatteryDetails(serialNumber);

                if (model == null)
                {
                    return PartialView("_DetailsPartialView", new BatteryDetailViewModel());
                }

                var log = model.LastAgentBatteryLog;
                ViewBag.StateCount = EnumHelper.GetFriendlyText(model.State).Split('\n').Count() - 1;
                if (log?.OperatingData?.BatteryOperatingStatus != null)
                {
                    ViewBag.StateCount += (log.OperatingData.BatteryOperatingStatus.StatusAText ?? "").Split(',').Count() - 1 +
                                          (log.OperatingData.BatteryOperatingStatus.StatusBText ?? "").Split(',').Count() - 1;
                }

                return PartialView("_DetailsPartialView", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                return PartialView("_DetailsPartialView", model ?? new BatteryDetailViewModel());
            }
        }

        public JsonResult TestEnQueue()
        {
            List<MQTTData> datas = new List<MQTTData>();

            _logger.Debug($"TestEnQueue Start {Global._queueHelp._queue.Count}");

            for (int i = 0; i < 500; i++)
            {
                var msg = new MQTTData("Tset",$"{i.ToString()} {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")}");
                Global._queueHelp.Enqueue(msg);
                datas.Add(msg);
                Thread.Sleep(10);

            }

            _logger.Debug($"TestEnQueue End {Global._queueHelp._queue.Count}");

            return Json(datas);
        }

        public JsonResult TestDequeueAPI()
        {
            _logger.Debug($"TestDequeueAPI start QTY:{Global._queueHelp._queue.Count}");

            var ret = Global._queueHelp.DequeueWhile(CancellationToken.None);

            _logger.Debug($"TestDequeueAPI end QTY:{Global._queueHelp._queue.Count}");


            return Json(ret);
        }

        public string TestDequeue()
        {
            _logger.Debug($"TestDequeue start {Global._queueHelp._queue.Count}");

            var ret = Global._queueHelp.DequeueWhile(CancellationToken.None);

            var data = JsonConvert.SerializeObject(ret);

            _logger.Debug($"TestDequeue end {Global._queueHelp._queue.Count}");

            return data;
        }

        /// <summary>
        /// Testing hangfire function
        /// </summary>
        /// <returns></returns>
        public string TestCheckDateTime()
        {
            string result = "";

            result = DateTime.Now.ToString();


            return result;
        }

        protected override void Dispose(bool disposing)
        {
            batterydb.Dispose();
            base.Dispose(disposing);
        }
    }
}
