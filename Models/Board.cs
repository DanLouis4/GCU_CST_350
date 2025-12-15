using System.Text.Json.Serialization;

namespace MineSweeper_MVC.Models
{
    public class Board
    {
        public int GameId { get; set; }
        public int Size { get; set; }
        public float Difficulty { get; set; }
        public string DifficultyType { get; set; }
        public Cell[,] Cells { get; set; }
        public int FlagsPlaced => Cells.Cast<Cell>().Count(c => c.IsFlagged);    
        public int DetectorOwned { get; set; }
        public int DetectorFound { get; set; }
        public int RadarOwned { get; set; }
        public int RadarFound { get; set; }
        public int TotalDetectors { get; private set; }
        public int TotalRadar { get; private set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int BombCount { get; set; }
        public enum GameStatus { InProgress, Won, Lost }
        public GameStatus CurrentStatus { get; set; } = GameStatus.InProgress;

        Random random = new Random();

        public Board(int size, float difficulty, string difficultyType)
        {

            Size = size;
            Difficulty = difficulty;
            DifficultyType = difficultyType;
            Cells = new Cell[size, size]; // Creates the array

            // This loop initializes each Cell in the 2D array
            for (int row = 0; row < Size; row++) // Iterates through each row
            {
                for (int col = 0; col < Size; col++)
                {
                    Cells[row, col] = new Cell(row, col); // Instantiates each Cell object
                }
            }
        }

        // Used during setup to place bombs on the board
        public void SetupBombs()
        {
            int bombsOnBoard = (int)(Size * Size * Difficulty);
            BombCount = 0;

            while (BombCount < bombsOnBoard)
            {
                int row = random.Next(Size);
                int col = random.Next(Size);

                if (!Cells[row, col].IsBomb)
                {
                    Cells[row, col].IsBomb = true;
                    BombCount++;
                }
            }
        }

        // Retrieves the total number of bombs on the board.
        public int GetBombCount()
        {
            return BombCount;
        }

        // Used during setup to place rewards on the board
        public void SetupRewards()
        {
            int detectorCount = 0;
            int availableCells = Size * Size - BombCount;

            if (DifficultyType == "Easy") return; // No rewards on Easy difficulty            

            if (DifficultyType == "Normal")
            {
               // detectorCount = availableCells / 20;
               return; // No rewards on Normal difficulty
            }
            else if (DifficultyType == "Hard")
            {
                // detectorCount = availableCells / 15;
                return; // No rewards on Hard difficulty
            }

            int radarCount = (DifficultyType == "") ? 1 : 0; // 0 radar

            int placeDetectors = 0;

            while (placeDetectors < detectorCount)
            {
                int row = random.Next(Size);
                int col = random.Next(Size);

                if (!Cells[row, col].IsBomb && Cells[row, col].Reward == Cell.RewardType.None)
                {
                    Cells[row, col].Reward = Cell.RewardType.Detector;
                    placeDetectors++;
                }
            }

            TotalDetectors = detectorCount; // Track total detectors

            int placeRadars = 0;

            if (!DifficultyType.Equals("")) return; // No radars available

            while (placeRadars < radarCount)
            {
                int row = random.Next(Size);
                int col = random.Next(Size);
                if (!Cells[row, col].IsBomb && Cells[row, col].Reward == Cell.RewardType.None)
                {
                    Cells[row, col].Reward = Cell.RewardType.Radar;
                    placeRadars++;
                }
            }
            TotalRadar = radarCount; // Track total detectors
        }

        // Use during setup to calculate the number of bomb neighbors for each cell
        public void CountBombNearby()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Cells[row, col].NumberOfBombNeighbors = GetNumberOfBombNeighbors(row, col);
                }
            }
        }

        // Helper function to determine the number of bomb neighbors for a cell
        public int GetNumberOfBombNeighbors(int row, int col)
        {
            int bombCount = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;
                    if (IsCellOnBoard(newRow, newCol) && Cells[newRow, newCol].IsBomb)
                    {
                        bombCount++;
                    }
                }
            }
            return bombCount;
        }

        // Helper function to determine if a cell is out of bounds
        public bool IsCellOnBoard(int row, int col)
        {
            return row >= 0 && row < Size && col >= 0 && col < Size;
        }

        // Used every turn to determine the current game state
        public GameStatus DetermineGameState()
        {
            foreach (var cell in Cells)
            {
                if (!cell.IsBomb && !cell.IsVisited)
                    return GameStatus.InProgress;
            }
            return GameStatus.Won;
        }

        // Used when the player selects a cell and chooses to play the reward
        public bool UseDetectorReward(Cell cell)
        {
            if (cell.IsBomb)
            {
                cell.IsDeactivated = true;
                FeedbackHandler?.Invoke("ALERT! Bomb Detected! Bomb Deactivated!");
            }
            else
            {
                FeedbackHandler?.Invoke("No Bomb Detected.");
            }

            cell.IsVisited = true;
            DetectorOwned--;
            return true;
        }

        public bool UseRadarReward(Cell cell)
        {

            FeedbackHandler?.Invoke("Radar Activated! Revealing surrounding cells...");
            RevealSurroundingCells(cell.Row, cell.Column);
            RadarOwned--;
            return true;
        }

        // Used when the player uses the radar to reveal surrounding cells
        public void RevealSurroundingCells(int row, int col)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    int newRow = row + dr;
                    int newCol = col + dc;

                    if (IsCellOnBoard(newRow, newCol))
                    {
                        var cell = Cells[newRow, newCol];

                        if (!cell.IsVisited)
                        {
                            cell.IsVisited = true;
                            cell.FloodFilled = true;

                            // Handle reward tile revealed by radar (not collected)
                            if (cell.Reward == Cell.RewardType.Detector)
                            {
                                DetectorFound++;
                            }
                            else if (cell.Reward == Cell.RewardType.Radar)
                            {
                                RadarFound++;
                            }

                            // Mark bombs as deactivated for visual purposes
                            if (cell.IsBomb)
                            {
                                cell.IsDeactivated = true;
                            }
                        }
                    }
                }
            }
        }

        [JsonIgnore]
        public Action<string>? FeedbackHandler; // Event to handle feedback messages

        // Used after game is over to calculate final score
        public int DetermineFinalScore(GameStatus status)
        {
            int score = 0;

            // Add points for every visited non-bomb cell
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (Cells[row, col].IsVisited && !Cells[row, col].IsBomb)
                    {
                        score += 10;
                    }
                }
            }

            // Penalize if the game was lost
            if (status == GameStatus.Lost)
            {
                score -= 100;
            }

            // Reward unused Detector and Radar bonuses
            score += DetectorOwned * 2;
            score += RadarOwned * 10;

            // Time penalty
            if (StartTime != default)
            {
                TimeSpan duration = DateTime.Now - StartTime;
                score -= (int)duration.TotalSeconds;
            }

            return Math.Max(score, 0); // Prevent negative scores
        }
    }
}
