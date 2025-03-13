using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wulkanizacja.Auth.Mapping;
using Wulkanizacja.Auth.Models;
using Wulkanizacja.Auth.PostgreSQL.Entities;
using Wulkanizacja.Auth.PostgreSQL.Repositories;

namespace Wulkanizacja.Auth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly CancellationToken _cancellationToken;

        public AuthController(IConfiguration configuration, IUserRepository userRepository, CancellationToken cancellationToken = default)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _cancellationToken = cancellationToken;
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Podano błędne dane"));
            }

           var userRecord = login.ToUserRecord();
            try
            {
                var tryLogin = await _userRepository.Login(userRecord, _cancellationToken);
            }
            catch(Exception e)
            {
                return Conflict(new ApiResponse(409, e.Message));
            }



            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, login.Username)
        };

            // Pobranie ustawień z konfiguracji
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiresInHours"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            // Zwrócenie tokenu
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Podano błędne dane"));
            }

            var userRecord = model.ToUserRecord();

            // Weryfikacja czy użytkownik już istnieje
            var existingUser = await _userRepository.GetUserByUsername(userRecord.Username, _cancellationToken);
            if (existingUser != null)
            {
                return Conflict(new ApiResponse(409, "Użytkownik o takim loginie już istnieje."));
            }

            try
            {
                await _userRepository.Register(userRecord, _cancellationToken);
            }
            catch(Exception e)
            {
                return Conflict(new ApiResponse(409, e.Message));
            }
           
            return Ok(new ApiResponse(200, "Rejestracja przebiegła pomyślnie."));
        }
    }
}
