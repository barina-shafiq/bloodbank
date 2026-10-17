using BloodBankManagement.Models;
using BloodBankManagement.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodBankManagement.Services
{
    public interface IHospitalService
    {
        Task<IEnumerable<Hospital>> GetAllHospitalsAsync();
        Task<Hospital?> GetHospitalByIdAsync(int id);
        Task AddHospitalAsync(Hospital hospital);
        Task UpdateHospitalAsync(Hospital hospital);
        Task DeleteHospitalAsync(int id);
    }

    public class HospitalService : IHospitalService
    {
        private readonly IGenericRepository<Hospital> _hospitalRepository;

        public HospitalService(IGenericRepository<Hospital> hospitalRepository)
        {
            _hospitalRepository = hospitalRepository;
        }

        public async Task<IEnumerable<Hospital>> GetAllHospitalsAsync()
        {
            return await _hospitalRepository.GetAllAsync();
        }

        public async Task<Hospital?> GetHospitalByIdAsync(int id)
        {
            return await _hospitalRepository.GetByIdAsync(id);
        }

        public async Task AddHospitalAsync(Hospital hospital)
        {
            await _hospitalRepository.AddAsync(hospital);
            await _hospitalRepository.SaveChangesAsync();
        }

        public async Task UpdateHospitalAsync(Hospital hospital)
        {
            _hospitalRepository.Update(hospital);
            await _hospitalRepository.SaveChangesAsync();
        }

        public async Task DeleteHospitalAsync(int id)
        {
            var hospital = await _hospitalRepository.GetByIdAsync(id);
            if (hospital != null)
            {
                _hospitalRepository.Remove(hospital);
                await _hospitalRepository.SaveChangesAsync();
            }
        }
    }
}
