using Microsoft.Data.SqlClient;
using BibleVerseApp.Models;

namespace BibleVerseApp.Data
{
    /// <summary>
    /// BibleDAO is responsible for all read-only database access
    /// related to Bible verse data across translations (AKJV, ASV).
    ///
    /// This class contains no business logic and no UI concerns.
    /// It simply executes SQL queries and maps results to Bible models.
    /// </summary>
    public class BibleDAO
    {
        private readonly string connectionString;

        public BibleDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves all verses for a given book and chapter
        /// from the specified translation table.
        /// </summary>
        public List<Bible> GetChapter(string translationTable, string book, int chapter)
        {
            ValidateTranslationTable(translationTable);

            List<Bible> verses = new();

            string sql = $@"SELECT Id, Book, Chapter, Verse, Text
                            FROM dbo.{translationTable}
                            WHERE Book = @Book AND Chapter = @Chapter
                            ORDER BY Verse;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@Book", book);
            cmd.Parameters.AddWithValue("@Chapter", chapter);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                verses.Add(MapBible(reader));
            }

            return verses;
        }

        /// <summary>
        /// Retrieves a single verse by its global VerseId
        /// from the specified translation table.
        /// </summary>
        public Bible GetVerseById(string translationTable, int verseId)
        {
            ValidateTranslationTable(translationTable);

            string sql = $@"SELECT Id, Book, Chapter, Verse, Text
                            FROM dbo.{translationTable}
                            WHERE Id = @Id;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@Id", verseId);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return MapBible(reader);
            }

            return null;
        }

        /// <summary>
        /// Searches for verses containing the given search term
        /// within the specified translation table.
        /// </summary>
        public List<Bible> SearchVerses(string translationTable, string searchTerm)
        {
            ValidateTranslationTable(translationTable);

            List<Bible> verses = new();

            string sql = $@"SELECT Id, Book, Chapter, Verse, Text
                            FROM dbo.{translationTable}
                            WHERE Text LIKE @SearchTerm
                            ORDER BY Book, Chapter, Verse;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                verses.Add(MapBible(reader));
            }

            return verses;
        }

        /// <summary>
        /// Maps a SqlDataReader row to a Bible model.
        /// Centralized to avoid duplication and keep mapping consistent.
        /// </summary>
        private static Bible MapBible(SqlDataReader reader)
        {
            return new Bible
            {
                Id = reader.GetInt32(0),
                Book = reader.GetString(1),
                Chapter = reader.GetInt32(2),
                Verse = reader.GetInt32(3),
                Text = reader.GetString(4)
            };
        }

        /// <summary>
        /// Ensures only known translation tables are queried.
        /// This prevents accidental misuse and guards against SQL injection
        /// through dynamic table names.
        /// </summary>
        private static void ValidateTranslationTable(string translationTable)
        {
            if (translationTable != "AKJV" && translationTable != "ASV")
            {
                throw new ArgumentException("Invalid translation table.");
            }
        }
    }
}
