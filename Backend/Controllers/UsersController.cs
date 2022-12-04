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

		/// <summary>
		/// Returns all the registered users
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Returns all the registered users</response>
		// GET: api/users
		[Authorize]
		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public ActionResult<IEnumerable<UserDTO>> Get()
        {
            return Ok(context.Users.Where(u => u.IsActive)
                                   .Select(u => Mapper.ToUserDTO(u))
                                   .ToList());
        }

		/// <summary>
		/// Returns the user
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <response code="200">Returns the user</response>
		/// <response code="404">If no user was found with the supplied id</response>
		// GET api/users/{id}
		[Authorize]
		[HttpGet("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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

		/// <summary>
		/// Registers a new user
		/// </summary>
		/// <param name="user">The new user's username and password</param>
		/// <returns></returns>
		/// <response code="201">The user was added successfuly</response>
		/// <response code="400">If the user's username or password or the username contains whitespaces or already is in use</response>
		// POST api/users
		[HttpPost]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
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

		/// <summary>
		/// Deletes the user
		/// </summary>
		/// <param name="id">the user's id</param>
		/// <returns></returns>
		/// <response code="204">The deletion was successful</response>
		/// <response code="404">If no user was found with the supplied id</response>
		// DELETE api/users/{id}
		[Authorize(Roles = "Admin")]
		[HttpDelete("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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
