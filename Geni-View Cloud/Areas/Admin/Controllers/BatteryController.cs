using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Application Admin")]
    public class BatteryController : Controller
    {
        private BatteriesDataRepository repository = new BatteriesDataRepository();
        private CommunitiesDataRepository comRepository = new CommunitiesDataRepository();
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Assign()
        {
            try
            {
                var list = repository.GetUnassignedBatteries();
                var model = new AssignRemoveListModel()
                {
                    CommunityID = 0,
                    DeviceList = list.ToList(),
                };
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                return View();
            }

        }

        public ActionResult AssignBatteries(AssignRemoveListModel model)
        {
            try
            {
                repository.AssignBatteries(model);
                // Register User Activity
                var _batteries = (from m in model.DeviceList.Where(x => x.IsChecked == true)
                                  select m.SerialNumber).ToList();
                string message = String.Format("Assign to community:{0}", comRepository.FindByID(model.CommunityID).Name);
                userAHM.AddActivity(message, ActivityObjectType.Battery, _batteries);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
            }
            return RedirectToAction("Assign");
        }

        public ActionResult Remove()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(AssignRemoveListModel model)
        {
            try
            {
                var list = repository.GetAssignedBatteries(model.CommunityID);
                model = new AssignRemoveListModel()
                {
                    CommunityID = model.CommunityID,
                    DeviceList = list.ToList(),
                };
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                return View(model);
            }
        }

        public ActionResult RemoveBatteries(AssignRemoveListModel model)
        {
            try
            {
                repository.RemoveBatteries(model);
                var _batteries = (from m in model.DeviceList.Where(x => x.IsChecked == true)
                                  select m.SerialNumber).ToList();
                userAHM.AddActivity("Unassign from community", ActivityObjectType.Battery, _batteries);
                userAHM.AddActivity("Just test");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                return View();
            }
            return RedirectToAction("Remove");

        }

        public ActionResult ActiveBatteries()
        {
            List<ActivateDeactivatedModel> model = repository.GetActiveBatteries();
            return View(model);
        }

        public ActionResult DeactivatedBatteries()
        {
            List<ActivateDeactivatedModel> model = repository.GetDeactivatedBatteries();
            return View(model);
        }

        public ActionResult Activate(List<ActivateDeactivatedModel> model)
        {
            try
            {
                repository.ActivateDeactivateBatteries(model, false);
                var _batteries = (from m in model.Where(x => x.IsChecked == true)
                                  select m.SerialNumber).ToList();
                userAHM.AddActivity("Activated", ActivityObjectType.Battery, _batteries);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
            }
            return RedirectToAction("ActiveBatteries");
        }

        public ActionResult Deactivate(List<ActivateDeactivatedModel> model)
        {
            try
            {
                repository.ActivateDeactivateBatteries(model, true);
                var _batteries = (from m in model.Where(x => x.IsChecked == true)
                                  select m.SerialNumber).ToList();
                userAHM.AddActivity("Deactivated", ActivityObjectType.Battery, _batteries);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
            }
            return RedirectToAction("DeactivatedBatteries");
        }
    }
}