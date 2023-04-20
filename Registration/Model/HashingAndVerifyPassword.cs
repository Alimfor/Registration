using System.Security.Cryptography;
using System.Text;

namespace Registration.Controllers
{
    public partial class UserController
    {
        private string HashPassword(string password)
        {
            // Хэширование пароля с использованием алгоритма SHA256
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashParts = passwordHash.Split(':');
            if (hashParts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(hashParts[0]);
            var hashBytes = Convert.FromBase64String(hashParts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            var testHashBytes = pbkdf2.GetBytes(hashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(testHashBytes, hashBytes);
        }
    }
}
