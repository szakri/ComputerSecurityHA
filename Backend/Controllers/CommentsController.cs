﻿using Backend.Services;
using DAL;
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

        // GET: api/comments
        [HttpGet]
        public ActionResult<IEnumerable<CommentDTO>> Get()
        {
            return Ok(context.Comments.Include(c => c.User)
                                      .Where(c => c.IsActive)
                                      .Select(c => Mapper.ToCommentDTO(c)));
        }

        // GET api/comments/{id}
        [HttpGet("{id}")]
        public ActionResult<CommentDTO> Get(string id)
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

        // POST api/comments
        [HttpPost]
        public ActionResult<CommentDTO> Post(string caffId, string userId, string text)
        {
            var caff = context.Caffs.Where(c => c.IsActive)
                                    .FirstOrDefault(c => c.Id == Mapper.GetCaffId(caffId));
            if (caff == null)
            {
                return NotFound($"No CAFF file was found with the id {caffId}!");
            }
            var user = context.Users.Where(u => u.IsActive)
                                    .FirstOrDefault(u => u.Id == Mapper.GetUserId(userId));
            if (user == null)
            {
                return NotFound($"No User was found with the id {userId}!");
            }
            var comment = new Comment
            {
                User = user,
                Text = text
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
            return CreatedAtAction("Get", new { id = Mapper.GetCommentHash(comment.Id) }, comment);
        }

        // PATCH api/comments/{id}
        [HttpPatch("{id}")]
        public ActionResult Patch(string id, [FromBody] string text)
        {
            var comment = context.Comments.Where(c => c.IsActive)
                                          .FirstOrDefault(c => c.Id == Mapper.GetCommentId(id));
            if (comment == null)
            {
                return NotFound($"No Comment was found with the id {id}!");
            }
            comment.Text = text;
            context.SaveChanges();
            return NoContent();
        }

        // DELETE api/comments/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
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
    }
}