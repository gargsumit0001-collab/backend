using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PaymentApi.Models;
using PaymentApi.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PaymentApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;
        public AuthService(IUserRepository users, IConfiguration config)
        {
            _users = users;
            _config = config;
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string hash, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public async Task<(string accessToken, string refreshToken)> AuthenticateAsync(string username, string password)
        {
            var user = await _users.GetByUsernameAsync(username);
            if (user == null) throw new UnauthorizedAccessException("Invalid credentials");
            if (!VerifyPassword(user.PasswordHash, password)) throw new UnauthorizedAccessException("Invalid credentials");

            var access = GenerateAccessToken(user);
            var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.RefreshToken = refresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _users.UpdateAsync(user);
            return (access, refresh);
        }

        public async Task<(string accessToken, string refreshToken)?> RefreshTokensAsync(string refreshToken)
        {
            var user = await _users.GetByRefreshTokenAsync(refreshToken);
            if (user == null) return null;
            if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow) return null;
            var access = GenerateAccessToken(user);
            var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.RefreshToken = refresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _users.UpdateAsync(user);
            return (access, refresh);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var user = await _users.GetByRefreshTokenAsync(refreshToken);
            if (user == null) return false;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _users.UpdateAsync(user);
            return true;
        }

        public string GenerateAccessToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? "ReplaceWithVeryLongSecretKeyForDevOnly!ChangeInProd";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("uid", user.Id.ToString())
            };
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
