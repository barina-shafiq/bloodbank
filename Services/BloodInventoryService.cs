using BloodBankManagement.Models;
using BloodBankManagement.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodBankManagement.Services
{
    public interface IBloodInventoryService
    {
        Task<IEnumerable<BloodInventory>> GetInventoryAsync();
        Task UpdateInventoryAsync(string bloodGroup, int unitsToAddOrRemove);
    }

    public class BloodInventoryService : IBloodInventoryService
    {
        private readonly IGenericRepository<BloodInventory> _inventoryRepository;

        public BloodInventoryService(IGenericRepository<BloodInventory> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<BloodInventory>> GetInventoryAsync()
        {
            return await _inventoryRepository.GetAllAsync();
        }

        public async Task UpdateInventoryAsync(string bloodGroup, int unitsToAddOrRemove)
        {
            var inventoryRecords = await _inventoryRepository.FindAsync(i => i.BloodGroup == bloodGroup);
            var inventory = System.Linq.Enumerable.FirstOrDefault(inventoryRecords);

            if (inventory == null)
            {
                inventory = new BloodInventory
                {
                    BloodGroup = bloodGroup,
                    Units = unitsToAddOrRemove > 0 ? unitsToAddOrRemove : 0
                };
                await _inventoryRepository.AddAsync(inventory);
            }
            else
            {
                inventory.Units += unitsToAddOrRemove;
                if (inventory.Units < 0) inventory.Units = 0;
                inventory.LastUpdated = System.DateTime.UtcNow;
                _inventoryRepository.Update(inventory);
            }

            await _inventoryRepository.SaveChangesAsync();
        }
    }
}
