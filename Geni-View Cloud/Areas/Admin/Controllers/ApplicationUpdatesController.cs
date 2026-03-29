using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Application Admin")]
    public class ApplicationUpdatesController : Controller
    {
        private ApplicationUpdatesDataRepository repository = new ApplicationUpdatesDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            return View(repository.GetUpdates());
        }

        public ActionResult Edit(long? id)
        {

            ApplicationUpdate model = new ApplicationUpdate();

            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                model = repository.FindById(id.Value);
                model.ReleaseDate = model.ReleaseDate != null ? model.ReleaseDate : DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("CustomFail", ex.Message);
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
        public ActionResult Edit(ApplicationUpdate model)
        {
            if (ModelState.IsValid && model.HasValidUpdateInfo)
            {
                try
                {
                    repository.Update(model);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    ModelState.AddModelError("CustomFail", ex.Message);
                    return View(model);
                }
            }
            return View(model);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (repository != null)
                {
                    repository.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}