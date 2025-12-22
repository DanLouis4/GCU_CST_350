using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Models;
using MineSweeper_MVC.Services;

namespace MineSweeper_MVC.Controllers
{
    // Handles HTTP flow and session state only.
    // All business logic and persistence are delegated to services.
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // ----------------------------------------------------
        // Registration
        // ----------------------------------------------------
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _userService.Register(model);
                TempData["RegisterSuccess"] = "Registration successful. Please sign in.";
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // ----------------------------------------------------
        // Login
        // ----------------------------------------------------
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userService.Login(model);
            if (userId == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", userId.Value);
            HttpContext.Session.SetString("Username", model.Username);

            return RedirectToAction("StartRedirect", "Home");
        }

        // ----------------------------------------------------
        // Profile (View / Edit)
        // ----------------------------------------------------
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var model = _userService.GetProfile(userId.Value);
            return View(model);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();
            
            var model = _userService.GetProfile(userId.Value);
            return PartialView("_ProfileEdit", model);
        }

        [HttpPost]
        public IActionResult UpdateProfile(UserProfileViewModel model)
        {
            _userService.UpdateProfile(model);
            var updated = _userService.GetProfile(model.Id);
            return PartialView("_ProfileView", updated);
        }

        // ----------------------------------------------------
        // Logout
        // ----------------------------------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
