using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu đăng ký không hợp lệ." });
            }

            var user = await _userService.RegisterAsync(dto.Username, dto.Password);
            if (user == null)
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            }

            return Ok(new { message = "Đăng ký thành công!", username = user.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không đúng định dạng." });
            }

            var user = await _userService.AuthenticateAsync(dto.Username, dto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForNightShiftAsylumGame2026Project";
            var key = Encoding.ASCII.GetBytes(secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Add role claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "1440")),
                Issuer = _configuration["JwtSettings:Issuer"] ?? "NightShiftAsylumBackend",
                Audience = _configuration["JwtSettings:Audience"] ?? "NightShiftAsylumClient",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
