using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventTickets.Data;
using EventTickets.Models;
namespace EventTickets.Controllers;
public class EventsController : Controller
{
    private readonly AppDbContext _db;
    public EventsController(AppDbContext db) => _db = db;

    public IActionResult Index(string? category, string? q)
    {
        var query = _db.Events.Include(e => e.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(e => e.Category!.Name == category);
        if (!string.IsNullOrWhiteSpace(q))        query = query.Where(e => e.Title.Contains(q));
        ViewBag.Categories = _db.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList();
        ViewBag.SelectedCategory = category; ViewBag.Search = q;
        return View(query.OrderBy(e => e.DateTime).ToList());
    }

    public IActionResult Create()
    {
        ViewBag.CategoryOptions = new SelectList(_db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name");
        return View(new Event { DateTime = DateTime.Now.AddDays(1) });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(Event model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryOptions = new SelectList(_db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", model.CategoryId);
            return View(model);
        }
        _db.Events.Add(model); _db.SaveChanges();
        TempData["Message"] = "Event created."; return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var item = _db.Events.Find(id); if (item is null) return NotFound();
        ViewBag.CategoryOptions = new SelectList(_db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", item.CategoryId);
        return View(item);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Event model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryOptions = new SelectList(_db.Categories.OrderBy(c => c.Name).ToList(), "Id", "Name", model.CategoryId);
            return View(model);
        }
        _db.Update(model); _db.SaveChanges(); TempData["Message"] = "Event updated."; return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var item = _db.Events.Include(e => e.Category).FirstOrDefault(e => e.Id == id);
        if (item is null) return NotFound(); return View(item);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var e = _db.Events.Find(id); if (e is not null) _db.Events.Remove(e); _db.SaveChanges();
        TempData["Message"] = "Event deleted."; return RedirectToAction(nameof(Index));
    }
}
