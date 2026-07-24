using Inventory.API.Services;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _service;

        public NotesController(INoteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] NoteType? type)
        {
            var notes = await _service.ListNotesAsync(type);
            return Ok(notes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var note = await _service.FindByIdAsync(id);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Note note)
        {
            if (note == null) return BadRequest("Nota inválida.");

            if (note.Lines == null || !note.Lines.Any())
            {
                return BadRequest("La nota debe incluir al menos un producto.");
            }

            if (string.IsNullOrWhiteSpace(note.CustomerName) && string.IsNullOrWhiteSpace(note.CustomerDocument))
            {
                return BadRequest("La nota debe incluir al menos el cliente que compra o consume el servicio.");
            }

            if (note.CreatedByUserId <= 0)
            {
                return BadRequest("La nota debe registrar el usuario que la creó.");
            }

            await _service.AddNoteAsync(note);
            return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
        }
    }
}
