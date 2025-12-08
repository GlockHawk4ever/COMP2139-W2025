using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventTickets.Data;
using EventTickets.Models;

namespace EventTickets.Controllers;

public class EventsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Public event listing
    public async Task<IActionResult> Index(string? category, string? q)
    {
        var query = _db.Events.Include(e => e.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(e => e.Category != null && e.Category.Name == category);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(e => e.Title.Contains(q));
        }

        ViewBag.Categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();

        ViewBag.SelectedCategory = category;
        ViewBag.Search = q;

        var items = await query.OrderBy(e => e.DateTime).ToListAsync();
        return View(items);
    }

    // Public event details
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Events
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    // Admin + Organizer only: create new events
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoryOptionsAsync();
        return View(new Event { DateTime = DateTime.Now.AddDays(1) });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoryOptionsAsync(model.CategoryId);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            // OrganizerId is used to enforce "organizer can only edit their own events"
            model.OrganizerId = user.Id;
        }

        _db.Events.Add(model);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Event created.";
        return RedirectToAction(nameof(Index));
    }

    // Admin + Organizer only: edit events
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Events.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        // Organizers may edit only their own events
        if (await _userManager.IsInRoleAsync(user, "Organizer") && item.OrganizerId != user.Id)
        {
            return Forbid();
        }

        await PopulateCategoryOptionsAsync(item.CategoryId);
        return View(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoryOptionsAsync(model.CategoryId);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var existing = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (existing is null)
        {
            return NotFound();
        }

        // Organizers may edit only their own events
        if (await _userManager.IsInRoleAsync(user, "Organizer") && existing.OrganizerId != user.Id)
        {
            return Forbid();
        }

        // Preserve organizer link
        model.OrganizerId = existing.OrganizerId;

        _db.Update(model);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    // Admin + Organizer only: delete events
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Events
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        if (await _userManager.IsInRoleAsync(user, "Organizer") && item.OrganizerId != user.Id)
        {
            return Forbid();
        }

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin,Organizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var item = await _db.Events.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        if (await _userManager.IsInRoleAsync(user, "Organizer") && item.OrganizerId != user.Id)
        {
            return Forbid();
        }

        _db.Events.Remove(item);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Event deleted.";
        return RedirectToAction(nameof(Index));
    }


    // Organizer & Admin analytics dashboard
    [Authorize(Roles = "Admin,Organizer")]
    public IActionResult MyAnalytics()
    {
        return View();
    }

    // JSON: ticket sales by category
    [Authorize(Roles = "Admin,Organizer")]
    [HttpGet]
    public async Task<IActionResult> CategorySalesData()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        var purchases = await _db.Purchases
            .Include(p => p.Event)
            .ThenInclude(e => e.Category)
            .ToListAsync();

        if (!isAdmin)
        {
            purchases = purchases
                .Where(p => p.Event != null && p.Event.OrganizerId == user.Id)
                .ToList();
        }

        var data = purchases
            .Where(p => p.Event != null && p.Event.Category != null)
            .GroupBy(p => p.Event!.Category!.Name)
            .Select(g => new
            {
                category = g.Key,
                tickets = g.Sum(p => p.Quantity)
            })
            .OrderByDescending(x => x.tickets)
            .ToList();

        return Json(data);
    }

    // JSON: revenue per month
    [Authorize(Roles = "Admin,Organizer")]
    [HttpGet]
    public async Task<IActionResult> RevenueByMonthData()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        var purchases = await _db.Purchases
            .Include(p => p.Event)
            .ToListAsync();

        if (!isAdmin)
        {
            purchases = purchases
                .Where(p => p.Event != null && p.Event.OrganizerId == user.Id)
                .ToList();
        }

        var grouped = purchases
            .GroupBy(p => new { p.PurchasedAt.Year, p.PurchasedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                year = g.Key.Year,
                month = g.Key.Month,
                label = $"{g.Key.Year}-{g.Key.Month:D2}",
                revenue = g.Sum(p => p.Total)
            })
            .ToList();

        return Json(grouped);
    }

    // JSON: Top 5 best-selling events
    [Authorize(Roles = "Admin,Organizer")]
    [HttpGet]
    public async Task<IActionResult> TopEventsData()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        var purchases = await _db.Purchases
            .Include(p => p.Event)
            .ToListAsync();

        if (!isAdmin)
        {
            purchases = purchases
                .Where(p => p.Event != null && p.Event.OrganizerId == user.Id)
                .ToList();
        }

        var data = purchases
            .Where(p => p.Event != null)
            .GroupBy(p => new { p.EventId, Title = p.Event!.Title })
            .Select(g => new
            {
                eventId = g.Key.EventId,
                title = g.Key.Title,
                tickets = g.Sum(p => p.Quantity),
                revenue = g.Sum(p => p.Total)
            })
            .OrderByDescending(x => x.tickets)
            .Take(5)
            .ToList();

        return Json(data);
    }

    private async Task PopulateCategoryOptionsAsync(int? selectedId = null)
    {
        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.CategoryOptions = new SelectList(categories, "Id", "Name", selectedId);
    }
}
