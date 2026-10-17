using BloodBankManagement.Models;
using BloodBankManagement.Services;
using BloodBankManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BloodBankManagement.Controllers
{
    [Authorize]
    public class RecipientController : Controller
    {
        private readonly IRecipientService _recipientService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecipientController(IRecipientService recipientService, UserManager<ApplicationUser> userManager)
        {
            _recipientService = recipientService;
            _userManager = userManager;
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Index()
        {
            var recipients = await _recipientService.GetAllRecipientsAsync();
            var viewModels = recipients.Select(r => new RecipientViewModel
            {
                Id = r.Id,
                FirstName = r.FirstName,
                LastName = r.LastName,
                BloodGroup = r.BloodGroup,
                ContactNumber = r.ContactNumber,
                Email = r.Email,
                IsActive = r.IsActive
            }).ToList();

            return View(viewModels);
        }

        [Authorize(Roles = Roles.AdminAndRecipient)]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var recipient = await _recipientService.GetRecipientByUserIdAsync(user.Id)
                ?? (string.IsNullOrEmpty(user.Email) ? null : await _recipientService.GetRecipientByEmailAsync(user.Email));

            if (recipient == null)
            {
                recipient = new Recipient
                {
                    UserId = user.Id,
                    FirstName = user.FirstName ?? "Recipient",
                    LastName = user.LastName ?? "User",
                    Email = user.Email ?? string.Empty,
                    IsActive = true
                };
                await _recipientService.AddRecipientAsync(recipient);
            }

            var model = new RecipientViewModel
            {
                Id = recipient.Id,
                FirstName = recipient.FirstName,
                LastName = recipient.LastName,
                BloodGroup = recipient.BloodGroup,
                ContactNumber = recipient.ContactNumber,
                Email = recipient.Email,
                IsActive = recipient.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AdminAndRecipient)]
        public async Task<IActionResult> Profile(RecipientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var recipient = await _recipientService.GetRecipientByIdAsync(model.Id);
                if (recipient != null)
                {
                    recipient.FirstName = model.FirstName;
                    recipient.LastName = model.LastName;
                    recipient.BloodGroup = model.BloodGroup;
                    recipient.ContactNumber = model.ContactNumber;
                    await _recipientService.UpdateRecipientAsync(recipient);
                    TempData["SuccessMessage"] = "Recipient profile updated successfully.";
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }

        [Authorize(Roles = Roles.Admin)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create(RecipientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var recipient = new Recipient
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BloodGroup = model.BloodGroup,
                    ContactNumber = model.ContactNumber,
                    Email = model.Email,
                    IsActive = true
                };
                await _recipientService.AddRecipientAsync(recipient);
                TempData["SuccessMessage"] = "Recipient registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
