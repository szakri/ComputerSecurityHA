using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly CaffContext context;
        private readonly IConfiguration configuration;

        public TestController(CaffContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpGet("initDB")]
        public void Init()
        {
            if (((RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>()).Exists())
            {
                context.Database.EnsureDeleted();
            }
            context.Database.EnsureCreated();
            List<User> users = new()
            {
                new User
                {
                    Username = "Admin",
                    Password = "admin1234"
                },
                new User
                {
                    Username = "TesztElek",
                    Password = "test1234"
                }
            };
            context.AddRange(users);

            List<Caff> caffs = new()
            {
                new Caff
                {
                    Name = "1",
                    FilePathWithoutExtension = "kPewzmLpJqED0y7N\\1_2022.11.22.17-56-44-9-95",
                    Uploader = users[1],
                    Comments = new List<Comment>
                    {
                        new Comment
                        {
                            User = users[1],
                            Text = "First"
                        }
                    }
                },
                new Caff
                {
                    Name = "2",
                    FilePathWithoutExtension = "kPewzmLpJqED0y7N\\2_2022.11.22.17-57-01-6-67",
                    Uploader = users[1]
                },
                new Caff
                {
                    Name = "3",
                    FilePathWithoutExtension = "kPewzmLpJqED0y7N\\3_2022.11.22.17-57-11-2-27",
                    Uploader = users[1]
                }
            };
            context.AddRange(caffs);

            context.SaveChanges();
        }

        [HttpGet("reset")]
        public void Reset()
        {
            var caffs = context.Caffs.ToArray();
            context.Caffs.RemoveRange(caffs);
            context.Comments.RemoveRange(context.Comments.ToArray());
            context.Users.RemoveRange(context.Users.ToArray());
            context.SaveChanges();
        }

        [HttpGet("login")]
        public ActionResult<LoginDTO> Login(string username, string password)
        {
            var user = context.Users.FirstOrDefault(u => u.Username== username && u.Password == password);
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

        [Authorize]
        [HttpGet("authorized")]
        public ActionResult Authorized()
        {
            return Ok("Authorized");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("adminauthorized")]
        public ActionResult AdminAuthorized()
        {
            return Ok("Authorized");
        }
    }
}
