using Microsoft.AspNetCore.Mvc;

namespace BibleVerseApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
