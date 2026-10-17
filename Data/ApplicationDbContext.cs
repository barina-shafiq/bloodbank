using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BloodBankManagement.Models;

namespace BloodBankManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Core entities
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<BloodInventory> BloodInventories { get; set; }
        public DbSet<BloodDonation> BloodDonations { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<BloodTypeLookup> BloodTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Donor>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Recipient>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Hospital>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<BloodRequest>()
                .HasOne(r => r.Recipient)
                .WithMany()
                .HasForeignKey(r => r.RecipientId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<BloodRequest>()
                .HasOne(r => r.Hospital)
                .WithMany()
                .HasForeignKey(r => r.HospitalId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Appointment>()
                .HasOne(a => a.Donor)
                .WithMany()
                .HasForeignKey(a => a.DonorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BloodDonation>()
                .HasOne(d => d.Donor)
                .WithMany()
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BloodDonation>()
                .HasOne(d => d.Appointment)
                .WithMany()
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
