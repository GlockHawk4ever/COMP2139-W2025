using System;
using Microsoft.AspNetCore.Mvc;
using COMP2139_ICE.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly COMP2139_ICE.Data.ApplicationDbContext _context;

        public ProjectsController(COMP2139_ICE.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Projects
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _context.Projects.OrderBy(p => p.Id).ToListAsync();
                return View(list);
            }
            catch (Exception ex)
            {
                ViewBag.DbError = ex.Message;
                return View(new List<Project>());
            }
        }

        // GET: /Projects/Create
        public IActionResult Create()
        {
            return View(new Project());
        }

        // POST: /Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (!ModelState.IsValid)
            {
                return View(project);
            }
            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Projects/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var proj = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (proj == null)
            {
                return NotFound();
            }
            return View(proj);
        }
    }
}
