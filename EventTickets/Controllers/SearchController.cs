using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventTickets.Data;
namespace EventTickets.Controllers;
public class SearchController : Controller
{
    private readonly AppDbContext _db;
    public SearchController(AppDbContext db) => _db = db;
    public IActionResult Index(string? q, string? category, DateTime? start, DateTime? end, string? availability, string? sort)
    {
        var query = _db.Events.Include(e => e.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(e => e.Title.Contains(q));
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(e => e.Category!.Name == category);
        if (start.HasValue) query = query.Where(e => e.DateTime >= start.Value);
        if (end.HasValue)   query = query.Where(e => e.DateTime <= end.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(availability))
        {
            if (availability == "available") query = query.Where(e => e.AvailableTickets > 0);
            if (availability == "soldout")   query = query.Where(e => e.AvailableTickets == 0);
        }
        query = sort switch { "title" => query.OrderBy(e => e.Title), "date" => query.OrderBy(e => e.DateTime), "price" => query.OrderBy(e => e.Price), _ => query.OrderBy(e => e.DateTime) };
        ViewBag.Categories = _db.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList();
        return View(query.ToList());
    }
}
