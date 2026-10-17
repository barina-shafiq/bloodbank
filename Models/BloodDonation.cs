using System;

namespace BloodBankManagement.Models
{
    public class BloodDonation
    {
        public int Id { get; set; }
        public Guid DonorId { get; set; }
        public Donor? Donor { get; set; }
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
        public int UnitsDonated { get; set; } = 1;
        public string BloodGroup { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

