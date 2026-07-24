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

        public CustomerAccountsController(
            ICustomerAccountService service,
            ICustomerAccountUserService userService)
        {
            _service = service;
            _userService = userService;
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
