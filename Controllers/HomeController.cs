using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Models;

namespace MineSweeper_MVC.Controllers
{
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

        // GET: /Home/StartRedirect
        public IActionResult StartRedirect()
        {
            // Check login status using session
            if (HttpContext.Session.GetString("Username") == null)
            {
                // Not logged in ? go to login page
                return RedirectToAction("Login", "User");
            }

            // Logged in ? go to Game
            return RedirectToAction("Index", "Game");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
