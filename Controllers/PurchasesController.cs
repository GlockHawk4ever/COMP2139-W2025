using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventTickets.Data;
using EventTickets.Models;
namespace EventTickets.Controllers;
public class PurchasesController : Controller
{
    private readonly AppDbContext _db;
    public PurchasesController(AppDbContext db) => _db = db;

    // Purchase history
    public IActionResult Index()
    {
        var items = _db.Purchases.Include(p => p.Event).OrderByDescending(p => p.PurchasedAt).ToList();
        return View(items);
    }

    // Buy tickets
    public IActionResult Create()
    {
        ViewBag.Events = _db.Events.OrderBy(e => e.Title).ToList();
        return View(new Purchase { Quantity = 1 });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Create(Purchase input)
    {
        var ev = _db.Events.Find(input.EventId);

        if (ev is null)
        {
            ModelState.AddModelError("", "Invalid event.");
        }
        else
        {
            if (ev.AvailableTickets <= 0)
            {
                ModelState.AddModelError("", "Sorry, this event is sold out.");
            }
            else if (input.Quantity > ev.AvailableTickets)
            {
                ModelState.AddModelError("", $"Only {ev.AvailableTickets} ticket(s) left for this event.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Events = _db.Events.OrderBy(e => e.Title).ToList();
            return View(input);
        }

        input.Total = ev!.Price * input.Quantity;
        input.PurchasedAt = DateTime.Now;
        _db.Purchases.Add(input);
        ev.AvailableTickets -= input.Quantity;
        _db.SaveChanges();
        return RedirectToAction(nameof(Confirm), new { id = input.Id });
    }

    public IActionResult Confirm(int id)
    {
        var p = _db.Purchases.Include(p => p.Event).FirstOrDefault(p => p.Id == id);
        if (p is null) return NotFound();
        return View(p);
    }
}
