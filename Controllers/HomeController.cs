using Microsoft.AspNetCore.Mvc;
using EventTickets.Data;
namespace EventTickets.Controllers;
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;
    public IActionResult Index()
    {
        ViewBag.TotalEvents = _db.Events.Count();
        ViewBag.TotalCategories = _db.Categories.Count();
        ViewBag.LowTicketEvents = _db.Events.Count(e => e.AvailableTickets < 5);
        return View();
    }
}
