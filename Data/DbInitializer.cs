using BloodBankManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BloodBankManagement.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitialization");
            var context = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Automatic Database Migration: Checking pending EF Core migrations.");
                await context.Database.MigrateAsync();
                logger.LogInformation("Automatic Database Migration: Applied database migrations successfully.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "MigrateAsync failed or no migration history table. Falling back to EnsureCreatedAsync.");
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Automatic Database Creation: Database schema ensured via EnsureCreatedAsync.");
                }
                catch (Exception innerEx)
                {
                    logger.LogError(innerEx, "Failed to ensure database creation.");
                }
            }

            try
            {
                logger.LogInformation("Automatic Data Seeding: Starting roles, admin user, and lookup seeding.");
                await SeedRolesAndAdminAsync(services);
                await SeedLookupDataAsync(services);
                logger.LogInformation("Automatic Data Seeding: Completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during data seeding.");
            }
        }

        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { Roles.Admin, Roles.Donor, Roles.Recipient, Roles.Hospital };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to create role '{roleName}': {FormatIdentityErrors(roleResult)}");
                    }
                }
            }

            // Create default admin user
            var adminEmail = "admin@bloodbank.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createPowerUser = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    if (!addToRoleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to add default admin to '{Roles.Admin}' role: {FormatIdentityErrors(addToRoleResult)}");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Failed to create default admin user: {FormatIdentityErrors(createPowerUser)}");
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                if (!addToRoleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to add existing admin user to '{Roles.Admin}' role: {FormatIdentityErrors(addToRoleResult)}");
                }
            }
        }

        public static async Task SeedLookupDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var bloodGroups = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

            // Seed blood types lookup
            foreach (var bg in bloodGroups)
            {
                if (!await context.BloodTypes.AnyAsync(b => b.BloodGroup == bg))
                {
                    context.BloodTypes.Add(new Models.BloodTypeLookup { BloodGroup = bg });
                }
            }
            await context.SaveChangesAsync();

            // Seed blood inventory stock for all 8 blood groups
            foreach (var bg in bloodGroups)
            {
                if (!await context.BloodInventories.AnyAsync(b => b.BloodGroup == bg))
                {
                    int initialUnits = bg switch
                    {
                        "O+" => 15,
                        "A+" => 12,
                        "B+" => 10,
                        "AB+" => 6,
                        "O-" => 8,
                        "A-" => 5,
                        "B-" => 4,
                        "AB-" => 3,
                        _ => 5
                    };
                    context.BloodInventories.Add(new Models.BloodInventory 
                    { 
                        BloodGroup = bg, 
                        Units = initialUnits, 
                        LastUpdated = DateTime.UtcNow 
                    });
                }
            }
            await context.SaveChangesAsync();

            // Seed sample hospitals
            var hospitals = new[] {
                new Models.Hospital { Name = "City General Hospital", Address = "123 Health Ave, Metro City", ContactNumber = "555-0199", Email = "contact@citygeneral.org", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.Hospital { Name = "St. Jude Children Hospital", Address = "456 Care Lane, Metro City", ContactNumber = "555-0244", Email = "info@stjude.org", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.Hospital { Name = "Red Cross Medical Center", Address = "789 Hope Blvd, Metro City", ContactNumber = "555-0377", Email = "requests@redcrossmed.org", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            foreach (var hospital in hospitals)
            {
                if (!await context.Hospitals.AnyAsync(h => h.Name == hospital.Name))
                {
                    context.Hospitals.Add(hospital);
                }
            }

            await context.SaveChangesAsync();
        }

        private static string FormatIdentityErrors(IdentityResult result)
        {
            return string.Join("; ", result.Errors.Select(error => error.Description));
        }
    }
}
