using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.RazorLib.Services
{
    public class NoteApiService
    {
        private readonly HttpClient _http;

        public NoteApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Note>> GetNotesAsync(string? type = null)
        {
            var url = string.IsNullOrWhiteSpace(type) ? "api/notes" : $"api/notes?type={type}";
            return await _http.GetFromJsonAsync<List<Note>>(url) ?? new List<Note>();
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Note>($"api/notes/{id}");
        }

        public async Task<List<Note>> GetNotesByTypeAsync(NoteType type)
        {
            return await GetNotesAsync(type.ToString());
        }

        public async Task<(bool Success, string? ErrorMessage)> SaveNoteAsync(Note note)
        {
            var response = await _http.PostAsJsonAsync("api/notes", note);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var errorText = await response.Content.ReadAsStringAsync();
            var message = string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText;
            return (false, message);
        }
    }
}
