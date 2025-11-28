namespace MineSweeper_MVC.Models
{
    public class StartGameOptions
    {
        public int BoardSize { get; set; }
        public string DifficultyType { get; set; } = "Easy";
    }
}
