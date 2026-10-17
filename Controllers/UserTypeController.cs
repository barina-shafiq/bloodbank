using BloodBankManagement.Models;
using BloodBankManagement.Services;
using BloodBankManagement.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BloodBankManagement.Controllers
{
    [Authorize]
    public class UserTypeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDonorService _donorService;
        private readonly IGenericRepository<Recipient> _recipientRepository;

        public UserTypeController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDonorService donorService,
            IGenericRepository<Recipient> recipientRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _donorService = donorService;
            _recipientRepository = recipientRepository;
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Select()
        {
            if (User.IsInRole(Roles.Donor) || User.IsInRole(Roles.Recipient) || User.IsInRole(Roles.Hospital) || User.IsInRole(Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Select(string selectedType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (selectedType == "Donor")
            {
                // add Donor role
                if (!await _userManager.IsInRoleAsync(user, Roles.Donor))
                {
                    await _userManager.AddToRoleAsync(user, Roles.Donor);
                }

                // remove Hospital role if they had it
                if (await _userManager.IsInRoleAsync(user, Roles.Hospital))
                {
                    await _userManager.RemoveFromRoleAsync(user, Roles.Hospital);
                }

                // Create a donor record if none exists with this email
                var existingDonors = await _donorService.GetAllDonorsAsync();
                if (!existingDonors.Any(d => d.Email == user.Email))
                {
                    var donor = new Donor
                    {
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        ContactNumber = string.Empty,
                        BloodType = string.Empty,
                        IsActive = true
                    };
                    await _donorService.AddDonorAsync(donor);
                }

                // Refresh cookie claims instantly
                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction("Index", "Home");
            }

            // treat any other selection as Recipient (Hospital role in this system)
            if (selectedType == "Recipient")
            {
                if (!await _userManager.IsInRoleAsync(user, Roles.Hospital))
                {
                    await _userManager.AddToRoleAsync(user, Roles.Hospital);
                }

                // remove Donor role if they had it
                if (await _userManager.IsInRoleAsync(user, Roles.Donor))
                {
                    await _userManager.RemoveFromRoleAsync(user, Roles.Donor);
                }

                // create recipient record only if one doesn't already exist for this email
                var existingRecipients = await _recipientRepository.FindAsync(r => r.Email == user.Email);
                if (!existingRecipients.Any())
                {
                    var recipient = new Recipient
                    {
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        BloodGroup = string.Empty,
                        ContactNumber = string.Empty,
                        IsActive = true
                    };

                    await _recipientRepository.AddAsync(recipient);
                    await _recipientRepository.SaveChangesAsync();
                }

                // Refresh cookie claims instantly
                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
