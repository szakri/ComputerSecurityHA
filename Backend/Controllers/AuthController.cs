using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly CaffContext context;
		private readonly IConfiguration configuration;

		public AuthController(CaffContext context, IConfiguration configuration)
		{
			this.context = context;
			this.configuration = configuration;
		}

		/// <summary>
		/// Logs in with the user
		/// </summary>
		/// <param name="username">The user's username</param>
		/// <param name="password">The user's password</param>
		/// <returns></returns>
		/// <response code="200">The login was successful</response>
		/// <response code="404">If no registeres user was found</response>
		// GET: api/auth/login
		[HttpGet("login")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<LoginDTO> Login(string username, string password)
		{
			var user = context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
			if (user == null)
			{
				return NotFound("There is no registered user with these parameters!");
			}
			var issuer = configuration["Jwt:Issuer"];
			var audience = configuration["Jwt:Audience"];
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			List<Claim> claims = new();
			if (username == "Admin")
			{
				claims.Add(new Claim(ClaimTypes.Role, "Admin"));
			}
			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				signingCredentials: credentials,
				expires: DateTime.UtcNow.AddMinutes(30));

			var tokenHandler = new JwtSecurityTokenHandler();
			var stringToken = tokenHandler.WriteToken(token);

			return Ok(new LoginDTO
			{
				Token = stringToken,
				UserId = Mapper.GetUserHash(user.Id)
			});
		}
	}
}
