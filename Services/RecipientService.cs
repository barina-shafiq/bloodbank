using BloodBankManagement.Models;
using BloodBankManagement.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodBankManagement.Services
{
    public interface IRecipientService
    {
        Task<IEnumerable<Recipient>> GetAllRecipientsAsync();
        Task<Recipient?> GetRecipientByIdAsync(int id);
        Task<Recipient?> GetRecipientByEmailAsync(string email);
        Task<Recipient?> GetRecipientByUserIdAsync(string userId);
        Task AddRecipientAsync(Recipient recipient);
        Task UpdateRecipientAsync(Recipient recipient);
        Task DeleteRecipientAsync(int id);
    }

    public class RecipientService : IRecipientService
    {
        private readonly IGenericRepository<Recipient> _recipientRepository;

        public RecipientService(IGenericRepository<Recipient> recipientRepository)
        {
            _recipientRepository = recipientRepository;
        }

        public async Task<IEnumerable<Recipient>> GetAllRecipientsAsync()
        {
            return await _recipientRepository.GetAllAsync();
        }

        public async Task<Recipient?> GetRecipientByIdAsync(int id)
        {
            return await _recipientRepository.GetByIdAsync(id);
        }

        public async Task<Recipient?> GetRecipientByEmailAsync(string email)
        {
            var results = await _recipientRepository.FindAsync(r => r.Email == email);
            return results.FirstOrDefault();
        }

        public async Task<Recipient?> GetRecipientByUserIdAsync(string userId)
        {
            var results = await _recipientRepository.FindAsync(r => r.UserId == userId);
            return results.FirstOrDefault();
        }

        public async Task AddRecipientAsync(Recipient recipient)
        {
            await _recipientRepository.AddAsync(recipient);
            await _recipientRepository.SaveChangesAsync();
        }

        public async Task UpdateRecipientAsync(Recipient recipient)
        {
            _recipientRepository.Update(recipient);
            await _recipientRepository.SaveChangesAsync();
        }

        public async Task DeleteRecipientAsync(int id)
        {
            var recipient = await _recipientRepository.GetByIdAsync(id);
            if (recipient != null)
            {
                _recipientRepository.Remove(recipient);
                await _recipientRepository.SaveChangesAsync();
            }
        }
    }
}
