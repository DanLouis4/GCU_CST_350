using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Filters;
using MineSweeper_MVC.Models;
using MineSweeper_MVC.Extensions;
using Newtonsoft.Json;


namespace MineSweeper_MVC.Controllers
{
    [RequiresLogin]
    public class GameController : Controller
    {
        // GET: /Game/StartGame
        [HttpGet]
        public IActionResult StartGame()
        {
            // 
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User");
            }

            // Provide the view with default configuration values
            var model = new StartGameOptions
            {
                BoardSize = 10,
                DifficultyType = "Easy"
            };

            return View("StartGame", model);
        }

        // Setting a method to restart the game with previous settings
        [HttpGet]
        public IActionResult RestartGame()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "User");
            }

            var username = HttpContext.Session.GetString("Username")!;
            var savedSize = HttpContext.Session.GetInt32($"LastBoardSize_{username}") ?? 10;
            var savedDifficulty = HttpContext.Session.GetString($"LastDifficulty_{username}") ?? "Easy";

            // Delegate to the POST initializer
            return RedirectToAction("InitializeGame", new
            {
                boardSize = savedSize,
                difficultyType = savedDifficulty
            });
        }

        //POST: /Game/MindsweeperBoard
        [HttpPost]
        public IActionResult InitializeGame(int boardSize, string difficultyType)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "User");
            }

            var username = User.Identity!.Name!;

            float difficulty = difficultyType switch
            {
                "Easy" => 0.075f,
                "Normal" => 0.10f,
                "Hard" => 0.25f,
                _ => 0.075f
            };

            var board = new Board(boardSize, difficulty, difficultyType);
            board.SetupBombs();
            board.SetupRewards();
            board.CountBombNearby();

            HttpContext.Session.SetString($"LastBoardSize_{username}", boardSize.ToString());
            HttpContext.Session.SetString($"LastDifficulty_{username}", difficultyType);
            HttpContext.Session.SetObject("CurrentBoard", board);

            return RedirectToAction("MineSweeperBoard");
        }

        // GET: /Game/MinesweeperBoard
        public IActionResult MinesweeperBoard()
        {
            return View();
        }
    }
}
