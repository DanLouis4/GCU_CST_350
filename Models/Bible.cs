namespace BibleVerseApp.Models
{
    // Represents a single Bible verse record.
    // This model maps directly to a verse row in a translation table (AKJV, ASV).
    // The Id is the canonical VerseId and is intentionally aligned across all translations.
    public class Bible
    {
        // Primary key for the verse.
        // This is the VerseId used to associate notes across translations.
        public int Id { get; set; }

        // The book name (e.g., Genesis, John).
        // Stored as text to preserve canonical naming and readability.
        public string Book { get; set; }

        // Chapter number within the book.
        public int Chapter { get; set; }

        // Verse number within the chapter.
        public int Verse { get; set; }

        // The full verse text for the selected translation.
        public string Text { get; set; }
    }
}