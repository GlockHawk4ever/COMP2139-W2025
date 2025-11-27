using System;
using System.Linq;
using System.Threading.Tasks;
using COMP2139_ICE.Areas.ProjectManagement.Models;
using COMP2139_ICE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectCommentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectCommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProjectComment/GetComments?projectId=5
        [HttpGet("GetComments")]
        public async Task<IActionResult> GetComments(int projectId)
        {
            var comments = await _context.ProjectComments
                .Where(c => c.ProjectId == projectId)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new
                {
                    CommentId = c.ProjectCommentId,
                    c.Content,
                    CreatedDate = c.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Ok(comments);
        }

        public class ProjectCommentInputModel
        {
            public int ProjectId { get; set; }
            public string Content { get; set; } = string.Empty;
        }

        // POST: api/ProjectComment/AddComment
        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] ProjectCommentInputModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Content) || model.ProjectId <= 0)
            {
                return BadRequest("Invalid comment data.");
            }

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == model.ProjectId);
            if (!projectExists)
            {
                return NotFound("Project not found.");
            }

            var comment = new ProjectComment
            {
                ProjectId = model.ProjectId,
                Content = model.Content.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            await _context.ProjectComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                CommentId = comment.ProjectCommentId,
                comment.Content,
                CreatedDate = comment.CreatedDate.ToString("yyyy-MM-dd HH:mm")
            });
        }
    }
}
