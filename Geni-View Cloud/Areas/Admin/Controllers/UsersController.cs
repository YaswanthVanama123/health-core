using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Application Admin")]
    public class UsersController : Controller
    {
        #region constructor
        IdentityDataRepository repository = new IdentityDataRepository();
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        #endregion

        public ActionResult Index(int? page)
        {
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            try
            {
                // Get All Users
                var model = repository.GetUsers();
                return View(model);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        public ActionResult CreateNewAccount()
        {
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateNewAccount(UserViewModel model)
        {
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            bool activateUserByEmail = model.ActivateUserByEmail;

            if (model.Password == null)
            {
                model.Password = Guid.NewGuid().ToString();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser();
                    var PasswordHash = new PasswordHasher<ApplicationUser>();

                    // Note : Full Name Can be Empty no Validations
                    user.FullName = model.User.FullName;
                    user.Email = model.User.Email;
                    user.UserName = model.User.Email;
                    user.TimeZoneId = model.User.TimeZoneId;
                    user.PasswordHash = PasswordHash.HashPassword(user, model.Password);
                    user.IsNotificationEnable = model.User.IsNotificationEnable;
                    // Enable for all users lockout function when create
                    user.LockoutEnabled = true;

                    if (model.RoleName.Contains("Application"))
                    {
                        user.GroupID = null;
                        user.CommunityID = null;
                    }
                    else if (model.RoleName == "Community Admin")
                    {
                        if (model.User.CommunityID != null)
                        {
                            user.GroupID = null;
                            user.CommunityID = model.User.CommunityID;
                        }
                        else
                        {
                            ModelState.AddModelError("DbFail", "Please, Select Community");
                            return View(model);
                        }
                    }
                    else if (model.RoleName == "Community Group Admin")
                    {
                        if (model.User.GroupID != null && model.User.CommunityID != null)
                        {
                            user.GroupID = model.User.GroupID;
                            user.CommunityID = model.User.CommunityID;
                        }
                        else
                        {
                            ModelState.AddModelError("GroupIDError", "Please, Select Group");
                            return View(model);
                        }
                    }
                    else if (model.RoleName == "Community User")
                    {
                        if (model.User.CommunityID != null)
                        {
                            user.GroupID = model.User.GroupID;
                            user.CommunityID = model.User.CommunityID;
                        }
                        else
                        {
                            ModelState.AddModelError("DbFail", "Please, Select Community");
                            return View(model);
                        }
                    }

                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.RoleName);
                        // E-mail confirm message
                        if (!activateUserByEmail)
                        {
                            user.EmailConfirmed = true;
                            await _userManager.UpdateAsync(user);
                        }
                        else
                        {
                            try
                            {
                                // Generate link to confirm e-mail
                                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { area = "", userId = user.Id, code = code }, protocol: Request.Scheme);
                                MailHelper mailhelper = new MailHelper();
                                await mailhelper.SendMailAsync(user.FullName, user.Email, MessageEnumeration.ConfirmEmail, callbackUrl);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("DbFail", "Can not send email : " + ex.Message);
                                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                                await _userManager.DeleteAsync(user);
                                return View(model);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("EmailError", string.Join("\n", result.Errors.Select(e => e.Description)));
                        return View(model);
                    }
                    userAHM.AddActivity("Create new User", ActivityObjectType.User, model.User.Email);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    ModelState.AddModelError("DbFail", ex.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }
            var model = new UserViewModel();
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                model = new UserViewModel
                {
                    User = user,
                    RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                    isUserLocked = user.LockoutEnd == null ? false : user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserViewModel model)
        {
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(model.User.Id);

                    user.FullName = model.User.FullName;
                    user.Email = model.User.Email;
                    user.UserName = model.User.Email;
                    user.TimeZoneId = model.User.TimeZoneId;
                    user.IsNotificationEnable = model.User.IsNotificationEnable;

                    if (model.isUserLocked)
                    {
                        user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(GlobalSettings.UserLockoutTimeInMinutes);
                    }
                    else
                    {
                        user.LockoutEnd = null;
                    }

                    if (model.RoleName.Contains("Application"))
                    {
                        user.GroupID = null;
                        user.CommunityID = null;
                    }
                    else if (model.RoleName == "Community Admin")
                    {
                        if (model.User.CommunityID != null)
                        {
                            user.CommunityID = model.User.CommunityID;
                            user.GroupID = null;
                        }
                        else
                        {
                            ModelState.AddModelError("CommunityError", "Please, Assign a Community");
                            return View(model);
                        }
                    }
                    else if (model.RoleName == "Community Group Admin")
                    {
                        if (model.User.GroupID != null)
                        {
                            user.GroupID = model.User.GroupID;
                            user.CommunityID = model.User.CommunityID;
                        }
                        else
                        {
                            ModelState.AddModelError("GroupIDError", "Please, Assign a Group");
                            return View(model);
                        }
                    }
                    else
                    {
                        user.CommunityID = model.User.CommunityID;
                        user.GroupID = model.User.GroupID;
                    }
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.RoleName);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("EmailError", error.Description);
                        }
                        return View(model);
                    }
                    userAHM.AddActivity("Edit User", ActivityObjectType.User, model.User.Email);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    ModelState.AddModelError("DbFail", ex.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }
            var model = new UserViewModel();

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                model = new UserViewModel { User = user };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }

            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var model = new UserViewModel();
            try
            {
                var _userName = User.Identity.Name;
                var user = await _userManager.FindByIdAsync(id);

                if (_userName != user.UserName)
                {
                    await _userManager.DeleteAsync(user);
                    userAHM.AddActivity("Delete User", ActivityObjectType.User, user.Email);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("DbFail", "User cannot delete himself.");
                    model = new UserViewModel { User = user };
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> ReSendMailConfirmation(string id)
        {
            var model = new UserViewModel();
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                // Generate link to confirm e-mail
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { area = "", userId = user.Id, code = code }, protocol: Request.Scheme);
                MailHelper mailhelper = new MailHelper();
                await mailhelper.SendMailAsync(user.FullName, user.Email, MessageEnumeration.ConfirmEmail, callbackUrl);

                model = new UserViewModel
                {
                    User = user,
                    RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                    isUserLocked = user.LockoutEnd == null ? false : user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                TempData["Alert"] = ex.Message;
                return View("Edit", model);
            }
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            TempData["Success"] = string.Format("Confirmation Mail send to {0} successfully", model.User.Email);
            return View("Edit", model);
        }

    }
}
