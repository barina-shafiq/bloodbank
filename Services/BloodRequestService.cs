using BloodBankManagement.Models;
using BloodBankManagement.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodBankManagement.Services
{
    public interface IBloodRequestService
    {
        Task<IEnumerable<BloodRequest>> GetAllRequestsAsync();
        Task<BloodRequest?> GetRequestByIdAsync(int id);
        Task AddRequestAsync(BloodRequest request);
        Task UpdateRequestAsync(BloodRequest request);
    }

    public class BloodRequestService : IBloodRequestService
    {
        private readonly IGenericRepository<BloodRequest> _requestRepository;

        public BloodRequestService(IGenericRepository<BloodRequest> requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<IEnumerable<BloodRequest>> GetAllRequestsAsync()
        {
            // In a real app we'd eagerly load Hospital/Recipient via a specialized repo or Include
            return await _requestRepository.GetAllAsync();
        }

        public async Task<BloodRequest?> GetRequestByIdAsync(int id)
        {
            return await _requestRepository.GetByIdAsync(id);
        }

        public async Task AddRequestAsync(BloodRequest request)
        {
            await _requestRepository.AddAsync(request);
            await _requestRepository.SaveChangesAsync();
        }

        public async Task UpdateRequestAsync(BloodRequest request)
        {
            _requestRepository.Update(request);
            await _requestRepository.SaveChangesAsync();
        }
    }
}
