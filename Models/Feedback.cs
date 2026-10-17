using System;

namespace BloodBankManagement.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Rating { get; set; } // 1 to 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
