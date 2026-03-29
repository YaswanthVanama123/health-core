using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using System.Net.Mail;
using System.Threading.Tasks;
using NLog;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Application Admin")]
    public class MailSettingsController : Controller
    {
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index()
        {
            MailServer model = new MailServer();
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                if(db.MailServer.Count() > 0)
                {
                    ViewBag.ViewMode = "Update";
                    model = db.MailServer.FirstOrDefault();
                }
                else
                {
                    ViewBag.ViewMode = "Create";
                }
                return View(model);
            }
                
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(MailServer mailServer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                    {
                        if (db.MailServer.Count() > 0)
                        {
                            db.Entry(mailServer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        else
                        {
                            db.MailServer.Add(mailServer);
                        }
                        db.SaveChanges();
                    }
                    TempData["Success"] = "Mail Server Updated.";
                    userAHM.AddActivity("Update mail server");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    TempData["Alert"] = ex.Message;
                }
                
            }
            ViewBag.ViewMode = "Update";
            return View(mailServer);
        }

        public async Task<ActionResult> Verify()
        {
            ApplicationUser currentUser;
            MailHelper mailHelper = new MailHelper();
            using (var identityRepo = new IdentityDataRepository())
            {
              currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            }

            if (mailHelper.IsMailServerConfigured())
            {
                try
                {
                    await mailHelper.SendMailAsync(currentUser.FullName, currentUser.Email, MessageEnumeration.VerifyMailServer, "");
                    TempData["Success"] = string.Format("Verification Mail send to {0} successfully", currentUser.Email);
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.",ex);
                    ModelState.AddModelError("", ex.Message);
                    TempData["Alert"] = ex.Message;
                }
            }
            else
            {
                TempData["Alert"] = "Please, Enter Email Server Settings First";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
