using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq;
using System.Linq.Dynamic.Core;

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
            return Ok(context.Caffs.Include(c => c.Uploader)
                                          .Include(c => c.Comments)
                                          .ThenInclude(c => c.User)
                                          .Where($"c => c.IsActive && {searchBy}")
                                          .Select(c => Mapper.ToCaffDTO(c)));
        }

        // GET api/caffs/{id}
        [HttpGet("{id}")]
        public ActionResult<CaffDTO> Get(string id)
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

        // GET api/caffs/{id}/download
        [HttpGet("{id}/download")]
        public ActionResult Download(string id)
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

        // GET api/caffs/{id}/preview
        [HttpGet("{id}/preview")]
        public ActionResult Preview(string id)
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

        // POST api/caffs
        [HttpPost]
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
            var user = context.Users.Where(u => u.IsActive)
                                    .FirstOrDefault(u => u.Id == Mapper.GetUserId(userId));
            if (user == null)
            {
                return NotFound($"No User was found with the id {userId}!");
            }
            var filePath = FileManager.SaveFile(file, user);
            var caff = new Caff
            {
                Name = file.FileName,
                FilePathWithoutExtension = filePath,
                Uploader = user
            };
            context.Add(caff);
            context.SaveChanges();
            return CreatedAtAction("Get", new { id = Mapper.GetCaffHash(caff.Id) }, Mapper.ToCaffDTO(caff));
        }

        // PATCH api/caffs/{id}
        [HttpPatch("{id}")]
        public ActionResult Patch(string id, [FromBody] string name)
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

        // DELETE api/caffs/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
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
    }
}
