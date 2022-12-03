using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaffsController : ControllerBase
    {
        private readonly CaffContext context;

        public CaffsController(CaffContext context)
        {
            this.context = context;
        }

        // GET: api/caffs
        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<CaffDTO>> GetWithFilter(string? searchBy = null)
        {
            if (string.IsNullOrEmpty(searchBy))
            {
                return Ok(context.Caffs.Include(c => c.Uploader)
                                       .Include(c => c.Comments)
                                       .ThenInclude(c => c.User)
                                       .Where(c => c.IsActive)
                                       .Select(c => Mapper.ToCaffDTO(c)));
            }
            try
            {
                return Ok(context.Caffs.Include(c => c.Uploader)
                                   .Include(c => c.Comments)
                                   .ThenInclude(c => c.User)
                                   .Where($"IsActive == true && {searchBy}")
                                   .Select(c => Mapper.ToCaffDTO(c)));
            }
            catch (ParseException)
            {
                return BadRequest("Invalid searchBy term!");
            }
        }

        // GET api/caffs/{id}
        [HttpGet("{id}")]
		[Authorize]
		public ActionResult<CaffDTO> Get(string id)
        {
            try
            {
                var caff = context.Caffs.Include(c => c.Uploader)
                                    .Include(c => c.Comments)
                                    .ThenInclude(c => c.User)
                                    .Where(c => c.IsActive)
                                    .FirstOrDefault(c => c.Id == Mapper.GetCaffId(id));
                if (caff == null)
                {
                    return NotFound($"No CAFF file was found with the id {id}!");
                }
                return Ok(Mapper.ToCaffDTO(caff));
            }
            catch (Exception)
            {
                return NotFound($"No CAFF file was found with the id {id}!");
            }
        }

        // GET api/caffs/{id}/download
        [HttpGet("{id}/download")]
		[Authorize]
		public ActionResult Download(string id)
        {
            try
            {
                var caff = context.Caffs.Where(c => c.IsActive)
                                    .Include(c => c.Uploader)
                                    .FirstOrDefault(c => c.Id == Mapper.GetCaffId(id));
                if (caff == null)
                {
                    return NotFound($"No CAFF file was found with the id {id}!");
                }
                var current = Directory.GetCurrentDirectory();
                var path = Path.Combine(current, "Files", $"{caff.FilePathWithoutExtension}.caff");
                return PhysicalFile(path, "application/caff", $"{caff.Name}.caff");
            }
            catch (Exception)
            {
                return NotFound($"No CAFF file was found with the id {id}!");
            }
        }

        // GET api/caffs/{id}/preview
        [HttpGet("{id}/preview")]
		[Authorize]
		public ActionResult Preview(string id)
        {
            try
            {
                var caff = context.Caffs.Where(c => c.IsActive)
                                    .Include(c => c.Uploader)
                                    .FirstOrDefault(c => c.Id == Mapper.GetCaffId(id));
                if (caff == null)
                {
                    return NotFound($"No CAFF file was found with the id {id}!");
                }
                var current = Directory.GetCurrentDirectory();
                var path = Path.Combine(current, "Previews", $"{caff.FilePathWithoutExtension}.gif");
                return PhysicalFile(path, "application/gif", $"{(caff.Name)}.gif");
            }
            catch (Exception)
            {
                return NotFound($"No CAFF file was found with the id {id}!");
            }
        }

        // POST api/caffs
        [HttpPost]
		//[Authorize]
		public ActionResult Post([FromForm] IFormFile file, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(Request.GetMultipartBoundary()))
            {
                return BadRequest("Bad content type!");
            }
            var extension = file.FileName.Split('.').Last().ToLower();
            if (extension != "caff")
            {
                return BadRequest("Bad file extension!");
            }
            User? user = default;
            try
            {
                user = context.Users.Where(u => u.IsActive)
                                    .FirstOrDefault(u => u.Id == Mapper.GetUserId(userId));
            }
            catch (Exception)
            {
                return NotFound($"No User was found with the id {userId}!");
            }
            if (user == null)
            {
                return NotFound($"No User was found with the id {userId}!");
            }
            try
            {
                var filePath = FileManager.SaveFile(file, user);
				var dotIndex = filePath.LastIndexOf('.');
				var withoutExtension = filePath[..dotIndex];
				var caff = new Caff
                {
                    Name = Path.GetFileNameWithoutExtension(file.FileName),
                    FilePathWithoutExtension = withoutExtension,
                    Uploader = user
                };
                context.Add(caff);
                context.SaveChanges();
                return CreatedAtAction("Get", new { id = Mapper.GetCaffHash(caff.Id) }, Mapper.ToCaffDTO(caff));
            }
            catch (CaffException ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        // PATCH api/caffs/{id}
        [HttpPatch("{id}")]
		[Authorize(Roles = "Admin")]
		public ActionResult Patch(string id, [FromBody] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("The name must be at least 1 character long!");
            }
            try
            {
                var caff = context.Caffs.Where(c => c.IsActive)
                                    .FirstOrDefault(c => c.Id == Mapper.GetCaffId(id));
                if (caff == null)
                {
                    return NotFound($"No CAFF file was found with the id {id}!");
                }
                caff.Name = name;
                context.SaveChanges();
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound($"No CAFF file was found with the id {id}!");
            }
        }

        // DELETE api/caffs/{id}
        [HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public ActionResult Delete(string id)
        {
            try
            {
                var caff = context.Caffs.Where(c => c.IsActive)
                                    .FirstOrDefault(c => c.Id == Mapper.GetCaffId(id));
                if (caff == null)
                {
                    return NotFound($"No CAFF file was found with the id {id}!");
                }
                caff.IsActive = false;
                context.SaveChanges();
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound($"No CAFF file was found with the id {id}!");
            }
        }
    }
}
