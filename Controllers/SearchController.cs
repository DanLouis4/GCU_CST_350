using Microsoft.AspNetCore.Mvc;

namespace BibleVerseApp.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
