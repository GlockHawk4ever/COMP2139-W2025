using EventTickets.Data;
using EventTickets.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var now = DateTime.Now;

        // Load all purchases for this user once
        var purchasesForUser = await _db.Purchases
            .Include(p => p.Event)
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync();

        // Upcoming = only tickets where the event date is still in the future
        var upcoming = purchasesForUser
            .Where(p => p.Event != null && p.Event.DateTime >= now)
            .ToList();

        // Purchase history = ALL purchases (regardless of event date)
        var past = purchasesForUser.ToList();

        // Events I organize / manage
        var myEventsQuery = _db.Events
            .Include(e => e.Category)
            .AsQueryable();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var isOrganizer = await _userManager.IsInRoleAsync(user, "Organizer");

        if (isAdmin)
        {
            // Admin sees all events
        }
        else if (isOrganizer)
        {
            // Organizer sees only their own events
            myEventsQuery = myEventsQuery.Where(e => e.OrganizerId == user.Id);
        }
        else
        {
            // Attendee and other roles: no organizer events
            myEventsQuery = myEventsQuery.Where(e => false);
        }

        var myEvents = await myEventsQuery.ToListAsync();

        var revenueByEvent = await _db.Purchases
            .Where(p => p.EventId != 0)
            .GroupBy(p => p.EventId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(p => p.Total));

        var vm = new DashboardViewModel
        {
            CurrentUser = user,
            UpcomingTickets = upcoming,
            PastPurchases = past,
            MyEvents = myEvents,
            RevenueByEvent = revenueByEvent
        };

        return View(vm);
    }
}
