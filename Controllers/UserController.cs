using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Models;
using MineSweeper_MVC.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace MineSweeper_MVC.Controllers
{
    public class UserController : Controller
    {
        // Dependency injection of DatabaseContext
        private readonly DatabaseContext _db;

        // Constructor for dependency injection
        public UserController(DatabaseContext db)
        {
            _db = db;
        }

        // GET: /User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model) // Method takes RegisterViewModel as parameter which contains user registration data
        {
            if (!ModelState.IsValid)
                return View(model);

            // SqlConnection object to connect to the database
            using (SqlConnection conn = _db.GetConnection())
            {
                // Open the database connection
                conn.Open();

                // Check if email already exists
                string checkEmailSql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

                // SqlCommand object to execute the SQL query
                using (SqlCommand emailCmd = new SqlCommand(checkEmailSql, conn))
                {
                    emailCmd.Parameters.AddWithValue("@Email", model.Email);

                    // Execute the ExecuteScalar method that returns the first column of the first row in the result set
                    int emailCount = (int)emailCmd.ExecuteScalar();
                    if (emailCount > 0)
                    {
                        TempData["RegisterFail"] = "An account already exists. Please sign in.";
                        return RedirectToAction("Login");
                    }
                }

                // Check if username is already taken
                string checkUserSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using (SqlCommand userCmd = new SqlCommand(checkUserSql, conn))
                {
                    userCmd.Parameters.AddWithValue("@Username", model.Username);

                    // Execute the ExecuteScalar method that returns the first column of the first row in the result set
                    int userCount = (int)userCmd.ExecuteScalar();
                    if (userCount > 0)
                    {
                        ModelState.AddModelError("", "User name unavailable.");
                        return View(model);
                    }
                }

                // Insert new user
                string passwordHash = HashPassword(model.Password);
                string insertSql = @"INSERT INTO Users (FirstName, LastName, Sex, Age, State, Email, Username, PasswordHash)
                             VALUES (@FirstName, @LastName, @Sex, @Age, @State, @Email, @Username, @PasswordHash)";

                using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", model.LastName);
                    cmd.Parameters.AddWithValue("@Sex", model.Sex);
                    cmd.Parameters.AddWithValue("@Age", model.Age);
                    cmd.Parameters.AddWithValue("@State", model.State.ToString());
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["RegisterSuccess"] = "Registration successful. Please sign in.";
            return RedirectToAction("Login");
        }


        // GET: /User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /User/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model) // Method takes LoginViewModel as parameter which contains user login data
        {
            if (!ModelState.IsValid)
                return View(model);

            // Hash the input password
            string hashedInput = HashPassword(model.Password);

            // Retrieve stored password hash from the database
            string storedHash = null;

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();

                // SQL command to get the password hash for the given username
                string sql = "SELECT PasswordHash FROM Users WHERE Username = @un";

                // Create and execute the command
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@un", model.Username);

                    // The ExecuteScalar method returns the first column of the first row in the result set
                    object result = cmd.ExecuteScalar();

                    // If user exists, retrieve the password hash
                    if (result != null)
                    {
                        storedHash = result.ToString(); // Cast to string
                    }
                }
            }

            // If storedhas is null then user does not exist; or if password does not match...
            if (storedHash == null || storedHash != hashedInput)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Login successful - store session
            HttpContext.Session.SetString("Username", model.Username);

            // Login success — redirect for now
            return RedirectToAction("StartRedirect", "Home");
        }

        // GET: /User/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }


        /*
         * -----------------------------------------
         * Helper Methods
         * -----------------------------------------
         */

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
