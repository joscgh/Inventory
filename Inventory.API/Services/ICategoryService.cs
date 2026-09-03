using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> ListCategoriesAsync();
        Task<Category?> FindByIdAsync(int id);
        Task<bool> RegisterCategoryAsync(Category category);
        Task<bool> ModifyCategoryAsync(Category category);
        Task<bool> RemoveCategoryAsync(int id);
    }
}
