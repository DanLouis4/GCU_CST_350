using MineSweeper_MVC.Data;
using MineSweeper_MVC.Models;
using MineSweeper_MVC.Services;
using System.Security.Cryptography;
using System.Text;

public class UserService : IUserService
{
    private readonly IUserDao _userDao;

    public UserService(IUserDao userDao)
    {
        _userDao = userDao;
    }

    public void Register(RegisterViewModel model)
    {
        if (_userDao.EmailExists(model.Email))
            throw new InvalidOperationException("Email already exists.");

        if (_userDao.UsernameExists(model.Username))
            throw new InvalidOperationException("Username unavailable.");

        var hash = HashPassword(model.Password);
        _userDao.CreateUser(model, hash);
    }

    public int? Login(LoginViewModel model)
    {
        var login = _userDao.GetLoginInfo(model.Username);
        if (login == null) return null;

        var hash = HashPassword(model.Password);
        return login.Value.PasswordHash == hash
            ? login.Value.Id
            : null;
    }

    public UserProfileViewModel GetProfile(int userId)
        => _userDao.GetProfileById(userId);

    public void UpdateProfile(UserProfileViewModel model)
    {
        // Username intentionally immutable
        _userDao.UpdateProfile(model);
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(password))
        );
    }
}
