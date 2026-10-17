using System;

namespace BloodBankManagement.Models
{
    public class BloodInventory
    {
        public int Id { get; set; }
        public string BloodGroup { get; set; } = string.Empty;
        public int Units { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
