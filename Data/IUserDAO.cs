using BibleVerseApp.Models;

namespace BibleVerseApp.Data.Interfaces
{
    public interface IUserDAO
    {
        /// <summary>
        /// Retrieves a user by Id.
        /// </summary>
        User GetUserById(int userId);

        /// <summary>
        /// Retrieves a user by username.
        /// </summary>
        User GetUserByUsername(string username);

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        void CreateUser(User user);
    }
}
