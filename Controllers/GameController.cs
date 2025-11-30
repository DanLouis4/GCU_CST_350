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
            return RedirectToAction("InitializeGame", new
            {
                boardSize = savedSize,
                difficultyType = savedDifficulty
            });
        }

        //POST: /Game/MindSweeperBoard
        [HttpPost]
        public IActionResult InitializeGame(int boardSize, string difficultyType)
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

            HttpContext.Session.SetString("StartTime", DateTime.UtcNow.ToString("o"));
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

            // --- 1. HANDLE REWARD PICKUP ---------------------------------------------------

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
                // Player loses
                current.IsVisited = true;

                // Reveal all bombs
                for (int r = 0; r < board.Size; r++)
                {
                    for (int c = 0; c < board.Size; c++)
                    {
                        if (board.Cells[r, c].IsBomb)
                            board.Cells[r, c].IsVisited = true;
                    }
                }

                // Mark game state
                var result = Board.GameStatus.Lost;

                // Save board
                HttpContext.Session.SetObject("CurrentBoard", board);

                ViewBag.GameState = result;
                return View("MineSweeperBoard", board);
            }

            // --- 3. HANDLE SAFE TILE -------------------------------------------------------

            current.IsVisited = true;

            // If the tile has no neighboring bombs → flood fill
            if (current.NumberOfBombNeighbors == 0)
            {
                FloodFill(board, row, col);
            }

            // --- 4. CHECK WIN CONDITION ----------------------------------------------------

            var state = board.DetermineGameState();

            if (state == Board.GameStatus.Won)
            {
                ViewBag.GameState = "Won";

                // Save updated board
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

    }
}
