namespace BibleVerseApp.Models
{
    // Represents a user-authored note attached to a specific Bible verse.
    // Notes are verse-specific, not translation-specific.
    // Because VerseIds are aligned across translations, the same note
    // can be displayed alongside any Bible version.
    public class Note
    {
        // Primary key for the note.
        public int Id { get; set; }

        // Foreign key reference to the verse.
        // This maps to the Id column in any translation table (AKJV, ASV).
        public int VerseId { get; set; }

        // Foreign key reference to the user who created the note.
        public int UserId { get; set; }

        // The actual note content written by the user.
        public string Content { get; set; }

        // Timestamp indicating when the note was created.
        // Defaults to the time of insertion.
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
