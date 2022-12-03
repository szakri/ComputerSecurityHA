using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CaffContext context;

        public CommentsController(CaffContext context)
        {
            this.context = context;
        }

		/// <summary>
		/// Returns all the comments
		/// </summary>
		/// <returns></returns>
		/// <response code="200">Returns all the comments</response>
		// GET: api/comments
		[Authorize]
		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public ActionResult<IEnumerable<CommentDTO>> Get()
        {
            return Ok(context.Comments.Include(c => c.User)
                                      .Where(c => c.IsActive)
                                      .Select(c => Mapper.ToCommentDTO(c)));
        }

		/// <summary>
		/// Returns the comment
		/// </summary>
		/// <param name="id">The comment's id</param>
		/// <returns></returns>
		/// <response code="200">Returns the comment</response>
		/// <response code="404">If no comment was found with the supplied id</response>
		// GET api/comments/{id}
		[Authorize]
		[HttpGet("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<CommentDTO> Get(string id)
        {
            try
            {
                var comment = context.Comments.Include(c => c.User)
                                          .Where(c => c.IsActive)
                                          .FirstOrDefault(c => c.Id == Mapper.GetCommentId(id));
                if (comment == null)
                {
                    return NotFound($"No Comment was found with the id {id}!");
                }
                return Ok(Mapper.ToCommentDTO(comment));
            }
            catch (Exception)
            {
                return NotFound($"No Comment was found with the id {id}!");
            }
        }

		/// <summary>
		/// Adds a new comment to a CAFF file
		/// </summary>
		/// <param name="newComment">New comment with the commenter's id, CAFF file's id and the comment text</param>
		/// <returns></returns>
		/// <response code="201">The comment was added successfuly</response>
		/// <response code="400">If the comment text was less than 1 character long</response>
		/// <response code="404">If no user or CAFF file was found with the supplied id</response>
		// POST api/comments
		[Authorize]
		[HttpPost]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<CommentDTO> Post([FromBody] NewCommentDTO newComment)
        {
            if (string.IsNullOrEmpty(newComment.CommentText))
            {
                return BadRequest("The commentText must be at least 1 character long!");
            }
            Caff? caff = default;
            try
            {
                caff = context.Caffs.Where(c => c.IsActive)
                                    .FirstOrDefault(c => c.Id == Mapper.GetCaffId(newComment.CaffId));
            }
            catch (Exception)
            {
                return NotFound($"No CAFF file was found with the id {newComment.CaffId}!");
            }
            if (caff == null)
            {
                return NotFound($"No CAFF file was found with the id {newComment.CaffId}!");
            }
            User? user = default;
            try
            {
                user = context.Users.Where(u => u.IsActive)
                                    .FirstOrDefault(u => u.Id == Mapper.GetUserId(newComment.UserId));
            }
            catch (Exception)
            {
                return NotFound($"No User was found with the id {newComment.UserId}!");
            }
            
            if (user == null)
            {
                return NotFound($"No User was found with the id {newComment.UserId}!");
            }
            var comment = new Comment
            {
                User = user,
                Text = newComment.CommentText
            };
            if (caff.Comments == null)
            {
                caff.Comments = new List<Comment> { comment };
            }
            else
            {
                caff.Comments.Add(comment);
            }
            context.SaveChanges();
            return CreatedAtAction("Get", new { id = Mapper.GetCommentHash(comment.Id) }, Mapper.ToCommentDTO(comment));
        }

		/// <summary>
		/// Changes the text of the comment
		/// </summary>
		/// <param name="id">The comment's id</param>
		/// <param name="commentText">New comment's text</param>
		/// <returns></returns>
		/// <response code="204">The change was successful</response>
		/// <response code="400">If the new name is less than 1 character</response>
		/// <response code="404">If no comment was found with the supplied id</response>
		// PATCH api/comments/{id}
		[Authorize]
		[HttpPatch("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult Patch(string id, [FromBody] string commentText)
        {
            if (string.IsNullOrEmpty(commentText))
            {
                return BadRequest("The commentText must be at least 1 character long!");
            }
            try
            {
                var comment = context.Comments.Where(c => c.IsActive)
                                          .FirstOrDefault(c => c.Id == Mapper.GetCommentId(id));
                if (comment == null)
                {
                    return NotFound($"No Comment was found with the id {id}!");
                }
                comment.Text = commentText;
                context.SaveChanges();
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound($"No Comment was found with the id {id}!");
            }
        }

		/// <summary>
		/// Deletes a comment
		/// </summary>
		/// <param name="id">the comment's id</param>
		/// <returns></returns>
		/// <response code="204">The deletion was successful</response>
		/// <response code="404">If no comment was found with the supplied id</response>
		// DELETE api/comments/{id}
		[Authorize(Roles = "Admin")]
		[HttpDelete("{id}")]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult Delete(string id)
        {
            try
            {
                var comment = context.Comments.Where(c => c.IsActive)
                                          .FirstOrDefault(c => c.Id == Mapper.GetCommentId(id));
                if (comment == null)
                {
                    return NotFound($"No Comment was found with the id {id}!");
                }
                comment.IsActive = false;
                context.SaveChanges();
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound($"No Comment was found with the id {id}!");
            }
        }
    }
}
