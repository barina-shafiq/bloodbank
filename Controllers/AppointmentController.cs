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
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDonorService _donorService;
        private readonly IBloodDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(
            IAppointmentService appointmentService,
            IDonorService donorService,
            IBloodDonationService donationService,
            UserManager<ApplicationUser> userManager)
        {
            _appointmentService = appointmentService;
            _donorService = donorService;
            _donationService = donationService;
            _userManager = userManager;
        }

        [Authorize(Roles = Roles.AdminAndDonor)]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            var donors = await _donorService.GetAllDonorsAsync();

            var donorDict = donors.ToDictionary(d => d.Id);

            if (currentUser != null && !await _userManager.IsInRoleAsync(currentUser, Roles.Admin))
            {
                // Filter appointments for the logged-in donor
                var userDonor = donors.FirstOrDefault(d => d.UserId == currentUser.Id || d.Email == currentUser.Email);
                if (userDonor != null)
                {
                    appointments = appointments.Where(a => a.DonorId == userDonor.Id);
                }
                else
                {
                    appointments = Enumerable.Empty<Appointment>();
                }
            }

            var viewModels = appointments.OrderByDescending(a => a.AppointmentDate).Select(a => new AppointmentViewModel
            {
                Id = a.Id,
                DonorId = a.DonorId,
                DonorName = donorDict.TryGetValue(a.DonorId, out var donor) 
                    ? $"{donor.FirstName} {donor.LastName}" 
                    : (a.Donor != null ? $"{a.Donor.FirstName} {a.Donor.LastName}" : "Donor"),
                DonorBloodType = donorDict.TryGetValue(a.DonorId, out var d1) ? d1.BloodType : a.Donor?.BloodType,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                Notes = a.Notes
            }).ToList();

            return View(viewModels);
        }

        [Authorize(Roles = Roles.AdminAndDonor)]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && await _userManager.IsInRoleAsync(currentUser, Roles.Donor))
            {
                var donor = (await _donorService.GetAllDonorsAsync())
                    .FirstOrDefault(d => d.UserId == currentUser.Id || d.Email == currentUser.Email);

                if (donor != null && donor.LastDonationDate.HasValue)
                {
                    var daysSinceLast = (DateTime.UtcNow - donor.LastDonationDate.Value).TotalDays;
                    if (daysSinceLast < 56)
                    {
                        var eligibleDate = donor.LastDonationDate.Value.AddDays(56);
                        ViewBag.IntervalWarning = $"Note: Minimum donation interval is 56 days. Your last donation was on {donor.LastDonationDate.Value:MMM dd, yyyy}. You will be eligible on {eligibleDate:MMM dd, yyyy}.";
                    }
                }
            }

            var model = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Now.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.AdminAndDonor)]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            Donor? donor = null;

            if (await _userManager.IsInRoleAsync(currentUser, Roles.Donor))
            {
                donor = (await _donorService.GetAllDonorsAsync())
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

                // Minimum donation interval check (56 days)
                if (donor.LastDonationDate.HasValue)
                {
                    var daysSinceLast = (DateTime.UtcNow - donor.LastDonationDate.Value).TotalDays;
                    if (daysSinceLast < 56)
                    {
                        var eligibleDate = donor.LastDonationDate.Value.AddDays(56);
                        ModelState.AddModelError(string.Empty, $"Minimum donation interval is 56 days. Your last donation was on {donor.LastDonationDate.Value:MMM dd, yyyy}. You can donate again starting {eligibleDate:MMM dd, yyyy}.");
                        TempData["ErrorMessage"] = $"Cannot book appointment: minimum 56 days required between donations. Next eligible date: {eligibleDate:MMM dd, yyyy}.";
                        return View(model);
                    }
                }

                model.DonorId = donor.Id;
            }

            if (model.AppointmentDate <= DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Appointment date and time must be in the future.");
            }

            if (ModelState.IsValid)
            {
                var appointment = new Appointment
                {
                    DonorId = model.DonorId,
                    AppointmentDate = model.AppointmentDate,
                    Status = "Pending",
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _appointmentService.AddAppointmentAsync(appointment);
                TempData["SuccessMessage"] = "Donation appointment requested successfully! Status is Pending approval.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            var previousStatus = appointment.Status;
            appointment.Status = newStatus;
            await _appointmentService.UpdateAppointmentAsync(appointment);

            // If appointment is marked Completed, automatically add to inventory & record donation
            if (newStatus == "Completed" && previousStatus != "Completed")
            {
                var donor = await _donorService.GetDonorByIdAsync(appointment.DonorId);
                var bloodGroup = donor?.BloodType ?? "O+";

                var donation = new BloodDonation
                {
                    DonorId = appointment.DonorId,
                    AppointmentId = appointment.Id,
                    DonationDate = DateTime.UtcNow,
                    UnitsDonated = 1,
                    BloodGroup = bloodGroup,
                    CreatedAt = DateTime.UtcNow
                };

                await _donationService.AddDonationAsync(donation);

                if (donor != null)
                {
                    donor.LastDonationDate = DateTime.UtcNow;
                    await _donorService.UpdateDonorAsync(donor);
                }

                TempData["SuccessMessage"] = $"Appointment marked as Completed! 1 unit of blood ({bloodGroup}) added to inventory.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Appointment status updated to '{newStatus}'.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminAndDonor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickBook(Guid donorId)
        {
            var donor = await _donorService.GetDonorByIdAsync(donorId);
            if (donor == null)
            {
                TempData["ErrorMessage"] = "Donor record not found.";
                return RedirectToAction("Index", "Donor");
            }

            // Minimum donation interval check (56 days)
            if (donor.LastDonationDate.HasValue)
            {
                var daysSinceLast = (DateTime.UtcNow - donor.LastDonationDate.Value).TotalDays;
                if (daysSinceLast < 56)
                {
                    var eligibleDate = donor.LastDonationDate.Value.AddDays(56);
                    TempData["ErrorMessage"] = $"Cannot book appointment: minimum 56 days required between donations for {donor.FirstName} {donor.LastName}. Eligible starting: {eligibleDate:MMM dd, yyyy}.";
                    return RedirectToAction("Index", "Appointment");
                }
            }

            // Book for tomorrow at 10:00 AM
            var appointmentDate = DateTime.Now.AddDays(1).Date.AddHours(10);

            var appointment = new Appointment
            {
                DonorId = donorId,
                AppointmentDate = appointmentDate,
                Status = User.IsInRole(Roles.Admin) ? "Approved" : "Pending",
                Notes = "Automatically booked via Donors Directory",
                CreatedAt = DateTime.UtcNow
            };

            await _appointmentService.AddAppointmentAsync(appointment);
            TempData["SuccessMessage"] = $"Appointment automatically booked for {donor.FirstName} {donor.LastName}! Scheduled for {appointmentDate:MMM dd, yyyy HH:mm}.";

            return RedirectToAction("Index", "Appointment");
        }
    }
}

