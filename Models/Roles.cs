namespace BloodBankManagement.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Donor = "Donor";
        public const string Recipient = "Recipient";
        public const string Hospital = "Hospital";

        // Combined role strings for use in Authorize attributes
        public const string AdminAndHospital = Admin + "," + Hospital;
        public const string AdminAndDonor = Admin + "," + Donor;
        public const string AdminAndRecipient = Admin + "," + Recipient;
        public const string AdminDonorHospital = Admin + "," + Donor + "," + Hospital;
        public const string AdminRecipientHospital = Admin + "," + Recipient + "," + Hospital;
        public const string AllRoles = Admin + "," + Donor + "," + Recipient + "," + Hospital;
    }
}

