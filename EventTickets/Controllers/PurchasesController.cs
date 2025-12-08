using EventTickets.Data;
using EventTickets.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace EventTickets.Controllers;

[Authorize(Roles = "Attendee")]
public class PurchasesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PurchasesController> _logger;

    public PurchasesController(AppDbContext db, UserManager<ApplicationUser> userManager, ILogger<PurchasesController> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    // Purchase history for the logged-in user
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var items = await _db.Purchases
            .Include(p => p.Event)
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync();

        return View(items);
    }

    // GET: buy tickets
    public IActionResult Create()
    {
        ViewBag.Events = _db.Events.OrderBy(e => e.Title).ToList();
        return View(new Purchase { Quantity = 1 });
    }

    // POST: buy tickets
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Purchase input)
    {
        var ev = await _db.Events.FindAsync(input.EventId);

        if (ev is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid event.");
        }
        else
        {
            if (ev.AvailableTickets <= 0)
            {
                ModelState.AddModelError(string.Empty, "Sorry, this event is sold out.");
            }
            else if (input.Quantity > ev.AvailableTickets)
            {
                ModelState.AddModelError(string.Empty, $"Only {ev.AvailableTickets} ticket(s) left for this event.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Events = _db.Events.OrderBy(e => e.Title).ToList();
            return View(input);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        input.Total = ev!.Price * input.Quantity;
        input.PurchasedAt = DateTime.Now;
        input.UserId = user.Id;
        input.CustomerName = user.FullName ?? user.UserName ?? input.CustomerName;
        input.Email = user.Email ?? input.Email;

        _db.Purchases.Add(input);
        ev.AvailableTickets -= input.Quantity;
        await _db.SaveChangesAsync();

        _logger.LogInformation("User {UserId} purchased {Qty} tickets for event {EventId} at total {Total}",
            user.Id, input.Quantity, input.EventId, input.Total);

        return RedirectToAction(nameof(Confirm), new { id = input.Id });
    }

    public async Task<IActionResult> Confirm(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var p = await _db.Purchases
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

        if (p is null)
        {
            return NotFound();
        }

        return View(p);
    }

    // Small QR image for dashboard + ticket
    public async Task<IActionResult> TicketQr(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var p = await _db.Purchases.Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

        if (p is null)
        {
            return NotFound();
        }

        var payload = $"TICKET:{p.Id};EVENT:{p.Event?.Title};QTY:{p.Quantity};WHEN:{p.Event?.DateTime}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(10); // size tuned; CSS will also limit display

        return File(pngBytes, "image/png");
    }

    // Very simple HTML-based ticket "download" (print-friendly)
    public async Task<IActionResult> DownloadTicket(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var p = await _db.Purchases.Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);

        if (p is null)
        {
            return NotFound();
        }

        return View("Ticket", p);
    }
}
