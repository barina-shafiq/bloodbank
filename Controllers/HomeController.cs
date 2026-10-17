using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BloodBankManagement.Models;
using BloodBankManagement.Services;
using BloodBankManagement.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace BloodBankManagement.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDonorService _donorService;
    private readonly IRecipientService _recipientService;
    private readonly IHospitalService _hospitalService;
    private readonly IBloodInventoryService _inventoryService;
    private readonly IAppointmentService _appointmentService;
    private readonly IBloodRequestService _requestService;
    private readonly IBloodDonationService _donationService;

    public HomeController(
        UserManager<ApplicationUser> userManager,
        IDonorService donorService,
        IRecipientService recipientService,
        IHospitalService hospitalService,
        IBloodInventoryService inventoryService,
        IAppointmentService appointmentService,
        IBloodRequestService requestService,
        IBloodDonationService donationService)
    {
        _userManager = userManager;
        _donorService = donorService;
        _recipientService = recipientService;
        _hospitalService = hospitalService;
        _inventoryService = inventoryService;
        _appointmentService = appointmentService;
        _requestService = requestService;
        _donationService = donationService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole(Roles.Admin))
        {
            var donors = await _donorService.GetAllDonorsAsync();
            var recipients = await _recipientService.GetAllRecipientsAsync();
            var hospitals = await _hospitalService.GetAllHospitalsAsync();
            var inventory = await _inventoryService.GetInventoryAsync();
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            var requests = await _requestService.GetAllRequestsAsync();
            var donations = await _donationService.GetAllDonationsAsync();

            var model = new AdminDashboardViewModel
            {
                TotalDonors = donors.Count(),
                TotalRecipients = recipients.Count(),
                TotalHospitals = hospitals.Count(),
                PendingAppointments = appointments.Count(a => a.Status == "Pending"),
                PendingBloodRequests = requests.Count(r => r.Status == "Pending"),
                CompletedDonations = donations.Count(),
                TotalBloodUnits = inventory.Sum(i => i.Units),
                InventoryList = inventory.Select(i => new BloodInventoryViewModel
                {
                    Id = i.Id,
                    BloodGroup = i.BloodGroup,
                    Units = i.Units,
                    LastUpdated = i.LastUpdated
                }).OrderBy(i => i.BloodGroup).ToList(),
                RecentAppointments = appointments.OrderByDescending(a => a.AppointmentDate).Take(5).Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    DonorId = a.DonorId,
                    DonorName = a.Donor != null ? $"{a.Donor.FirstName} {a.Donor.LastName}" : "Donor #" + a.DonorId.ToString().Substring(0, 5),
                    DonorBloodType = a.Donor?.BloodType,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Notes = a.Notes
                }).ToList(),
                RecentRequests = requests.OrderByDescending(r => r.RequestDate).Take(5).Select(r => new BloodRequestViewModel
                {
                    Id = r.Id,
                    RecipientId = r.RecipientId,
                    RecipientName = r.Recipient != null ? $"{r.Recipient.FirstName} {r.Recipient.LastName}" : null,
                    HospitalId = r.HospitalId,
                    HospitalName = r.Hospital != null ? r.Hospital.Name : null,
                    PatientName = r.PatientName,
                    RequestDate = r.RequestDate,
                    BloodGroup = r.BloodGroup,
                    UnitsRequested = r.UnitsRequested,
                    Status = r.Status
                }).ToList()
            };

            return View(model);
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Roles = Roles.Recipient)]
    [HttpGet]
    public async Task<IActionResult> RecipientProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var recipient = await _recipientService.GetRecipientByUserIdAsync(user.Id);
        if (recipient == null && !string.IsNullOrEmpty(user.Email))
        {
            recipient = await _recipientService.GetRecipientByEmailAsync(user.Email);
        }

        if (recipient == null)
        {
            recipient = new Recipient
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                BloodGroup = string.Empty,
                ContactNumber = user.PhoneNumber ?? string.Empty,
                IsActive = true
            };
            await _recipientService.AddRecipientAsync(recipient);
        }

        var viewModel = new RecipientViewModel
        {
            Id = recipient.Id,
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            BloodGroup = recipient.BloodGroup,
            ContactNumber = recipient.ContactNumber,
            Email = recipient.Email,
            IsActive = recipient.IsActive
        };

        return View(viewModel);
    }

    [Authorize(Roles = Roles.Recipient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecipientProfile(RecipientViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            var recipient = await _recipientService.GetRecipientByUserIdAsync(user.Id) 
                ?? (!string.IsNullOrEmpty(user.Email) ? await _recipientService.GetRecipientByEmailAsync(user.Email) : null);

            if (recipient != null)
            {
                recipient.FirstName = model.FirstName;
                recipient.LastName = model.LastName;
                recipient.BloodGroup = model.BloodGroup;
                recipient.ContactNumber = model.ContactNumber;
                await _recipientService.UpdateRecipientAsync(recipient);
                TempData["SuccessMessage"] = "Recipient profile updated successfully!";
                return RedirectToAction("Index", "Home");
            }
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        string? exceptionMessage = null;
        string? stackTrace = null;

        if (exception != null)
        {
            exceptionMessage = exception.Message;
            stackTrace = exception.StackTrace;
        }

        return View(new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ExceptionMessage = exceptionMessage,
            StackTrace = stackTrace
        });
    }
}

