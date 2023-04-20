using Microsoft.AspNetCore.Mvc;
using Registration.Model;

namespace Registration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("{userName}")]
        public IActionResult GetUserByUsername(string username)
        {
            var user = _userRepository.GetUserByUsername(username);
            if(user == null) 
            {
                return NotFound();
            }

            user.Password = null;
            return Ok(user);
        }

        [HttpPost]
        public IActionResult AddUser([FromBody]User user)
        {
            if(string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Username and password are required");
            }

            if(_userRepository.GetUserByUsername(user.Username) != null)
            {
                return Conflict("Username already exists");
            }


            user.Password = HashPassword(user.Password);

            _userRepository.AddUser(user);
            return CreatedAtAction(nameof(GetUserByUsername), new { username = user.Username }, user);
        }


        [HttpPost("{username}/login")]
        public IActionResult Login(string username, [FromBody] string password) 
        {
            var user = _userRepository.GetUserByUsername(username);

            if(user == null)
            {
                return NotFound();
            }

            if (user.IsLockedOut)
            {
                return BadRequest("Account is locked out");
            }

            if (VerifyPassword(password, user.Password))
            {
                user.FailedLoginAttempts = 0;
                _userRepository.UpdateUser(user);
                return Ok("Login successful");
            }
            else
            {
                user.FailedLoginAttempts++;
                if(user.FailedLoginAttempts >= 3) 
                { 
                    user.IsLockedOut = true;
                }

                _userRepository.UpdateUser(user);
                return BadRequest("Invalid password");
            }
        }
    }

}