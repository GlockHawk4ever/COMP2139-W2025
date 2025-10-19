using Microsoft.AspNetCore.Mvc;
using EventTickets.Data;
using EventTickets.Models;
namespace EventTickets.Controllers;
public class CategoriesController : Controller
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;
    public IActionResult Index() => View(_db.Categories.OrderBy(c => c.Name).ToList());
    public IActionResult Create() => View(new Category());
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(Category model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Categories.Add(model); _db.SaveChanges(); return RedirectToAction(nameof(Index));
    }
}
