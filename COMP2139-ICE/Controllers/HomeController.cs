using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using COMP2139_ICE.Models;

namespace COMP2139_ICE.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) =>
        _logger = logger;

    public IActionResult Index() =>
        View();

    public IActionResult About() =>
        View();

    // General search entry point from navbar
    [HttpGet]
    public IActionResult Search(string searchType, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchType) || string.IsNullOrWhiteSpace(searchTerm))
        {
            return RedirectToAction(nameof(Index));
        }

        var type = searchType.Trim().ToLowerInvariant();

        if (type == "project")
        {
            return RedirectToAction("Search", "Projects", new { area = "ProjectManagement", searchTerm });
        }

        if (type == "task")
        {
            return RedirectToAction("Search", "ProjectTask", new { area = "ProjectManagement", searchTerm });
        }

        return RedirectToAction(nameof(Index));
    }

    // Custom NotFound view for 404
    [HttpGet]
    public IActionResult NotFound(int statusCode)
    {
        Response.StatusCode = statusCode == 0 ? 404 : statusCode;
        ViewBag.StatusCode = Response.StatusCode;
        return View("NotFound");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
