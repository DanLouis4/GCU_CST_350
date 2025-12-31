using Microsoft.Data.SqlClient;
using BibleVerseApp.Models;

namespace BibleVerseApp.Data
{
    /// <summary>
    /// NotesDAO is responsible for all READ and WRITE database access
    /// related to user-created verse notes.
    ///
    /// This class does not concern itself with Bible text, translations,
    /// or display logic. Its sole responsibility is persisting and
    /// retrieving notes tied to a specific VerseId and UserId.
    /// </summary>
    public class NotesDAO
    {
        private readonly string connectionString;

        public NotesDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves all notes created by a specific user.
        /// Notes are returned in descending order of creation,
        /// allowing the most recent reflections to appear first.
        /// </summary>
        public List<Note> GetNotesByUser(int userId)
        {
            List<Note> notes = new();

            string sql = @"SELECT Id, VerseId, UserId, Content, CreatedAt
                           FROM dbo.Notes
                           WHERE UserId = @UserId
                           ORDER BY CreatedAt DESC;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@UserId", userId);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                notes.Add(MapNote(reader));
            }

            return notes;
        }

        /// <summary>
        /// Retrieves all notes written by a user for a specific verse.
        /// This allows verse-centric views where notes remain
        /// translation-neutral.
        /// </summary>
        public List<Note> GetNotesForVerse(int userId, int verseId)
        {
            List<Note> notes = new();

            string sql = @"SELECT Id, VerseId, UserId, Content, CreatedAt
                           FROM dbo.Notes
                           WHERE UserId = @UserId
                             AND VerseId = @VerseId
                           ORDER BY CreatedAt ASC;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@VerseId", verseId);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                notes.Add(MapNote(reader));
            }

            return notes;
        }

        /// <summary>
        /// Inserts a new note for a given verse and user.
        /// Notes are immutable at creation and timestamped
        /// at the database boundary.
        /// </summary>
        public void AddNote(Note note)
        {
            string sql = @"INSERT INTO dbo.Notes (VerseId, UserId, Content, CreatedAt)
                           VALUES (@VerseId, @UserId, @Content, @CreatedAt);";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@VerseId", note.VerseId);
            cmd.Parameters.AddWithValue("@UserId", note.UserId);
            cmd.Parameters.AddWithValue("@Content", note.Content);
            cmd.Parameters.AddWithValue("@CreatedAt", note.CreatedAt);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates the content of an existing note.
        /// This allows refinement without altering
        /// verse association or authorship.
        /// </summary>
        public void UpdateNote(Note note)
        {
            string sql = @"UPDATE dbo.Notes
                           SET Content = @Content
                           WHERE Id = @Id
                             AND UserId = @UserId;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@Id", note.Id);
            cmd.Parameters.AddWithValue("@UserId", note.UserId);
            cmd.Parameters.AddWithValue("@Content", note.Content);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes a note owned by a user.
        /// Ownership is enforced at the query level
        /// to prevent cross-user modification.
        /// </summary>
        public void DeleteNote(int noteId, int userId)
        {
            string sql = @"DELETE FROM dbo.Notes
                           WHERE Id = @Id
                             AND UserId = @UserId;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@Id", noteId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Maps a SqlDataReader row to a Note model.
        /// Centralizing this logic keeps the DAO readable
        /// and consistent.
        /// </summary>
        private static Note MapNote(SqlDataReader reader)
        {
            return new Note
            {
                Id = reader.GetInt32(0),
                VerseId = reader.GetInt32(1),
                UserId = reader.GetInt32(2),
                Content = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }
    }
}
