using BloodBankManagement.Models;
using BloodBankManagement.Services;
using BloodBankManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodBankManagement.Controllers
{
    [Authorize]
    public class BloodDonationController : Controller
    {
        private readonly IBloodDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDonorService _donorService;

        public BloodDonationController(
            IBloodDonationService donationService,
            UserManager<ApplicationUser> userManager,
            IDonorService donorService)
        {
            _donationService = donationService;
            _userManager = userManager;
            _donorService = donorService;
        }

        [Authorize(Roles = Models.Roles.Admin + "," + Models.Roles.Donor)]
        public async Task<IActionResult> Index()
        {
            var donations = await _donationService.GetAllDonationsAsync();
            
            var user = await _userManager.GetUserAsync(User);
            if (user != null && await _userManager.IsInRoleAsync(user, Models.Roles.Donor))
            {
                var donors = await _donorService.GetAllDonorsAsync();
                var donor = donors.FirstOrDefault(d => d.Email == user.Email);
                if (donor != null)
                {
                    donations = donations.Where(d => d.DonorId == donor.Id).ToList();
                }
                else
                {
                    donations = new List<BloodDonation>();
                }
            }

            var viewModels = donations.Select(d => new BloodDonationViewModel
            {
                Id = d.Id,
                DonorId = d.DonorId,
                DonationDate = d.DonationDate,
                UnitsDonated = d.UnitsDonated,
                BloodGroup = d.BloodGroup
            });
            return View(viewModels);
        }

        [Authorize(Roles = Models.Roles.Admin + "," + Models.Roles.Donor)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Models.Roles.Admin + "," + Models.Roles.Donor)]
        public async Task<IActionResult> Create(BloodDonationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var donation = new BloodDonation
                {
                    DonorId = model.DonorId,
                    DonationDate = model.DonationDate,
                    UnitsDonated = model.UnitsDonated,
                    BloodGroup = model.BloodGroup
                };
                // Adding a donation automatically updates the inventory via the service
                await _donationService.AddDonationAsync(donation);
                TempData["SuccessMessage"] = "Donation logged and inventory updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
