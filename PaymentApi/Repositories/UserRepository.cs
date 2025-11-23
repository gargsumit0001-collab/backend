using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Models;

namespace PaymentApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PaymentsDbContext _db;
        public UserRepository(PaymentsDbContext db) { _db = db; }

        public async Task<User?> GetByUsernameAsync(string username)
            => await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
            => await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
