using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CaffContext context;

        public UsersController(CaffContext context)
        {
            this.context = context;
        }

        // GET: api/users
        [HttpGet]
		[Authorize]
		public ActionResult<IEnumerable<UserDTO>> Get()
        {
            return Ok(context.Users.Where(u => u.IsActive)
                                   .Select(u => Mapper.ToUserDTO(u))
                                   .ToList());
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
		[Authorize]
		public ActionResult<UserDTO> Get(string id)
        {
            try
            {
                var user = context.Users.Where(u => u.IsActive)
                                    .FirstOrDefault(u => u.Id == Mapper.GetUserId(id));
                if (user == null)
                {
                    return NotFound($"No User was found with the id {id}!");
                }
                return Mapper.ToUserDTO(user);
            }
            catch (Exception)
            {
                return NotFound($"No User was found with the id {id}!");
            }
        }

        // POST api/users
        [HttpPost]
        public ActionResult Post([FromBody] RegisterDTO user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return BadRequest("The Username must not be empty!");
            }
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("The Password must not be empty!");
            }
            if (user.Username.Any(x => char.IsWhiteSpace(x)))
            {
                return BadRequest("The Username must not contain whitespaces!");
            }
            if (context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("The Username is already in use!");
            }
            var newUser = new User
            {
                Username = user.Username,
                Password = user.Password
            };
            context.Add(newUser);
            context.SaveChanges();
            return CreatedAtAction("Get", new { id = Mapper.GetUserHash(newUser.Id) }, Mapper.ToUserDTO(newUser));
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public ActionResult Delete(string id)
        {
            try
            {
                var user = context.Users.Where(u => u.IsActive)
                                    .FirstOrDefault(u => u.Id == Mapper.GetUserId(id));
                if (user == null)
                {
                    return NotFound($"No User was found with the id {id}!");
                }
                user.IsActive = false;
                context.SaveChanges();
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound($"No User was found with the id {id}!");
            }
            
        }
    }
}
