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
    public class DeviceController : Controller
    {
        private DevicesDataRepository repository = new DevicesDataRepository();
        private CommunitiesDataRepository comRepository = new CommunitiesDataRepository();
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Assign()
        {
            try
            {
                var list = repository.GetUnassignedDevices();
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
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return View();
            }
        }
        public ActionResult AssignDevice(AssignRemoveListModel model)
        {
            try
            {
                repository.AssignDevices(model);
                var _devices = (from m in model.DeviceList.Where(x => x.IsChecked == true)
                                select m.SerialNumber).ToList();
                var message = String.Format("Assign to community:{0}", comRepository.FindByID(model.CommunityID).Name);
                userAHM.AddActivity(message, ActivityObjectType.Device, _devices);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
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
                if (model.CommunityID != null)
                {
                    var list = repository.GetAssignedDevices(model.CommunityID);
                    model = new AssignRemoveListModel()
                    {
                        CommunityID = model.CommunityID,
                        DeviceList = list.ToList(),
                    };
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error(ex);
                return View(model);
            }
        }
        public ActionResult RemoveDevice(AssignRemoveListModel model)
        {
            try
            {
                repository.RemoveDevices(model);
                if (model.DeviceList.Count > 0)
                {
                    var _devices = (from m in model.DeviceList.Where(x => x.IsChecked == true)
                                    select m.SerialNumber).ToList();
                    userAHM.AddActivity("Unassign from community", ActivityObjectType.Device, _devices);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
            }
            return RedirectToAction("Remove");
        }

        public ActionResult ActiveDevices()
        {
            List<ActivateDeactivatedModel> model = repository.GetActivedDevices();
            return View(model);
        }

        public ActionResult DeactivatedDevices()
        {
            List<ActivateDeactivatedModel> model = repository.GetDeactivatedDevices();
            return View(model);
        }

        public ActionResult Activate(List<ActivateDeactivatedModel> model)
        {
            try
            {
                repository.ActivateDeactivateDevices(model, false);
                var _devices = (from m in model.Where(x => x.IsChecked == true)
                                select m.SerialNumber).ToList();
                userAHM.AddActivity("Activated", ActivityObjectType.Device, _devices);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
            }

            return RedirectToAction("ActiveDevices");
        }

        public ActionResult Deactivate(List<ActivateDeactivatedModel> model)
        {
            try
            {
                repository.ActivateDeactivateDevices(model, true);
                if (model.Count > 0)
                {
                    var _devices = (from m in model.Where(x => x.IsChecked == true)
                                    select m.SerialNumber).ToList();
                    userAHM.AddActivity("Deactivated", ActivityObjectType.Device, _devices);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
            }
            return RedirectToAction("DeactivatedDevices");
        }
    }
}