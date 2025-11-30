using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Filters;
using MineSweeper_MVC.Models;
using MineSweeper_MVC.Extensions;
using Newtonsoft.Json;


namespace MineSweeper_MVC.Controllers
{
    // Controller to manage game-related actions, requires user to be logged in
    [RequiresLogin]
    public class GameController : Controller
    {
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

            // Set the start time in both Session AND Board
            var now = DateTime.UtcNow;
            board.StartTime = now;

            // HttpContext.Session.SetString("StartTime", DateTime.UtcNow.ToString("o"));
            HttpContext.Session.SetString($"LastBoardSize_{username}", boardSize.ToString());
            HttpContext.Session.SetString($"LastDifficulty_{username}", difficultyType);
            HttpContext.Session.SetObject("CurrentBoard", board);

            return RedirectToAction("MineSweeperBoard");
        }

        // POST: /Game/VisitCell: Handle cell visit actions
        [HttpPost]
        public IActionResult VisitCell(string cell)
        {
            // Validate user
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "User");

            // Get board
            var board = HttpContext.Session.GetObject<Board>("CurrentBoard");
            if (board == null)
                return RedirectToAction("StartGame");

            // Parse the "row,col" input
            var parts = cell.Split(',');
            int row = int.Parse(parts[0]);
            int col = int.Parse(parts[1]);

            var current = board.Cells[row, col];

            // If already visited, no action needed
            if (current.IsVisited)
                return View("MineSweeperBoard", board);

            // --- 1. HANDLE REWARD PICKUP (Rewards not Implemented) ---------------------------------------------------

            if (current.Reward == Cell.RewardType.Detector)
            {
                board.DetectorOwned++;
                current.Reward = Cell.RewardType.None;
                current.IsVisited = true;
            }
            else if (current.Reward == Cell.RewardType.Radar)
            {
                board.RadarOwned++;
                current.Reward = Cell.RewardType.None;
                current.IsVisited = true;
            }

            // --- 2. HANDLE BOMB CLICK ------------------------------------------------------

            if (current.IsBomb && !current.IsDeactivated)
            {
                // Mark the bomb hit
                current.IsVisited = true;

                // Reveal all bombs so board updates visually later
                for (int r = 0; r < board.Size; r++)
                {
                    for (int c = 0; c < board.Size; c++)
                    {
                        if (board.Cells[r, c].IsBomb)
                            board.Cells[r, c].IsVisited = true;
                    }
                }

                board.EndTime = DateTime.Now;
                int score = board.DetermineFinalScore(Board.GameStatus.Lost);

                foreach (var c in board.Cells)
                {
                    if (c.IsBomb)
                        c.IsDeactivated = true;
                }

                ViewBag.GameState = Board.GameStatus.Lost;
                ViewBag.GameOver = true;
                ViewBag.FinalScore = score;

                board.EndTime = DateTime.UtcNow;
                var elapsed = board.EndTime - board.StartTime;
                ViewBag.ElapsedTime = elapsed.ToString(@"mm\:ss");


                HttpContext.Session.SetObject("CurrentBoard", board);

                return View("MineSweeperBoard", board);
            }

            // --- 3. HANDLE SAFE TILE -------------------------------------------------------

            current.IsVisited = true;

            // If the tile has no neighboring bombs → flood fill
            if (current.NumberOfBombNeighbors == 0)
            {
                FloodFill(board, row, col);
            }

            // --- 4. CHECK WON CONDITION ----------------------------------------------------

            var state = board.DetermineGameState();

            if (state == Board.GameStatus.Won)
            {
                board.EndTime = DateTime.Now;
                int score = board.DetermineFinalScore(Board.GameStatus.Won);

                // Reveal all cells after winning
                for (int r = 0; r < board.Size; r++)
                {
                    for (int c = 0; c < board.Size; c++)
                    {
                        board.Cells[r, c].IsVisited = true;
                    }
                }

                // Save results for the Win View
                ViewBag.GameState = Board.GameStatus.Won;
                ViewBag.GameOver = true;
                ViewBag.FinalScore = score;

                board.EndTime = DateTime.UtcNow;
                var elapsed = board.EndTime - board.StartTime;
                ViewBag.ElapsedTime = elapsed.ToString(@"mm\:ss");

                // Persist board end state (optional but safe)
                HttpContext.Session.SetObject("CurrentBoard", board);

                return View("MineSweeperBoard", board);
            }

            // --- 5. SAVE AND RETURN --------------------------------------------------------

            HttpContext.Session.SetObject("CurrentBoard", board);

            return View("MineSweeperBoard", board);
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

        /*
         * -----------------------------------------
         * Helper Methods
         * -----------------------------------------
         */

        // Recursive flood fill to reveal empty cells
        private void FloodFill(Board board, int row, int col)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int newRow = row + dr;
                    int newCol = col + dc;

                    if (board.IsCellOnBoard(newRow, newCol))
                    {
                        var neighbor = board.Cells[newRow, newCol];

                        if (!neighbor.IsVisited && !neighbor.IsBomb)
                        {
                            neighbor.IsVisited = true;

                            if (neighbor.NumberOfBombNeighbors == 0)
                            {
                                FloodFill(board, newRow, newCol);
                            }
                        }
                    }
                }
            }
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
