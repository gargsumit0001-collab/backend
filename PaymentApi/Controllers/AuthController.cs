using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Models;
using PaymentApi.Services;
using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _svc;
        public AuthController(IAuthService svc) { _svc = svc; }

        public record LoginDto([Required] string username, [Required] string password);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var tokens = await _svc.AuthenticateAsync(dto.username, dto.password);
                Response.Cookies.Append("accessToken", tokens.accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    Path = "/"
                });
                
                Response.Cookies.Append("refreshToken", tokens.refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7),
                    Path = "/"
                });
                
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var tokens = await _svc.RefreshTokensAsync(refreshToken);
            if (tokens == null)
                return Unauthorized();
            
            Response.Cookies.Append("accessToken", tokens.Value.accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Path = "/"
            });

            Response.Cookies.Append("refreshToken", tokens.Value.refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            });

            return Ok(new { success = true });
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var ok = await _svc.RevokeRefreshTokenAsync(refreshToken);
            if (!ok) return NotFound();
            
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return NoContent();
        }
        
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var username = User.Identity?.Name;
            var userId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;

            return Ok(new
            {
                username,
                userId
            });
        }
    }
}
