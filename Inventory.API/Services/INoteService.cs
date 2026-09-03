using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> ListNotesAsync(NoteType? type = null);
        Task<Note?> FindByIdAsync(int id);
        Task AddNoteAsync(Note note);
    }
}
