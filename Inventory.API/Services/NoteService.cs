using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _repository;

        public NoteService(INoteRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Note>> ListNotesAsync(NoteType? type = null)
        {
            return type.HasValue
                ? await _repository.GetByTypeAsync(type.Value)
                : await _repository.GetAllAsync();
        }

        public async Task<Note?> FindByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddNoteAsync(Note note)
        {
            await _repository.AddAsync(note);
        }
    }
}
