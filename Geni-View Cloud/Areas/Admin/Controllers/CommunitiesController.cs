using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Net;
using System;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using GeniView.Cloud.Models;
using NLog;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Authorize(Roles = "Application Admin")]
    public class CommunitiesController : Controller
    {
        private CommunitiesDataRepository communityRepo = new CommunitiesDataRepository();
        private IdentityDataRepository identityRepo = new IdentityDataRepository();
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            try {
                ViewBag.CurrentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                return View(communityRepo.GetCommunities());
            }
            catch(Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ID,Name,Description,isActive")] Community community)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    community.Address = new Address();
                    communityRepo.Insert(community);
                    userAHM.AddActivity("Create new community", ActivityObjectType.Community, community.Name);
                    return RedirectToAction("Index");
                }
                catch(Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                    ModelState.AddModelError("DbFail", ex.Message);
                }
            }

            return View(community);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

            Community model = new Community();

            try
            {
                model = communityRepo.FindByID(id.Value);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Community community)
        {
            if (ModelState.IsValid)
            {
                try {
                    communityRepo.Update(community);
                    userAHM.AddActivity("Edit community", ActivityObjectType.Community, community.Name);
                }
                catch(Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.",ex);
                    ModelState.AddModelError("DbFail", ex.Message);
                    return View(community);
                }
                return RedirectToAction("Index");
            }
            return View(community);
        }

        protected override void Dispose(bool disposing)
        {
            communityRepo.Dispose();
            base.Dispose(disposing);
        }
    }
}
