using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private UserActivityHistory userAHM = new UserActivityHistory();

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                var model = await _userManager.GetUserAsync(User);
                if (model == null) return RedirectToAction("Login", "Account");

                ViewBag.RoleName = (await _userManager.GetRolesAsync(model)).FirstOrDefault();

                using (CommunitiesDataRepository comdb = new CommunitiesDataRepository())
                {
                    ViewBag.Community = model.CommunityID != null
                        ? comdb.FindByID(model.CommunityID.Value)?.Name ?? ""
                        : "";
                }
                using (GroupsDataRepository grpdb = new GroupsDataRepository())
                {
                    var groups = grpdb.GetGroups(model.CommunityID, model.GroupID);
                    ViewBag.Groups      = groups;
                    ViewBag.GroupsCount = groups.Count();
                }
                ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("ProfileController.Index error.", ex);
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
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null) return RedirectToAction("Login", "Account");

                    user.FullName              = model.FullName;
                    user.Email                 = model.Email;
                    user.UserName              = model.Email;
                    user.TimeZoneId            = model.TimeZoneId;
                    user.IsNotificationEnable  = model.IsNotificationEnable;

                    if (image != null)
                    {
                        user.ImageMimeType = image.ContentType;
                        using var ms = new System.IO.MemoryStream();
                        await image.CopyToAsync(ms);
                        user.ProfilePhoto = ms.ToArray();
                    }

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        userAHM.AddActivity("Profile information updated.");
                        TempData["Success"] = "Profile information updated.";
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error("ProfileController.Index POST error.", ex);
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
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Success"] = "Profile password changed.";
                    userAHM.AddActivity("Profile password changed.");
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", result.Errors.FirstOrDefault()?.Description ?? "Password change failed.");
            }
            catch (Exception ex)
            {
                _logger.Error("ProfileController.ChangePassword error.", ex);
                TempData["Fail"] = "Profile password not changed.";
                ModelState.AddModelError("DbFail", ex.Message);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<string> GetUserInfo()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                return user?.FullName ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error("ProfileController.GetUserInfo error.", ex);
                return string.Empty;
            }
        }

        public async Task<FileResult> GetProfileImage()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.ProfilePhoto != null)
                    return File(user.ProfilePhoto, user.ImageMimeType!);
            }
            catch (Exception ex)
            {
                _logger.Warn("Profile photo not loaded.", ex);
            }
            return File("~/Resources/default_profile_photo.png", "image/png");
        }
    }
}
