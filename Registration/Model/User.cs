using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Registration.Model
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLockedOut { get; set; }
    }
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
