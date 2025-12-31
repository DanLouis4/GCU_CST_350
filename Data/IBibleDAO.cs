using BibleVerseApp.Models;

namespace BibleVerseApp.Data.Interfaces
{
    public interface IBibleDAO
    {
        /// <summary>
        /// Retrieves all verses for a given book and chapter
        /// from the specified translation table (AKJV, ASV).
        /// </summary>
        List<Bible> GetChapter(string translationTable, string book, int chapter);

        /// <summary>
        /// Retrieves a single verse by its global VerseId
        /// from the specified translation table.
        /// </summary>
        Bible GetVerseById(string translationTable, int verseId);

        /// <summary>
        /// Searches verses containing the specified search term
        /// within the selected translation table.
        /// </summary>
        List<Bible> Search(string translationTable, string searchTerm);
    }
}
