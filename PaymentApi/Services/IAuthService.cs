using PaymentApi.Models;

namespace PaymentApi.Services
{
    public interface IAuthService
    {
        Task<(string accessToken, string refreshToken)> AuthenticateAsync(string username, string password);
        Task<(string accessToken, string refreshToken)?> RefreshTokensAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        string GenerateAccessToken(User user);
    }
}
