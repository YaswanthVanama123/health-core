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

namespace GeniView.Cloud.Controllers
{
    [Authorize(Roles = "Community Admin,Community Group Admin")]
    public class UsersController : Controller
    {
        private IdentityDataRepository repository = new IdentityDataRepository();
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ActionResult Index()
        {
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }

                if (User.Identity.IsAuthenticated)
                {
                    var model = repository.GetUsers(currentUser.CommunityID);

                    if (User.IsInRole("Community Group Admin"))
                        model = repository.GetUsers(currentUser.CommunityID, currentUser.GroupID);

                    return View(model);
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
            return View();
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
            var activateUserByEmail = model.ActivateUserByEmail;

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

                    user.FullName = model.User.FullName;
                    user.Email = model.User.Email;
                    user.UserName = model.User.Email;
                    user.TimeZoneId = model.User.TimeZoneId;
                    user.PasswordHash = PasswordHash.HashPassword(user, model.Password);
                    user.IsNotificationEnable = model.User.IsNotificationEnable;
                    user.LockoutEnabled = true;

                    if (model.RoleName == "Community Admin")
                    {
                        if (model.User.CommunityID != null)
                        {
                            user.GroupID = null;
                            user.CommunityID = model.User.CommunityID;
                        }
                        else
                        {
                            ModelState.AddModelError("DbFail", "Please Select Community");
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
                            ModelState.AddModelError("GroupIDError", "Please Choose Parent Group");
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
                            ModelState.AddModelError("DbFail", "Please Select Community");
                            return View(model);
                        }
                    }
                    else
                    {
                        user.CommunityID = model.User.CommunityID;
                        user.GroupID = model.User.GroupID;
                    }

                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.RoleName);
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
                                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                                ModelState.AddModelError("DbFail", "Can not send email : " + ex.Message);
                                await _userManager.DeleteAsync(user);
                                return View(model);
                            }
                        }
                        userAHM.AddActivity("Create new user", ActivityObjectType.User, model.User.Email);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("EmailError", error.Description);
                        }
                        return View(model);
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                    ModelState.AddModelError("DbFail", ex.Message);
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
            var user = new ApplicationUser();
            var model = new UserViewModel();

            try
            {
                var currentUser = repository.FindUserByID(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                if (User.IsInRole("Community Admin"))
                {
                    user = repository.FindUserByID(id, currentUser.CommunityID.Value);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    user = repository.FindUserByID(id, currentUser.CommunityID.Value, currentUser.GroupID.Value);
                }

                if (user == null)
                    return NotFound();

                model = new UserViewModel
                {
                    User = user,
                    RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                    isUserLocked = user.LockoutEnd == null ? false : user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
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

                    if (model.RoleName == "Community Admin")
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
                        userAHM.AddActivity("Edit user", ActivityObjectType.User, user.Email);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("EmailError", error.Description);
                        }
                        return View(model);
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
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
            var user = new ApplicationUser();
            var model = new UserViewModel();

            try
            {
                var currentUser = repository.FindUserByID(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                if (User.IsInRole("Community Admin"))
                {
                    user = repository.FindUserByID(id, currentUser.CommunityID.Value);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    user = repository.FindUserByID(id, currentUser.CommunityID.Value, currentUser.GroupID.Value);
                }

                if (user == null)
                {
                    return NotFound();
                }

                model = new UserViewModel
                {
                    User = user,
                    RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var _userName = User.Identity.Name;
                var user = await _userManager.FindByIdAsync(id);

                if (_userName != user.UserName)
                {
                    await _userManager.DeleteAsync(user);
                    userAHM.AddActivity("Delete user", ActivityObjectType.User, user.Email);
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("DbFail", "User cannot Delete himself!!!");
                    return View(new UserViewModel { User = user });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
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
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                TempData["Alert"] = ex.Message;
                return View("Edit", model);
            }
            ViewBag.TimeZones = TimeZoneHelper.GetTimeZoneList();
            TempData["Success"] = string.Format("Confirmation Mail send to {0} successfully", model.User.Email);
            return View("Edit", model);
        }

        public JsonResult IsMailServerConfigured()
        {
            MailHelper mailHelper = new MailHelper();
            var retval = mailHelper.IsMailServerConfigured();
            return Json(retval);
        }

    }
}
