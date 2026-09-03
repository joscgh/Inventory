using Inventory.API.Services;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ICustomerAccountUserService _userService;
        private readonly ICustomerAccountService _accountService;

        public AuthController(
            ICustomerAccountUserService userService,
            ICustomerAccountService accountService)
        {
            _userService = userService;
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized("Usuario o contraseña incorrecta.");
            }

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Usuario o contraseña incorrecta.");
            }

            return Ok(new LoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AccountId = user.CustomerAccountId,
                AccountName = user.Account?.Name ?? string.Empty,
                AccountEmail = user.Account?.Email ?? string.Empty
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountName) || string.IsNullOrWhiteSpace(request.AccountEmail)
                || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            var account = new CustomerAccount
            {
                Name = request.AccountName,
                Document = request.AccountDocument,
                Address = request.AccountAddress,
                Email = request.AccountEmail,
                Phone = request.AccountPhone
            };

            var accountCreated = await _accountService.RegisterAccountAsync(account);
            if (!accountCreated)
            {
                return BadRequest("No se pudo crear la cuenta. Verifica que el correo de la cuenta no exista y los datos sean válidos.");
            }

            var user = new CustomerAccountUser
            {
                FullName = request.FullName,
                Email = request.Email,
                Role = request.Role,
                CustomerAccountId = account.Id
            };

            var userAssigned = await _userService.AssignUserToAccountAsync(account.Id, user, request.Password);
            if (!userAssigned)
            {
                return BadRequest("No se pudo crear el usuario para la cuenta. Verifica que el correo no exista y los datos sean válidos.");
            }

            return Ok(new LoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AccountId = account.Id,
                AccountName = account.Name,
                AccountEmail = account.Email
            });
        }
    }
}
