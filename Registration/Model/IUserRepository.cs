using Microsoft.EntityFrameworkCore;
using Registration.Model;
using System.Data;

namespace Registration.Controllers
{
    public interface IUserRepository
    {
        public Task CreateUserAsync(User user);
        public Task DeleteUserAsync(User user);
        public Task UpdateUserAsync(User user);
        public Task<User> GetUserByUsernameAsync(string username);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateUserAsync(User user)
        {
            if(await GetUserByUsernameAsync(user.Username) != null)
            {
                throw new DuplicateNameException("User with the same username already exists");
            }
            try
            {
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to add user", ex);
            }
        }
        public async Task DeleteUserAsync(User user)
        {
            var existingUser = await GetUserByUsernameAsync(user.Username);

            try
            {
                _dbContext.Remove(existingUser);
                _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to delete user", ex);
            }
        }
        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await GetUserByUsernameAsync(user.Username) ??
                throw new Exception("User not found");

            try
            {
                existingUser.Username = user.Username;
                existingUser.Password = user.Password;
                existingUser.Email = user.Email;
                _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Failed to update user", ex);
            }
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                var users = await _dbContext.Users.Where(u => u.Username.Equals(username)).ToListAsync();
                return users.FirstOrDefault(u => u.Username == username);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error getting user by username '{username}'", ex);
            }
        }
    }


}