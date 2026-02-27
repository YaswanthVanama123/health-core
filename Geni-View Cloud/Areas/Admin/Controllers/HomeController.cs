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
    public class HomeController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index()
        {
            try
            {
                using (var db = new DashboardDataRepository())
                {
                    var model = db.GetAdminDashboard();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.",ex);
            }

            return View();

        }
        public ActionResult About()
        {
            return View();
        }

    }
}