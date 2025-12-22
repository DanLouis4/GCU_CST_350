using MineSweeper_MVC.Models;

namespace MineSweeper_MVC.Services
{
    public interface IUserService
    {
        void Register(RegisterViewModel model);
        int? Login(LoginViewModel model);

        UserProfileViewModel GetProfile(int userId);
        void UpdateProfile(UserProfileViewModel model);
    }


}
