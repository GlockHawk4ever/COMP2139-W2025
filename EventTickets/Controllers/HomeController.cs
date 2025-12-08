using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using EventTickets.Data;
using Microsoft.Extensions.Logging;

namespace EventTickets.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.TotalEvents = _db.Events.Count();
        ViewBag.TotalCategories = _db.Categories.Count();
        ViewBag.LowTicketEvents = _db.Events.Count(e => e.AvailableTickets < 5);
        return View();
    }

    // Global error handler view for 500 errors and unhandled exceptions
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (feature != null)
        {
            _logger.LogError(feature.Error, "Unhandled exception for path {Path}", feature.Path);
        }

        Response.StatusCode = 500;
        return View("ServerError");
    }

    // Custom status code handler (404, 500, etc.)
    public IActionResult StatusCode(int code)
    {
        var path = HttpContext.Request.Path;
        if (code == 404)
        {
            _logger.LogWarning("Page not found: {Path}", path);
            return View("NotFound");
        }

        if (code == 500)
        {
            _logger.LogError("Server error while processing {Path}", path);
            return View("ServerError");
        }

        ViewBag.StatusCode = code;
        return View("StatusCode");
    }
}
