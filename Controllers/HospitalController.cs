using BloodBankManagement.Models;
using BloodBankManagement.Services;
using BloodBankManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BloodBankManagement.Controllers
{
    [Authorize]
    public class HospitalController : Controller
    {
        private readonly IHospitalService _hospitalService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HospitalController(IHospitalService hospitalService, UserManager<ApplicationUser> userManager)
        {
            _hospitalService = hospitalService;
            _userManager = userManager;
        }

        [Authorize(Roles = Roles.AdminAndHospital)]
        public async Task<IActionResult> Index()
        {
            var hospitals = await _hospitalService.GetAllHospitalsAsync();
            var viewModels = hospitals.Select(h => new HospitalViewModel
            {
                Id = h.Id,
                Name = h.Name,
                ContactNumber = h.ContactNumber,
                Email = h.Email,
                Address = h.Address,
                IsActive = h.IsActive
            }).ToList();

            return View(viewModels);
        }

        [Authorize(Roles = Roles.AdminAndHospital)]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var hospital = (await _hospitalService.GetAllHospitalsAsync())
                .FirstOrDefault(h => h.UserId == user.Id || h.Email == user.Email);

            if (hospital == null)
            {
                hospital = new Hospital
                {
                    UserId = user.Id,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email ?? string.Empty,
                    IsActive = true
                };
                await _hospitalService.AddHospitalAsync(hospital);
            }

            var model = new HospitalViewModel
            {
                Id = hospital.Id,
                Name = hospital.Name,
                Address = hospital.Address,
                ContactNumber = hospital.ContactNumber,
                Email = hospital.Email,
                IsActive = hospital.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AdminAndHospital)]
        public async Task<IActionResult> Profile(HospitalViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hospital = await _hospitalService.GetHospitalByIdAsync(model.Id);
                if (hospital != null)
                {
                    hospital.Name = model.Name;
                    hospital.Address = model.Address;
                    hospital.ContactNumber = model.ContactNumber;
                    await _hospitalService.UpdateHospitalAsync(hospital);
                    TempData["SuccessMessage"] = "Hospital profile updated successfully.";
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
        public async Task<IActionResult> Create(HospitalViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hospital = new Hospital
                {
                    Name = model.Name,
                    ContactNumber = model.ContactNumber,
                    Email = model.Email,
                    Address = model.Address,
                    IsActive = true
                };
                await _hospitalService.AddHospitalAsync(hospital);
                TempData["SuccessMessage"] = "Hospital registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}

