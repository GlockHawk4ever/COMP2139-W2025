using System;
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
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ProjectManagement/Projects or /ProjectManagement/Projects/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Projects
                .OrderBy(p => p.Id)
                .ToListAsync();

            return View(list);
        }

        // GET: /ProjectManagement/Projects/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new Project());
        }

        // POST: /ProjectManagement/Projects/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (!ModelState.IsValid)
            {
                return View(project);
            }

            if (project.StartDate.HasValue)
            {
                project.StartDate = DateTime.SpecifyKind(project.StartDate.Value, DateTimeKind.Utc);
            }

            if (project.DueDate.HasValue)
            {
                project.DueDate = DateTime.SpecifyKind(project.DueDate.Value, DateTimeKind.Utc);
            }

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /ProjectManagement/Projects/Details/5
        [HttpGet("Details/{id:int:min(1)}")]
        public async Task<IActionResult> Details(int id)
        {
            var proj = await _context.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proj == null)
            {
                return NotFound();
            }

            return View(proj);
        }

        // GET: /ProjectManagement/Projects/Edit/5
        [HttpGet("Edit/{id:int:min(1)}")]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: /ProjectManagement/Projects/Edit/5
        [HttpPost("Edit/{id:int:min(1)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(project);
            }

            if (project.StartDate.HasValue)
            {
                project.StartDate = DateTime.SpecifyKind(project.StartDate.Value, DateTimeKind.Utc);
            }

            if (project.DueDate.HasValue)
            {
                project.DueDate = DateTime.SpecifyKind(project.DueDate.Value, DateTimeKind.Utc);
            }

            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Try again.");
                return View(project);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /ProjectManagement/Projects/Delete/5
        [HttpGet("Delete/{id:int:min(1)}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: /ProjectManagement/Projects/DeleteConfirmed/5
        [HttpPost("DeleteConfirmed/{id:int:min(1)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /ProjectManagement/Projects/Search?searchTerm=...
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            ViewBag.IsSearch = true;
            ViewBag.SearchTerm = searchTerm;

            var query = _context.Projects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    (p.Description != null && p.Description.Contains(searchTerm)));
            }

            var list = await query
                .OrderBy(p => p.Id)
                .ToListAsync();

            return View("Index", list);
        }
    }
}
