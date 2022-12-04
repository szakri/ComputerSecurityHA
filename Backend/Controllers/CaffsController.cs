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

		/// <summary>
		/// Returns all the CAFF files' information that match the searchBy term
		/// (if it's empty all CAFF files' information returned)
		/// </summary>
		/// <param name="searchBy">Dynamic LINQ search term</param>
		/// <returns></returns>
		/// <response code="200">Returns the CAFF files' information</response>
		// GET: api/caffs
		[Authorize]
		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
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

		/// <summary>
		/// Returns the CAFF file's information
		/// </summary>
		/// <param name="id">CAFF file's id</param>
		/// <returns></returns>
		/// <response code="200">Returns the CAFF file's id</response>
		/// <response code="404">If no CAFF file was found with the supplied id</response>
		// GET api/caffs/{id}
		[Authorize]
		[HttpGet("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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

		/// <summary>
		/// Returns the CAFF file
		/// </summary>
		/// <param name="id">CAFF file's id</param>
		/// <returns></returns>
		/// <response code="200">Returns the CAFF files</response>
		/// <response code="404">If no CAFF file was found with the supplied id</response>
		// GET api/caffs/{id}/download
		[Authorize]
		[HttpGet("{id}/download")]
		[Produces("application/caff")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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

		/// <summary>
		/// Returns the parsed CAFF file
		/// </summary>
		/// <param name="id">CAFF file's id</param>
		/// <returns></returns>
		/// <response code="200">Returns the parsed CAFF files</response>
		/// <response code="404">If no CAFF file was found with the supplied id</response>
		// GET api/caffs/{id}/preview
		[Authorize]
		[HttpGet("{id}/preview")]
		[Produces("application/gif")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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

		/// <summary>
		/// Uploads a CAFF file
		/// </summary>
		/// <param name="file">The CAFF file</param>
		/// <param name="userId">The id of the uploader</param>
		/// <returns></returns>
		/// <response code="201">Successful upload</response>
		/// <response code="400">If the content type is not multipart boundry or
        /// the file extension is worng or the CAFF file is not valid</response>
		/// <response code="404">If no user was found with the supplied id</response>
		// POST api/caffs
		[Authorize]
		[HttpPost]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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

		/// <summary>
		/// Changes the name of the CAFF file
		/// </summary>
		/// <param name="id">CAFF file's id</param>
		/// <param name="name">File's new name</param>
		/// <returns></returns>
		/// <response code="204">The change was successful</response>
		/// <response code="400">If the new name is less than 1 character</response>
		/// <response code="404">If no CAFF file was found with the supplied id</response>
		// PATCH api/caffs/{id}
		[Authorize(Roles = "Admin")]
		[HttpPatch("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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

		/// <summary>
		/// Deletes a CAFF file
		/// </summary>
		/// <param name="id">CAFF file's id</param>
		/// <returns></returns>
		/// <response code="204">The deletion was successful</response>
		/// <response code="404">If no CAFF file was found with the supplied id</response>
		// DELETE api/caffs/{id}
		[Authorize(Roles = "Admin")]
		[HttpDelete("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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
