using Microsoft.AspNetCore.Mvc;

namespace BibleVerseApp.Controllers
{
    public class NotesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
