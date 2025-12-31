using BibleVerseApp.Models;

namespace BibleVerseApp.Data.Interfaces
{
    public interface INotesDAO
    {
        /// <summary>
        /// Retrieves all notes created by a specific user.
        /// </summary>
        List<Note> GetNotesByUser(int userId);

        /// <summary>
        /// Retrieves all notes associated with a specific verse.
        /// </summary>
        List<Note> GetNotesByVerse(int verseId);

        /// <summary>
        /// Creates a new note for a verse.
        /// </summary>
        void AddNote(Note note);

        /// <summary>
        /// Updates the content of an existing note.
        /// </summary>
        void UpdateNote(Note note);

        /// <summary>
        /// Deletes a note by its Id.
        /// </summary>
        void DeleteNote(int noteId);
    }
}
