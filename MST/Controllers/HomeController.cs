using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MST.Models;

namespace MST.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }
    [Authorize(Roles = "User")]
    public IActionResult User()
    {
        return View();
    }
    public IActionResult Service()
    {
        return View();
    }
    public IActionResult AboutUs()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
