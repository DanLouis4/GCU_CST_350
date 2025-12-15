namespace MineSweeper_MVC.Models
{
    public class CellViewModel
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Cell Cell { get; set; }
        public bool IsFlagged { get; set; }
        public Board.GameStatus GameState { get; set; }

    }
}
