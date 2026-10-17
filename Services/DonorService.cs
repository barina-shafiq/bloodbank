using BloodBankManagement.Models;
using BloodBankManagement.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodBankManagement.Services
{
    public interface IDonorService
    {
        Task<IEnumerable<Donor>> GetAllDonorsAsync();
        Task<Donor?> GetDonorByIdAsync(System.Guid id);
        Task AddDonorAsync(Donor donor);
        Task UpdateDonorAsync(Donor donor);
        Task DeleteDonorAsync(System.Guid id);
    }

    public class DonorService : IDonorService
    {
        private readonly IGenericRepository<Donor> _donorRepository;

        public DonorService(IGenericRepository<Donor> donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<IEnumerable<Donor>> GetAllDonorsAsync()
        {
            return await _donorRepository.GetAllAsync();
        }

        public async Task<Donor?> GetDonorByIdAsync(System.Guid id)
        {
            return await _donorRepository.GetByIdAsync(id);
        }

        public async Task AddDonorAsync(Donor donor)
        {
            await _donorRepository.AddAsync(donor);
            await _donorRepository.SaveChangesAsync();
        }

        public async Task UpdateDonorAsync(Donor donor)
        {
            _donorRepository.Update(donor);
            await _donorRepository.SaveChangesAsync();
        }

        public async Task DeleteDonorAsync(System.Guid id)
        {
            var donor = await _donorRepository.GetByIdAsync(id);
            if (donor != null)
            {
                _donorRepository.Remove(donor);
                await _donorRepository.SaveChangesAsync();
            }
        }
    }
}
