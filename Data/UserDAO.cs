using Microsoft.Data.SqlClient;
using BibleVerseApp.Models;

namespace BibleVerseApp.Data
{
    /// <summary>
    /// UserDAO is responsible for all database operations
    /// related to application users.
    ///
    /// This class handles user persistence only:
    /// creating users, retrieving users, and validating
    /// login credentials.
    ///
    /// It contains no authentication logic, hashing strategy,
    /// or UI concerns. Those belong higher up the stack.
    /// </summary>
    public class UserDAO
    {
        private readonly string connectionString;

        public UserDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves a single user by their unique Id.
        /// Used when loading profile data or ownership checks.
        /// </summary>
        public User GetById(int userId)
        {
            User user = null;

            string sql = @"SELECT Id, Username, Email, PasswordHash, CreatedAt
                           FROM dbo.Users
                           WHERE Id = @UserId;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@UserId", userId);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                user = MapUser(reader);
            }

            return user;
        }

        /// <summary>
        /// Retrieves a single user by username.
        /// Used during login and account checks.
        /// </summary>
        public User GetByUsername(string username)
        {
            User user = null;

            string sql = @"SELECT Id, Username, Email, PasswordHash, CreatedAt
                           FROM dbo.Users
                           WHERE Username = @Username;";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@Username", username);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                user = MapUser(reader);
            }

            return user;
        }

        /// <summary>
        /// Creates a new user record in the database.
        /// Assumes the password has already been hashed.
        /// </summary>
        public int Create(User user)
        {
            string sql = @"INSERT INTO dbo.Users (Username, Email, PasswordHash, CreatedAt)
                           VALUES (@Username, @Email, @PasswordHash, @CreatedAt);
                           SELECT SCOPE_IDENTITY();";

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Internal mapper that converts a SqlDataReader
        /// row into a User domain object.
        ///
        /// This keeps mapping logic centralized and consistent.
        /// </summary>
        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }
    }
}
