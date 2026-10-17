using BloodBankManagement.Models;
using BloodBankManagement.Services;
using BloodBankManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BloodBankManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDonorService _donorService;
        private readonly IRecipientService _recipientService;
        private readonly IHospitalService _hospitalService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDonorService donorService,
            IRecipientService recipientService,
            IHospitalService hospitalService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _donorService = donorService;
            _recipientService = recipientService;
            _hospitalService = hospitalService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Logged in successfully!";
                        return RedirectToLocal(returnUrl);
                    }
                    if (result.IsLockedOut)
                    {
                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "An exception occurred while attempting to log in user {Email}", model?.Email);
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Register(string? returnUrl = null, string? role = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            ViewData["ReturnUrl"] = returnUrl;
            var model = new RegisterViewModel();
            if (!string.IsNullOrEmpty(role))
            {
                if (role.Equals("Donor", StringComparison.OrdinalIgnoreCase)) model.Role = "Donor";
                else if (role.Equals("Recipient", StringComparison.OrdinalIgnoreCase)) model.Role = "Recipient";
                else if (role.Equals("Hospital", StringComparison.OrdinalIgnoreCase)) model.Role = "Hospital";
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email, 
                    FirstName = model.FirstName, 
                    LastName = model.LastName ?? string.Empty 
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    var selectedRole = model.Role;
                    if (selectedRole != Roles.Donor && selectedRole != Roles.Recipient && selectedRole != Roles.Hospital)
                    {
                        selectedRole = Roles.Donor;
                    }

                    await _userManager.AddToRoleAsync(user, selectedRole);

                    // Create corresponding domain entity record
                    if (selectedRole == Roles.Donor)
                    {
                        var donor = new Donor
                        {
                            UserId = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            IsActive = true
                        };
                        await _donorService.AddDonorAsync(donor);
                    }
                    else if (selectedRole == Roles.Recipient)
                    {
                        var recipient = new Recipient
                        {
                            UserId = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            IsActive = true
                        };
                        await _recipientService.AddRecipientAsync(recipient);
                    }
                    else if (selectedRole == Roles.Hospital)
                    {
                        var hospitalName = string.IsNullOrWhiteSpace(user.LastName)
                            ? user.FirstName
                            : $"{user.FirstName} {user.LastName}";

                        var hospital = new Hospital
                        {
                            UserId = user.Id,
                            Name = hospitalName,
                            Email = user.Email,
                            IsActive = true
                        };
                        await _hospitalService.AddHospitalAsync(hospital);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = $"Registration successful! Welcome to LifeSaver as a {selectedRole}.";
                    return RedirectToLocal(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have logged out successfully.";
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && 
                Url.IsLocalUrl(returnUrl) &&
                !returnUrl.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase) &&
                !returnUrl.Contains("/Account/Register", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}

