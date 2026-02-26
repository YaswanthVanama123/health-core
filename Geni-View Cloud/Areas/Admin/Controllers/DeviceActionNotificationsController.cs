using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Authorize(Roles = "Application Admin")]
    public class DeviceActionNotificationsController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index()
        {
            List<DeviceActionNotificationViewModel> model;
            try
            {
                using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                {
                    model = (from dan in db.DeviceEventActionNotifications
                            select new DeviceActionNotificationViewModel
                            {
                                IsChecked = false,
                                DeviceEventNotification = dan
                            }).ToList();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            
        }

        public JsonResult EnableNotifications(List<DeviceActionNotificationViewModel> model)
        {
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                try
                {
                    foreach (var item in model.Where(x => x.IsChecked))
                    {
                        var origin = db.DeviceEventActionNotifications.Find(item.DeviceEventNotification.ID);
                        origin.SendNotifications = true;
                        db.Entry(origin).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return Json("Success");
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                    return Json(ex.Message);
                }
                
                
            }
        }

        public JsonResult DisableNotifications(List<DeviceActionNotificationViewModel> model)
        {
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                try
                {
                    foreach (var item in model.Where(x => x.IsChecked))
                    {
                        var origin = db.DeviceEventActionNotifications.Find(item.DeviceEventNotification.ID);
                        origin.SendNotifications = false;
                        db.Entry(origin).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return Json("Success");
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                    return Json(ex.Message);
                }


            }
        }
    }
}