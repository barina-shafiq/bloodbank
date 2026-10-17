using BloodBankManagement.Models;
using BloodBankManagement.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodBankManagement.Services
{
    public interface IBloodDonationService
    {
        Task<IEnumerable<BloodDonation>> GetAllDonationsAsync();
        Task<BloodDonation?> GetDonationByIdAsync(int id);
        Task AddDonationAsync(BloodDonation donation);
    }

    public class BloodDonationService : IBloodDonationService
    {
        private readonly IGenericRepository<BloodDonation> _donationRepository;
        private readonly IBloodInventoryService _inventoryService;

        public BloodDonationService(
            IGenericRepository<BloodDonation> donationRepository,
            IBloodInventoryService inventoryService)
        {
            _donationRepository = donationRepository;
            _inventoryService = inventoryService;
        }

        public async Task<IEnumerable<BloodDonation>> GetAllDonationsAsync()
        {
            return await _donationRepository.GetAllAsync();
        }

        public async Task<BloodDonation?> GetDonationByIdAsync(int id)
        {
            return await _donationRepository.GetByIdAsync(id);
        }

        public async Task AddDonationAsync(BloodDonation donation)
        {
            await _donationRepository.AddAsync(donation);
            await _donationRepository.SaveChangesAsync();
            
            // Automatically increase the inventory
            await _inventoryService.UpdateInventoryAsync(donation.BloodGroup, donation.UnitsDonated);
        }
    }
}
