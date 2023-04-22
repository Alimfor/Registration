using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Registration.Model;
using Serilog;

namespace Registration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> loggerFactory)
        {
            _userRepository = userRepository;
            _logger = loggerFactory;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound();
                }
                user.Password = "";
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by username '{username}'");
                return StatusCode(500, $"Error getting user by username '{username}': {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync(UserAddModel user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Username and password are required");
            }

            var existingUser = await _userRepository.GetUserByUsernameAsync(user.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists");
            }

            user.Password = HashPassword(user.Password);

            try
            {
                await _userRepository.CreateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add user: {ex.Message}");
                return BadRequest($"Failed to add user: {ex.Message}");
                
            }
            return Ok(user);
            //return CreatedAtAction(nameof(AddUserAsync), new { username = user.Username }, user);
        }

        [HttpPost("{username}/login")]
        public async Task<IActionResult> Login(string username, string password) 
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest("Username is empty or null");
                }
                var user = await _userRepository.GetUserByUsernameAsync(username);

                if (user.IsLockedOut)
                {
                    return BadRequest("Account is locked out");
                }

                if (VerifyPassword(password, user.Password))
                {
                    user.FailedLoginAttempts = 0;
                    await _userRepository.UpdateUserAsync(user);
                    return Ok("Login successful");
                }
                else
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= 3)
                    {
                        user.IsLockedOut = true;
                    }

                    await _userRepository.UpdateUserAsync(user);
                    return BadRequest("Invalid password");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login method.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLogin(string username,string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest("Username is empty or null");
                }
                var user = await _userRepository.GetUserByUsernameAsync(username);

                if (VerifyPassword(password, user.Password))
                {
                    user.FailedLoginAttempts = 0;
                    await _userRepository.DeleteUserAsync(user);
                    return Ok("Delete successful");
                }
                else
                {
                    user.FailedLoginAttempts++;
                    if (user.FailedLoginAttempts >= 3)
                    {
                        user.IsLockedOut = true;
                    }
                    await _userRepository.UpdateUserAsync(user);
                    return BadRequest("Invalid password");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login method.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

    }

}