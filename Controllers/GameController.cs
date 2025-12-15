using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Filters;
using MineSweeper_MVC.Models;
using MineSweeper_MVC.Extensions;
using MineSweeper_MVC.Services;
using Newtonsoft.Json;


namespace MineSweeper_MVC.Controllers
{
    // Controller to manage game-related actions, requires user to be logged in
    [RequiresLogin]
    public class GameController : Controller
    {

        private readonly CoreGameServices _gameServices = new CoreGameServices();


        // GET: /Game/StartGame: Display game start options
        [HttpGet]
        public IActionResult StartGame()
        {
            // Clear any existing game session data
            ClearGameSession();

            // Retrieve username from session to ensure user is logged in
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User");
            }

            // Provide the view with default configuration values
            var options = new StartGameOptions
            {
                BoardSize = 10,
                DifficultyType = "Easy"
            };

            return View("StartGame", options);
        }

        // Setting a method to restart the game with previous settings
        [HttpGet]
        public IActionResult RestartGame()
        {
            // Clear any existing game session data
            ClearGameSession();

            // Ensure user is authenticated
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User");
            }

            // Retrieve previous settings from session
            var savedSize = HttpContext.Session.GetInt32($"LastBoardSize_{username}") ?? 10;
            var savedDifficulty = HttpContext.Session.GetString($"LastDifficulty_{username}") ?? "Easy";

            // Delegate to the POST initializer
            return RedirectToAction("InitializeGameGet", new
            {
                boardSize = savedSize,
                difficultyType = savedDifficulty
            });
        }

        [HttpGet]
        public IActionResult InitializeGameGet(int boardSize, string difficultyType)
        {
            return InitializeGameInternal(boardSize, difficultyType);
        }

        //POST: /Game/MindSweeperBoard
        [HttpPost]
        public IActionResult InitializeGamePost(int boardSize, string difficultyType)
        {
            return InitializeGameInternal(boardSize, difficultyType);
        }

        private IActionResult InitializeGameInternal(int boardSize, string difficultyType)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "User");
            }

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

            board.GameId = Random.Shared.Next(10000000, 99999999); // 8-digit seed

            // Set the start time in both Session AND Board
            var now = DateTime.UtcNow;
            board.StartTime = now;

            // HttpContext.Session.SetString("StartTime", DateTime.UtcNow.ToString("o"));
            HttpContext.Session.SetString($"LastBoardSize_{username}", boardSize.ToString());
            HttpContext.Session.SetString($"LastDifficulty_{username}", difficultyType);
            HttpContext.Session.SetObject("CurrentBoard", board);

            return RedirectToAction("MineSweeperBoard");
        }

        [HttpPost]
        public IActionResult VisitCell(string cell)
        {
            // --- 0. VALIDATE INPUT & SESSION ----------------------------------------------
            var parts = cell.Split(',');
            int row = int.Parse(parts[0]);
            int col = int.Parse(parts[1]);

            var board = HttpContext.Session.GetObject<Board>("CurrentBoard");
            if (board == null)
                return RedirectToAction("StartGame");

            // --- 1. RUN SHARED CLICK LOGIC -------------------------------------------------
            var state = _gameServices.ProcessCellClick(board, row, col);

            // --- 2. HANDLE LOSS UI ---------------------------------------------------------
            if (state == Board.GameStatus.Lost)
            {
                board.EndTime = DateTime.UtcNow;
                int score = board.DetermineFinalScore(Board.GameStatus.Lost);

                ViewBag.GameState = Board.GameStatus.Lost;
                ViewBag.GameOver = true;
                ViewBag.FinalScore = score;

                var elapsed = board.EndTime - board.StartTime;
                ViewBag.ElapsedTime = elapsed.ToString(@"mm\:ss");

                HttpContext.Session.SetObject("CurrentBoard", board);
                return View("MineSweeperBoard", board);
            }

            // --- 3. HANDLE WIN UI ----------------------------------------------------------
            if (state == Board.GameStatus.Won)
            {
                board.EndTime = DateTime.UtcNow;
                int score = board.DetermineFinalScore(Board.GameStatus.Won);

                ViewBag.GameState = Board.GameStatus.Won;
                ViewBag.GameOver = true;
                ViewBag.FinalScore = score;

                var elapsed = board.EndTime - board.StartTime;
                ViewBag.ElapsedTime = elapsed.ToString(@"mm\:ss");

                HttpContext.Session.SetObject("CurrentBoard", board);
                return View("MineSweeperBoard", board);
            }

            // --- 4. SAVE AND RETURN NORMAL BOARD -------------------------------------------
            HttpContext.Session.SetObject("CurrentBoard", board);
            return View("MineSweeperBoard", board);
        }

        [HttpPost]
        public IActionResult VisitCellAjax(int row, int col)
        {
            // --- 0. VALIDATE SESSION -------------------------------------------------------
            var board = HttpContext.Session.GetObject<Board>("CurrentBoard");
            if (board == null)
                return BadRequest("No game in session.");

            // --- 1. PROCESS CLICK THROUGH SHARED LOGIC -------------------------------------
            var state = _gameServices.ProcessCellClick(board, row, col);

            // --- 2. SET VIEWBAG OUTCOME DATA FOR PARTIAL RENDERING -------------------------
            ViewBag.GameState = state;
            ViewBag.GameOver = (state == Board.GameStatus.Won || state == Board.GameStatus.Lost);

            if (ViewBag.GameOver)
            {
                // Record end time for elapsed time and scoring
                board.EndTime = DateTime.UtcNow;

                // Calculate final score based on win/loss
                ViewBag.FinalScore = board.DetermineFinalScore(state);

                // Compute elapsed time in mm:ss format
                var elapsed = board.EndTime - board.StartTime;
                ViewBag.ElapsedTime = elapsed.ToString(@"mm\:ss");          
            }

            // --- 3. SAVE UPDATED BOARD BACK INTO SESSION ------------------------------------
            HttpContext.Session.SetObject("CurrentBoard", board);

            // --- 4. RETURN UPDATED BOARD PARTIAL FOR AJAX -----------------------------------
            return PartialView("_GameState", board);
        }

        [HttpPost]
        public IActionResult ToggleFlagAjax(int row, int col)
        {
            var board = HttpContext.Session.GetObject<Board>("CurrentBoard");
            if (board == null)
                return BadRequest();

            _gameServices.ToggleFlag(board, row, col);

            // Update ViewBag for Razor
            ViewBag.GameOver = false;
            ViewBag.GameState = Board.GameStatus.InProgress;

            HttpContext.Session.SetObject("CurrentBoard", board);

            return PartialView("_GameState", board);
        }

        // GET: /Game/MinesweeperBoard
        public IActionResult MinesweeperBoard()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "User");

            var board = HttpContext.Session.GetObject<Board>("CurrentBoard");
            if (board == null)
                return RedirectToAction("StartGame");

            return View(board);
        }

         // Clear game session data
        private void ClearGameSession()
        {
            HttpContext.Session.Remove("GameOver");
            HttpContext.Session.Remove("StartTime");
            HttpContext.Session.Remove("CurrentBoard");
        }

    }
}
