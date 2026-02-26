using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using GeniView.Cloud.Models;
using System.Collections.ObjectModel;
using NLog;
using GeniView.Cloud.Repository;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private dynamic /* TODO Phase 4: ApplicationSignInManager */ _signInManager;
        private dynamic /* TODO Phase 4: ApplicationUserManager */ _userManager;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private UserActivityHistory userAHM = new UserActivityHistory();

        #region constructor
        public ProfileController()
        {
        }

        public ProfileController(dynamic /* TODO Phase 4: ApplicationUserManager */ userManager, dynamic /* TODO Phase 4: ApplicationSignInManager */ signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public dynamic /* TODO Phase 4: ApplicationSignInManager */ SignInManager
        {
            get { throw new NotImplementedException("TODO Phase 4: inject SignInManager via ASP.NET Core Identity"); }
            private set
            {
                _signInManager = value;
            }
        }

        public dynamic /* TODO Phase 4: ApplicationUserManager */ UserManager
        {
            get { throw new NotImplementedException("TODO Phase 4: inject UserManager via ASP.NET Core Identity"); }
            private set
            {
                _userManager = value;
            }
        }
        #endregion

        public ActionResult Index()
        {
            try
            {
                var model = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
                ViewBag.RoleName = UserManager.GetRoles(model.Id).FirstOrDefault();
                using (CommunitiesDataRepository comdb = new CommunitiesDataRepository())
                {
                    ViewBag.Community = model.CommunityID != null ? comdb.FindByID(model.CommunityID.Value).Name : "";
                }
                using (GroupsDataRepository grpdb = new GroupsDataRepository())
                {
                    var groups = grpdb.GetGroups(model.CommunityID, model.GroupID);
                    ViewBag.Groups = groups;
                    ViewBag.GroupsCount = groups.Count();
                }
                ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ApplicationUser model, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await UserManager.FindByIdAsync(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
                    user.FullName = model.FullName;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.TimeZoneId = model.TimeZoneId;
                    user.IsNotificationEnable = model.IsNotificationEnable;

                    if (image != null)
                    {
                        user.ImageMimeType = image.ContentType;
                        using var ms = new System.IO.MemoryStream();
                        await image.CopyToAsync(ms);
                        user.ProfilePhoto = ms.ToArray();
                    }
                    var result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        if (user != null)
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        }
                        userAHM.AddActivity("Profile information updated.");
                        TempData["Success"] = "Profile information updated.";
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                    TempData["Fail"] = "Profile information not updated.";
                    ModelState.AddModelError("DbFail", ex.Message);
                    return View();
                }
            }
            return RedirectToAction("Index");
        }


        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await UserManager.ChangePasswordAsync(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    TempData["Success"] = "Profile password changed.";
                    userAHM.AddActivity("Profile password changed.");
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", result.Errors.FirstOrDefault());
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                TempData["Fail"] = "Profile password not changed.";
                ModelState.AddModelError("DbFail", ex.Message);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // [ChildActionOnly] removed — not available in ASP.NET Core. Use ViewComponent or partial view instead.
        public string GetUserInfo()
        {
            try
            {
                var user = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
                return user.FullName;
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return "";
            }
        }

        public FileResult GetProfileImage()
        {
            try
            {
                var user = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
                if (user.ProfilePhoto != null)
                    return File(user.ProfilePhoto, user.ImageMimeType);
                else
                    return File("~/Resources/default_profile_photo.png", "image/png");
            }
            catch (Exception ex)
            {
                _logger.Warn("Profile photo not loaded.", ex);
                return File("~/Resources/default_profile_photo.png", "image/png");
            }
        }
    }
}
