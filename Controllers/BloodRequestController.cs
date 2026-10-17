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
    public class BloodRequestController : Controller
    {
        private readonly IBloodRequestService _requestService;
        private readonly IBloodInventoryService _inventoryService;
        private readonly IRecipientService _recipientService;
        private readonly IHospitalService _hospitalService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BloodRequestController> _logger;

        public BloodRequestController(
            IBloodRequestService requestService,
            IBloodInventoryService inventoryService,
            IRecipientService recipientService,
            IHospitalService hospitalService,
            UserManager<ApplicationUser> userManager,
            ILogger<BloodRequestController> logger)
        {
            _requestService = requestService;
            _inventoryService = inventoryService;
            _recipientService = recipientService;
            _hospitalService = hospitalService;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = Roles.AdminRecipientHospital)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var requests = await _requestService.GetAllRequestsAsync();
                var recipients = await _recipientService.GetAllRecipientsAsync();
                var hospitals = await _hospitalService.GetAllHospitalsAsync();

                var recipientDict = recipients.ToDictionary(r => r.Id);
                var hospitalDict = hospitals.ToDictionary(h => h.Id);

                if (currentUser != null && !await _userManager.IsInRoleAsync(currentUser, Roles.Admin))
                {
                    if (await _userManager.IsInRoleAsync(currentUser, Roles.Recipient))
                    {
                        var userRecipient = recipients.FirstOrDefault(r => r.UserId == currentUser.Id || r.Email == currentUser.Email);
                        requests = userRecipient != null 
                            ? requests.Where(r => r.RecipientId == userRecipient.Id) 
                            : Enumerable.Empty<BloodRequest>();
                    }
                    else if (await _userManager.IsInRoleAsync(currentUser, Roles.Hospital))
                    {
                        var userHospital = hospitals.FirstOrDefault(h => h.UserId == currentUser.Id || h.Email == currentUser.Email);
                        requests = userHospital != null 
                            ? requests.Where(r => r.HospitalId == userHospital.Id) 
                            : Enumerable.Empty<BloodRequest>();
                    }
                }

                var viewModels = requests.OrderByDescending(r => r.RequestDate).Select(r => new BloodRequestViewModel
                {
                    Id = r.Id,
                    RecipientId = r.RecipientId,
                    RecipientName = r.RecipientId.HasValue && recipientDict.TryGetValue(r.RecipientId.Value, out var rec) 
                        ? $"{rec.FirstName} {rec.LastName}" 
                        : (r.Recipient != null ? $"{r.Recipient.FirstName} {r.Recipient.LastName}" : null),
                    HospitalId = r.HospitalId,
                    HospitalName = r.HospitalId.HasValue && hospitalDict.TryGetValue(r.HospitalId.Value, out var hosp) 
                        ? hosp.Name 
                        : (r.Hospital != null ? r.Hospital.Name : null),
                    PatientName = r.PatientName,
                    PatientDetails = r.PatientDetails,
                    RequestDate = r.RequestDate,
                    BloodGroup = r.BloodGroup,
                    UnitsRequested = r.UnitsRequested,
                    Status = r.Status,
                    Remarks = r.Remarks
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load blood requests.");
                return View(new List<BloodRequestViewModel>());
            }
        }

        [Authorize(Roles = Roles.AdminRecipientHospital)]
        public async Task<IActionResult> Create()
        {
            var inventory = await _inventoryService.GetInventoryAsync();
            ViewBag.InventoryStock = inventory.ToDictionary(i => i.BloodGroup, i => i.Units);
            ViewBag.Hospitals = await _hospitalService.GetAllHospitalsAsync();

            var model = new BloodRequestViewModel
            {
                UnitsRequested = 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AdminRecipientHospital)]
        public async Task<IActionResult> Create(BloodRequestViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (model.UnitsRequested <= 0)
            {
                ModelState.AddModelError("UnitsRequested", "Units requested must be greater than 0.");
            }

            if (ModelState.IsValid)
            {
                var request = new BloodRequest
                {
                    RequestDate = DateTime.UtcNow,
                    BloodGroup = model.BloodGroup,
                    UnitsRequested = model.UnitsRequested,
                    PatientName = model.PatientName,
                    PatientDetails = model.PatientDetails,
                    Status = "Pending"
                };

                if (await _userManager.IsInRoleAsync(currentUser, Roles.Recipient))
                {
                    var recipient = await _recipientService.GetRecipientByUserIdAsync(currentUser.Id)
                        ?? (string.IsNullOrEmpty(currentUser.Email) ? null : await _recipientService.GetRecipientByEmailAsync(currentUser.Email));

                    if (recipient == null)
                    {
                        recipient = new Recipient
                        {
                            UserId = currentUser.Id,
                            FirstName = currentUser.FirstName ?? "Recipient",
                            LastName = currentUser.LastName ?? "User",
                            Email = currentUser.Email ?? string.Empty,
                            BloodGroup = model.BloodGroup,
                            IsActive = true
                        };
                        await _recipientService.AddRecipientAsync(recipient);
                    }
                    request.RecipientId = recipient.Id;
                }
                else if (await _userManager.IsInRoleAsync(currentUser, Roles.Hospital))
                {
                    var hospital = (await _hospitalService.GetAllHospitalsAsync())
                        .FirstOrDefault(h => h.UserId == currentUser.Id || h.Email == currentUser.Email);

                    if (hospital == null)
                    {
                        hospital = new Hospital
                        {
                            UserId = currentUser.Id,
                            Name = $"{currentUser.FirstName} {currentUser.LastName}".Trim(),
                            Email = currentUser.Email ?? string.Empty,
                            IsActive = true
                        };
                        await _hospitalService.AddHospitalAsync(hospital);
                    }
                    request.HospitalId = hospital.Id;
                }
                else if (await _userManager.IsInRoleAsync(currentUser, Roles.Admin))
                {
                    if (model.HospitalId.HasValue) request.HospitalId = model.HospitalId.Value;
                    if (model.RecipientId.HasValue) request.RecipientId = model.RecipientId.Value;
                }

                await _requestService.AddRequestAsync(request);
                TempData["SuccessMessage"] = $"Blood request for {model.UnitsRequested} unit(s) of {model.BloodGroup} submitted successfully! Status is Pending.";
                return RedirectToAction(nameof(Index));
            }

            var inventory = await _inventoryService.GetInventoryAsync();
            ViewBag.InventoryStock = inventory.ToDictionary(i => i.BloodGroup, i => i.Units);
            ViewBag.Hospitals = await _hospitalService.GetAllHospitalsAsync();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus, string? remarks = null)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                TempData["ErrorMessage"] = "Blood request not found.";
                return RedirectToAction(nameof(Index));
            }

            var previousStatus = request.Status;

            // Approving blood request business rule check: stock must be sufficient
            if (newStatus == "Approved" && previousStatus != "Approved" && previousStatus != "Completed")
            {
                var inventoryList = await _inventoryService.GetInventoryAsync();
                var stockItem = inventoryList.FirstOrDefault(i => i.BloodGroup == request.BloodGroup);
                int availableUnits = stockItem?.Units ?? 0;

                if (availableUnits < request.UnitsRequested)
                {
                    TempData["ErrorMessage"] = $"Cannot approve blood request! Insufficient stock for '{request.BloodGroup}'. Stock available: {availableUnits} unit(s), requested: {request.UnitsRequested} unit(s).";
                    return RedirectToAction(nameof(Index));
                }

                // Automatically deduct units from inventory
                await _inventoryService.UpdateInventoryAsync(request.BloodGroup, -request.UnitsRequested);
                request.Status = "Approved";
                if (!string.IsNullOrWhiteSpace(remarks)) request.Remarks = remarks;
                await _requestService.UpdateRequestAsync(request);

                TempData["SuccessMessage"] = $"Blood request #{request.Id} approved! {request.UnitsRequested} unit(s) of {request.BloodGroup} deducted from inventory.";
            }
            else
            {
                request.Status = newStatus;
                if (!string.IsNullOrWhiteSpace(remarks)) request.Remarks = remarks;
                await _requestService.UpdateRequestAsync(request);

                TempData["SuccessMessage"] = $"Blood request #{request.Id} updated to status '{newStatus}'.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

