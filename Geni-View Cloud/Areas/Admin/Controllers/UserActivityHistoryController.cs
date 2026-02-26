using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    public class UserActivityHistoryController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index()
        {
            var query = new UserActivityHistoryFilter()
            {
                BeginDate = DateTime.Now.AddDays(-7),
                Count = 50,
                ActivityList = null
            };

            return View(query);
                
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(UserActivityHistoryFilter model)
        {
            UserActivityHistoryFilter query = new UserActivityHistoryFilter()
            {
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
                using (var db = new IdentityDataRepository())
                {
                    query.ActivityList = db.GetActivities(query, currentUser);
                }
                 
                return View(query);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.",ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View(query);
            }
        }
    }
}