using MineSweeper_MVC.Models;

namespace MineSweeper_MVC.Data
{
    public interface IUserDao
    {
        bool EmailExists(string email);
        bool UsernameExists(string username);

        int CreateUser(RegisterViewModel model, string passwordHash);

        (int Id, string PasswordHash)? GetLoginInfo(string username);

        UserProfileViewModel GetProfileById(int userId);

        void UpdateProfile(UserProfileViewModel model);
    }
}
