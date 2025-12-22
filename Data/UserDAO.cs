using Microsoft.Data.SqlClient;
using MineSweeper_MVC.Models;

namespace MineSweeper_MVC.Data
{
    public class UserDao : IUserDao

    {
        private readonly DatabaseContext _db;

        public UserDao(DatabaseContext db)
        {
            _db = db;
        }

        public bool EmailExists(string email)
        {
            return ExecuteCount(
                "SELECT COUNT(*) FROM dbo.Users WHERE Email = @Email",
                ("@Email", email)
            ) > 0;
        }

        public bool UsernameExists(string username)
        {
            return ExecuteCount(
                "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username",
                ("@Username", username)
            ) > 0;
        }

        public int CreateUser(RegisterViewModel model, string passwordHash)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            const string sql = @"
                INSERT INTO dbo.Users (FirstName, LastName, Sex, Age, State, Email, Username, PasswordHash)
                OUTPUT INSERTED.Id VALUES (@FirstName, @LastName, @Sex, @Age, @State, @Email, @Username, @PasswordHash)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
            cmd.Parameters.AddWithValue("@LastName", model.LastName);
            cmd.Parameters.AddWithValue("@Sex", model.Sex);
            cmd.Parameters.AddWithValue("@Age", model.Age);
            cmd.Parameters.AddWithValue("@State", model.State.ToString());
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Username", model.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            return (int)cmd.ExecuteScalar();
        }

        public (int Id, string PasswordHash)? GetLoginInfo(string username)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            const string sql = @"SELECT Id, PasswordHash FROM dbo.Users WHERE Username = @Username";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return (reader.GetInt32(0), reader.GetString(1));
        }

        public UserProfileViewModel GetProfileById(int userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            const string sql = @"SELECT Id, FirstName, LastName, Sex, Age, State, Email, Username FROM dbo.Users WHERE Id = @Id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", userId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new UserProfileViewModel
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Sex = reader.GetString(3),
                Age = reader.GetInt32(4),
                State = reader.GetString(5),
                Email = reader.GetString(6),
                Username = reader.GetString(7)
            };
        }

        public void UpdateProfile(UserProfileViewModel model)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            const string sql = @"UPDATE dbo.Users SET FirstName = @FirstName, LastName = @LastName, Sex = @Sex, Age = @Age, State = @State, Email = @Email WHERE Id = @Id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
            cmd.Parameters.AddWithValue("@LastName", model.LastName);
            cmd.Parameters.AddWithValue("@Sex", model.Sex);
            cmd.Parameters.AddWithValue("@Age", model.Age);
            cmd.Parameters.AddWithValue("@State", model.State.ToString());
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Id", model.Id);

            cmd.ExecuteNonQuery();
        }

        private int ExecuteCount(string sql, params (string, object)[] parameters)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            foreach (var (name, value) in parameters)
                cmd.Parameters.AddWithValue(name, value);

            return (int)cmd.ExecuteScalar();
        }
    }
}
