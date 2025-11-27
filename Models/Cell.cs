namespace MineSweeper_MVC.Models
{
    public class Cell
    {
        public int Row { get; set; } = -1;
        public int Column { get; set; } = -1;
        public bool IsVisited { get; set; } = false;
        public bool IsBomb { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        public int NumberOfBombNeighbors { get; set; } = 0;
        public enum RewardType { None, Detector, Radar }
        public RewardType Reward { get; set; } = RewardType.None;
        public bool IsKillerBomb { get; set; } = false;
        public bool IsDeactivated { get; set; } = false;
        public bool FloodFilled { get; set; } = false;

        public Cell(int row, int col)
        {
            Row = row;
            Column = col;
        }
        public override string ToString()
        {
            return $"Cell[{Row},{Column}]";
        }
    }
}
