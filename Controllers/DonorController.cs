using BloodBankManagement.Models;
using BloodBankManagement.Services;
using BloodBankManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodBankManagement.Controllers
{
    [Authorize]
    public class DonorController : Controller
    {
        private readonly IDonorService _donorService;
        private readonly ILogger<DonorController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonorController(IDonorService donorService, ILogger<DonorController> logger, UserManager<ApplicationUser> userManager)
        {
            _donorService = donorService;
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize(Roles = Roles.AdminDonorHospital)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var donors = await _donorService.GetAllDonorsAsync() ?? Enumerable.Empty<Donor>();
                var viewModels = donors.Select(d => new DonorViewModel
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    BloodType = d.BloodType,
                    DateOfBirth = d.DateOfBirth,
                    ContactNumber = d.ContactNumber,
                    Email = d.Email,
                    LastDonationDate = d.LastDonationDate,
                    IsActive = d.IsActive
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load donors.");
                return View(new List<DonorViewModel>());
            }
        }

        [Authorize(Roles = Roles.AdminAndDonor)]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var donor = (await _donorService.GetAllDonorsAsync())
                .FirstOrDefault(d => d.UserId == currentUser.Id || d.Email == currentUser.Email);

            if (donor == null)
            {
                donor = new Donor
                {
                    UserId = currentUser.Id,
                    FirstName = currentUser.FirstName ?? "Donor",
                    LastName = currentUser.LastName ?? "User",
                    Email = currentUser.Email ?? string.Empty,
                    IsActive = true
                };
                await _donorService.AddDonorAsync(donor);
            }

            var model = new DonorViewModel
            {
                Id = donor.Id,
                FirstName = donor.FirstName,
                LastName = donor.LastName,
                BloodType = donor.BloodType,
                DateOfBirth = donor.DateOfBirth,
                ContactNumber = donor.ContactNumber,
                Email = donor.Email,
                LastDonationDate = donor.LastDonationDate,
                IsActive = donor.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AdminAndDonor)]
        public async Task<IActionResult> Profile(DonorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var donor = await _donorService.GetDonorByIdAsync(model.Id);
                if (donor != null)
                {
                    donor.FirstName = model.FirstName;
                    donor.LastName = model.LastName;
                    donor.BloodType = model.BloodType;
                    donor.DateOfBirth = model.DateOfBirth;
                    donor.ContactNumber = model.ContactNumber;
                    await _donorService.UpdateDonorAsync(donor);
                    TempData["SuccessMessage"] = "Donor profile updated successfully!";
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }

        [Authorize(Roles = Roles.AllRoles)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Create(DonorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var donor = new Donor
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BloodType = model.BloodType,
                    DateOfBirth = model.DateOfBirth,
                    ContactNumber = model.ContactNumber,
                    Email = model.Email,
                    IsActive = true
                };

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && await _userManager.IsInRoleAsync(currentUser, Roles.Donor))
                {
                    donor.UserId = currentUser.Id;
                    donor.Email = currentUser.Email ?? model.Email;
                }

                await _donorService.AddDonorAsync(donor);
                TempData["SuccessMessage"] = "Donor profile registered successfully!";

                if (currentUser != null && await _userManager.IsInRoleAsync(currentUser, Roles.Donor))
                {
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}

