namespace BibleVerseApp.Models
{
    // Represents an application user.
    // Users own notes and authenticate against the system.
    public class User
    {
        // Primary key for the user.
        public int Id { get; set; }

        // Public-facing username.
        public string Username { get; set; }

        // User email address. Useful for expandability features such as email notifications, etc.
        public string Email { get; set; }

        // Hashed password value.
        // Raw passwords are never stored.
        public string PasswordHash { get; set; }

        // Timestamp indicating when the account was created.
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
