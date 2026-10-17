using System;

namespace BloodBankManagement.Models
{
    public class BloodRequest
    {
        public int Id { get; set; }
        public int? RecipientId { get; set; }
        public Recipient? Recipient { get; set; }
        public int? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }
        public string? PatientName { get; set; }
        public string? PatientDetails { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public string BloodGroup { get; set; } = string.Empty;
        public int UnitsRequested { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed
        public string? Remarks { get; set; }
    }
}

