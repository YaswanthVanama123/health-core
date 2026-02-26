using GeniView.Cloud.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Threading.Tasks;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
        }

        // ── Login ────────────────────────────────────────────────────────────

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.Info("User {Email} logged in.", model.Email);
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.Warn("User {Email} account locked out.", model.Email);
                ModelState.AddModelError("", "Account locked out. Please try again later.");
                return View(model);
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // ── Log off ──────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.Info("User signed out.");
            return RedirectToAction("Login", "Account");
        }

        // ── Email confirmation ───────────────────────────────────────────────

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                ViewBag.ErrorText = "Invalid confirmation link.";
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorText = "User not found.";
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // ── Forgot password ──────────────────────────────────────────────────

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Don't reveal whether the user exists — always redirect to confirmation.
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = Url.Action(
                    "ResetPassword", "Account",
                    new { userId = user.Id, code },
                    protocol: Request.Scheme);

                var mail = new MailHelper();
                if (mail.IsMailServerConfigured())
                {
                    await mail.SendMailAsync(
                        user.FullName ?? user.Email!,
                        user.Email!,
                        MessageEnumeration.ResetPassword,
                        callbackUrl!);
                }
                else
                {
                    _logger.Warn("Mail server not configured — password reset link for {Email}: {Url}",
                        user.Email, callbackUrl);
                }
            }

            return RedirectToAction("ForgotPasswordConfirmation", "Account");
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ── Reset password ───────────────────────────────────────────────────

        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            if (code == null || userId == null)
            {
                ViewBag.ErrorText = "Invalid password reset link.";
                return View("Error");
            }
            return View(new ResetPasswordViewModel { Code = code });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist.
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation", "Account");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private ActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
