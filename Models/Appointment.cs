using System;

namespace BloodBankManagement.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public Guid DonorId { get; set; }
        public Donor? Donor { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

