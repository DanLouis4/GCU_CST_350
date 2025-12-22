using MineSweeper_MVC.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace MineSweeper_MVC.Services
{
    public class CoreGameServices
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CoreGameServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ------------------------------------------------------------
        // PUBLIC API — what the controller is allowed to call
        // ------------------------------------------------------------

        // Generates a unique game seed.
        public async Task<int> GenerateSeed()
        {
            int seed;

            do
            {
                seed = Random.Shared.Next(10000000, 99999999); // 8-digit seed
            }
            while (await GameSeedExists(seed));
            return seed;
        }

        public Board.GameStatus ProcessCellClick(Board board, int row, int col)
        {
            var current = board.Cells[row, col];

            // --- 0. IGNORE INVALID OR POST-GAME CLICKS ----------------
            if (current.IsVisited || current.IsFlagged ||
                board.CurrentStatus != Board.GameStatus.InProgress)
            {
                return board.CurrentStatus;
            }

            // --- 1. HANDLE BOMB CLICK ---------------------------------
            if (current.IsBomb)
            {
                RevealAllBombs(board);
                board.CurrentStatus = Board.GameStatus.Lost;
                board.EndTime = DateTime.UtcNow;
                return Board.GameStatus.Lost;
            }

            // --- 2. HANDLE SAFE TILE ----------------------------------
            current.IsVisited = true;

            if (current.NumberOfBombNeighbors == 0)
                FloodFill(board, row, col);

            // --- 3. CHECK FOR WIN -------------------------------------
            var state = board.DetermineGameState();
            board.CurrentStatus = state;

            if (state == Board.GameStatus.Won)
            {
                RevealEntireBoard(board);
                board.EndTime = DateTime.UtcNow;
            }

            return state;
        }

        public void ToggleFlag(Board board, int row, int col)
        {
            var cell = board.Cells[row, col];

            if (cell.IsVisited || board.CurrentStatus != Board.GameStatus.InProgress)
                return;

            cell.IsFlagged = !cell.IsFlagged;
        }

        // Save game
        public async Task SaveGameAsync(int userId, Board board)
        {
            var json = JsonConvert.SerializeObject(board);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"INSERT INTO dbo.Games (UserId, Seed, DateSaved, GameData) VALUES (@UserId, @Seed, @DateSaved, @GameData)", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Seed", board.GameId);
            cmd.Parameters.AddWithValue("@DateSaved", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@GameData", json);

            await cmd.ExecuteNonQueryAsync();
        }

        // Create a list of all saved games via API
        public async Task<List<SavedGameViewModel>> GetAllSavedGamesAsync()
        {
            var games = new List<SavedGameViewModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT Id, Seed, DateSaved FROM dbo.Games ORDER BY DateSaved DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new SavedGameViewModel
                {
                    Id = reader.GetInt32(0),
                    Seed = reader.GetInt32(1),
                    DateSaved = reader.GetDateTime(2)
                });
            }

            return games;
        }

        // Create a list of only one game regardless of user via API
        public async Task<object?> GetGameByIdAsync(int gameId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                "SELECT Id, Seed, DateSaved, GameData FROM dbo.Games WHERE Id = @Id",
                conn);

            cmd.Parameters.AddWithValue("@Id", gameId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new
            {
                Id = reader.GetInt32(0),
                Seed = reader.GetInt32(1),
                DateSaved = reader.GetDateTime(2),
                GameData = reader.GetString(3)
            };
        }

        // Create list of saved games for a specific user
        public async Task<List<SavedGameViewModel>> GetSavedGamesAsync(int userId)
        {
            var games = new List<SavedGameViewModel>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"SELECT Id, Seed, DateSaved FROM dbo.Games WHERE UserId = @UserId ORDER BY DateSaved DESC", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new SavedGameViewModel
                {
                    Id = reader.GetInt32(0),
                    Seed = reader.GetInt32(1),
                    DateSaved = reader.GetDateTime(2)
                });
            }

            return games;
        }

        // Retrieve game and deserialize using json to injection onto gameboard
        public async Task<Board?> LoadGameAsync(int gameId, int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@" SELECT GameData FROM dbo.Games WHERE Id = @Id AND UserId = @UserId", conn);

            cmd.Parameters.AddWithValue("@Id", gameId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var result = await cmd.ExecuteScalarAsync();

            if (result == null)
                return null;

            var json = result.ToString();
            return JsonConvert.DeserializeObject<Board>(json);
        }

        public async Task UpdateGameAsync(int gameId, int userId, Board board)
        {
            var json = JsonConvert.SerializeObject(board);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"UPDATE dbo.Games SET GameData = @GameData, DateSaved = @DateSaved WHERE Id = @GameId AND UserId = @UserId", conn);

            cmd.Parameters.AddWithValue("@GameId", gameId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@GameData", json);
            cmd.Parameters.AddWithValue("@DateSaved", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteGameAsync(int gameId, int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"DELETE FROM dbo.Games WHERE Id = @Id AND UserId = @UserId", conn);

            cmd.Parameters.AddWithValue("@Id", gameId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        // Delete a game via API
        public async Task<bool> DeleteGameByIdAsync(int gameId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                "DELETE FROM dbo.Games WHERE Id = @Id",
                conn);

            cmd.Parameters.AddWithValue("@Id", gameId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }


        // ------------------------------------------------------------
        // PRIVATE GAME RULE HELPERS - For CoreGameServices Only
        // ------------------------------------------------------------

        // Checks to determine if the seed already exists in the database.
        private async Task<bool> GameSeedExists(int seed)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT COUNT (1) FROM dbo.Games WHERE Seed = @Seed", conn);
            cmd.Parameters.AddWithValue("@Seed",  seed);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        private void FloodFill(Board board, int row, int col)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int newRow = row + dr;
                    int newCol = col + dc;

                    if (!board.IsCellOnBoard(newRow, newCol))
                        continue;

                    var neighbor = board.Cells[newRow, newCol];

                    if (!neighbor.IsVisited && !neighbor.IsBomb)
                    {
                        neighbor.IsVisited = true;

                        if (neighbor.NumberOfBombNeighbors == 0)
                            FloodFill(board, newRow, newCol);
                    }
                }
            }
        }

        private void RevealAllBombs(Board board)
        {
            for (int r = 0; r < board.Size; r++)
            {
                for (int c = 0; c < board.Size; c++)
                {
                    if (board.Cells[r, c].IsBomb)
                        board.Cells[r, c].IsVisited = true;
                }
            }
        }

        private void RevealEntireBoard(Board board)
        {
            for (int r = 0; r < board.Size; r++)
            {
                for (int c = 0; c < board.Size; c++)
                {
                    board.Cells[r, c].IsVisited = true;
                }
            }
        }
    }
}
