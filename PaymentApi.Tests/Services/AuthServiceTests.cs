using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using PaymentApi.Models;
using PaymentApi.Repositories;
using PaymentApi.Services;
using Xunit;

namespace PaymentApi.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<IConfiguration> _config = new();

        private AuthService CreateService()
        {
            _config.Setup(c => c["Jwt:Key"]).Returns("TestJwtKeyForUnitTestsOnly_ChangeInRealApp");
            return new AuthService(_users.Object, _config.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_Throws_WhenUserNotFound()
        {
            _users.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync((User?)null);

            var svc = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.AuthenticateAsync("alice", "password"));
        }

        [Fact]
        public async Task AuthenticateAsync_Throws_WhenPasswordInvalid()
        {
            var user = new User
            {
                Username = "alice",
                PasswordHash = AuthService.HashPassword("other-password")
            };
            _users.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(user);

            var svc = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                svc.AuthenticateAsync("alice", "password"));
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsTokens_WhenCredentialsValid()
        {
            var plainPassword = "Password123!";
            var user = new User
            {
                Id = 1,
                Username = "alice",
                PasswordHash = AuthService.HashPassword(plainPassword)
            };
            _users.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(user);
            _users.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var svc = CreateService();

            var tokens = await svc.AuthenticateAsync("alice", plainPassword);

            Assert.False(string.IsNullOrWhiteSpace(tokens.accessToken));
            Assert.False(string.IsNullOrWhiteSpace(tokens.refreshToken));

            _users.Verify(r => r.UpdateAsync(It.Is<User>(u => u.RefreshToken != null)), Times.Once);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenRefreshTokenUnknown()
        {
            _users.Setup(r => r.GetByRefreshTokenAsync("bad-token")).ReturnsAsync((User?)null);

            var svc = CreateService();

            var result = await svc.RefreshTokensAsync("bad-token");

            Assert.Null(result);
        }
    }
}
