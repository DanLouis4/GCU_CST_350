using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Filters;

namespace MineSweeper_MVC.Controllers
{
    [RequiresLogin]
    public class GameController : Controller
    {
        // GET: /Game/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
