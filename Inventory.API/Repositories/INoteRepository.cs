using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetAllAsync();
        Task<Note?> GetByIdAsync(int id);
        Task<IEnumerable<Note>> GetByTypeAsync(NoteType type);
        Task AddAsync(Note note);
    }
}
