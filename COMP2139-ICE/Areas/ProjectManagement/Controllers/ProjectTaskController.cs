using System.Linq;
using System.Threading.Tasks;
using COMP2139_ICE.Areas.ProjectManagement.Models;
using COMP2139_ICE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE.Areas.ProjectManagement.Controllers
{
    [Area("ProjectManagement")]
    [Route("ProjectManagement/[controller]")]
    public class ProjectTaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectTaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ProjectManagement/ProjectTask/Index?projectId=5
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.Project = project;

            var tasks = await _context.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.ProjectTaskId)
                .ToListAsync();

            return View(tasks);
        }

        // GET: /ProjectManagement/ProjectTask/Details/5
        [HttpGet("Details/{id:int:min(1)}")]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.ProjectTaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: /ProjectManagement/ProjectTask/Create?projectId=5
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            ViewBag.Project = project;

            var model = new ProjectTask
            {
                ProjectId = projectId
            };

            return View(model);
        }

        // POST: /ProjectManagement/ProjectTask/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectTask task)
        {
            if (!ModelState.IsValid)
            {
                var project = await _context.Projects.FindAsync(task.ProjectId);
                ViewBag.Project = project;
                return View(task);
            }

            await _context.ProjectTasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
        }

        // GET: /ProjectManagement/ProjectTask/Edit/5
        [HttpGet("Edit/{id:int:min(1)}")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: /ProjectManagement/ProjectTask/Edit/5
        [HttpPost("Edit/{id:int:min(1)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectTask task)
        {
            if (id != task.ProjectTaskId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(task);
            }

            try
            {
                _context.ProjectTasks.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Try again.");
                return View(task);
            }

            return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
        }

        // GET: /ProjectManagement/ProjectTask/Delete/5
        [HttpGet("Delete/{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.ProjectTaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: /ProjectManagement/ProjectTask/DeleteConfirmed/5
        [HttpPost("DeleteConfirmed/{id:int:min(1)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var projectId = task.ProjectId;

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { projectId });
        }

        // GET: /ProjectManagement/ProjectTask/Search?searchTerm=...
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            ViewBag.IsSearch = true;
            ViewBag.SearchTerm = searchTerm;

            var tasksQuery = _context.ProjectTasks
                .Include(t => t.Project)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    (t.Description != null && t.Description.Contains(searchTerm)));
            }

            var tasks = await tasksQuery
                .OrderBy(t => t.ProjectTaskId)
                .ToListAsync();

            ViewBag.Project = null;

            return View("Index", tasks);
        }
    }
}
