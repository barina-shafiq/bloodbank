using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BloodBankManagement.ViewModels
{
    public class BloodRequestViewModel
    {
        public int Id { get; set; }

        public int? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        
        public int? HospitalId { get; set; }
        public string? HospitalName { get; set; }

        public string? PatientName { get; set; }
        public string? PatientDetails { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Blood Group is required.")]
        public string BloodGroup { get; set; } = string.Empty;

        [Required(ErrorMessage = "Units Requested is required.")]
        [Range(1, 100, ErrorMessage = "Units must be between 1 and 100.")]
        public int UnitsRequested { get; set; } = 1;

        public string Status { get; set; } = "Pending";
        public string? Remarks { get; set; }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }

        public Guid DonorId { get; set; }
        public string? DonorName { get; set; }
        public string? DonorBloodType { get; set; }

        [Required(ErrorMessage = "Appointment Date & Time is required.")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
    }

    public class BloodDonationViewModel
    {
        public int Id { get; set; }

        public Guid DonorId { get; set; }
        public string? DonorName { get; set; }
        public int? AppointmentId { get; set; }

        [DataType(DataType.Date)]
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Units Donated is required.")]
        [Range(1, 10, ErrorMessage = "Units must be between 1 and 10.")]
        public int UnitsDonated { get; set; } = 1;

        [Required(ErrorMessage = "Blood Group is required.")]
        public string BloodGroup { get; set; } = string.Empty;
    }

    public class AdminDashboardViewModel
    {
        public int TotalDonors { get; set; }
        public int TotalRecipients { get; set; }
        public int TotalHospitals { get; set; }
        public int PendingAppointments { get; set; }
        public int PendingBloodRequests { get; set; }
        public int CompletedDonations { get; set; }
        public int TotalBloodUnits { get; set; }
        public List<BloodInventoryViewModel> InventoryList { get; set; } = new();
        public List<AppointmentViewModel> RecentAppointments { get; set; } = new();
        public List<BloodRequestViewModel> RecentRequests { get; set; } = new();
    }
}

