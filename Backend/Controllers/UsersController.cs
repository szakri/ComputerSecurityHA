using Backend.Services;
using DAL;
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
        public ActionResult<IEnumerable<UserDTO>> Get()
        {
            return Ok(context.Users.Where(u => u.IsActive)
                                   .Select(u => Mapper.ToUserDTO(u))
                                   .ToList());
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public ActionResult<UserDTO> Get(string id)
        {
            var user = context.Users.Where(u => u.IsActive)
                                    .FirstOrDefault(u => u.Id == Mapper.GetUserId(id));
            if (user == null)
            {
                return NotFound($"No User was found with the id {id}!");
            }
            return Mapper.ToUserDTO(user);
        }

        // POST api/users
        [HttpPost]
        public ActionResult Post([FromBody] RegisterDTO user)
        {
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
        public ActionResult Delete(string id)
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
    }
}
