using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Category>> ListCategoriesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Category?> FindByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterCategoryAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return false;
            }

            var existing = await _repository.GetByNameAsync(category.Name.Trim());
            if (existing != null) return false;

            category.Name = category.Name.Trim();
            category.Description = category.Description?.Trim() ?? string.Empty;

            await _repository.AddAsync(category);
            return true;
        }

        public async Task<bool> ModifyCategoryAsync(Category category)
        {
            var existing = await _repository.GetByIdAsync(category.Id);
            if (existing == null) return false;

            existing.Name = category.Name.Trim();
            existing.Description = category.Description?.Trim() ?? string.Empty;

            await _repository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> RemoveCategoryAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
