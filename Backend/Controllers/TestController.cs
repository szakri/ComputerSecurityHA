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

        public TestController(CaffContext context)
        {
            this.context = context;
        }

		/// <summary>
		/// Initializes the DB for testing (only available in debug mode)
		/// </summary>
		/// <response code="200">Successful initialization</response>
		// GET /test/initdb
		[HttpGet("initdb")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
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

		/// <summary>
		/// Emptys all the tables (only available in debug mode)
		/// </summary>
		/// <response code="200">Successful reset</response>
		// GET /test/reset
		[HttpGet("reset")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public void Reset()
        {
            var caffs = context.Caffs.ToArray();
            context.Caffs.RemoveRange(caffs);
            context.Comments.RemoveRange(context.Comments.ToArray());
            context.Users.RemoveRange(context.Users.ToArray());
            context.SaveChanges();
        }
    }
}
