using Inventory.API.Services;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAccountsController : ControllerBase
    {
        private readonly ICustomerAccountService _service;
        private readonly ICustomerAccountUserService _userService;
        private readonly IAccountLocationService _locationService;
        private readonly IAccountLogoService _logoService;

        public CustomerAccountsController(
            ICustomerAccountService service,
            ICustomerAccountUserService userService,
            IAccountLocationService locationService,
            IAccountLogoService logoService)
        {
            _service = service;
            _userService = userService;
            _locationService = locationService;
            _logoService = logoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerAccount>>> Get()
        {
            var accounts = await _service.ListAccountsAsync();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerAccount>> GetById(int id)
        {
            var account = await _service.FindByIdAsync(id);
            if (account == null) return NotFound();
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerAccount account)
        {
            var created = await _service.RegisterAccountAsync(account);
            if (!created) return BadRequest("No se pudo registrar la cuenta. Verifica que el nombre y el correo sean válidos y no exista otra cuenta con el mismo correo.");
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
        }

        [HttpPost("{accountId}/users")]
        public async Task<IActionResult> AddUser(int accountId, [FromBody] AddAccountUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Nombre, correo y contraseña son obligatorios.");
            }

            var user = new CustomerAccountUser
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = request.Role,
                CustomerAccountId = accountId
            };

            var created = await _userService.AssignUserToAccountAsync(accountId, user, request.Password);
            if (!created)
            {
                return BadRequest("No se pudo agregar el usuario. Verifica que el correo no exista y los datos sean válidos.");
            }

            return Ok(user);
        }

        [HttpGet("{accountId}/locations")]
        public async Task<ActionResult<IEnumerable<AccountLocation>>> GetLocations(int accountId)
        {
            var locations = await _locationService.ListByAccountAsync(accountId);
            return Ok(locations);
        }

        [HttpPost("{accountId}/locations")]
        public async Task<IActionResult> AddLocation(int accountId, [FromBody] AccountLocation location)
        {
            var (created, error) = await _locationService.RegisterLocationAsync(accountId, location);
            if (!created) return BadRequest(error);
            return Ok(location);
        }

        [HttpPut("{accountId}/locations/{locationId}")]
        public async Task<IActionResult> UpdateLocation(int accountId, int locationId, [FromBody] AccountLocation location)
        {
            var (updated, error) = await _locationService.ModifyLocationAsync(accountId, locationId, location);
            if (!updated) return BadRequest(error);
            return NoContent();
        }

        [HttpDelete("{accountId}/locations/{locationId}")]
        public async Task<IActionResult> DeleteLocation(int accountId, int locationId)
        {
            var deleted = await _locationService.RemoveLocationAsync(accountId, locationId);
            if (!deleted) return NotFound("No se encontró el depósito o tienda para eliminar.");
            return NoContent();
        }

        [HttpGet("{accountId}/logo")]
        public async Task<IActionResult> GetLogo(int accountId)
        {
            var logo = await _logoService.FindByAccountAsync(accountId);
            if (logo == null) return NotFound();

            // nosniff para que el navegador no reinterprete el contenido como otra cosa.
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(logo.Data, logo.ContentType);
        }

        [HttpPost("{accountId}/logo")]
        [RequestSizeLimit(4 * 1024 * 1024)]
        public async Task<IActionResult> UploadLogo(int accountId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Debes seleccionar un archivo de imagen.");

            if (file.Length > IAccountLogoService.MaxSizeBytes)
                return BadRequest("La imagen no puede superar los 2 MB.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var (saved, error) = await _logoService.SaveLogoAsync(accountId, stream.ToArray(), file.FileName);
            if (!saved) return BadRequest(error);
            return NoContent();
        }

        [HttpDelete("{accountId}/logo")]
        public async Task<IActionResult> DeleteLogo(int accountId)
        {
            var deleted = await _logoService.RemoveLogoAsync(accountId);
            if (!deleted) return NotFound("La cuenta no tiene logo.");
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CustomerAccount account)
        {
            var updated = await _service.ModifyAccountAsync(account);
            if (!updated) return NotFound("No se encontró la cuenta para actualizar.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.RemoveAccountAsync(id);
            if (!deleted) return NotFound("No se encontró la cuenta para eliminar.");
            return NoContent();
        }
    }
}
