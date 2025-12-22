namespace MineSweeper_MVC.Models
{
    public class SavedGameViewModel
    {
        public int Id { get; set; }
        public int Seed { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public string Difficulty { get; set; }
        public DateTime DateSaved { get; set; }
    }

}
