using login_form.Models;
using System.Text.Json;

namespace login_form.Services;

public class UserService
{

    public UserService()
    {
        Load();
    }

    private string userDataFile = "users.json";

    private List<MyUser> Users { get; set; } = new List<MyUser>();

    public void Load()
    {
        if (File.Exists(userDataFile))
        {
            Users = JsonSerializer.Deserialize<List<MyUser>>(File.ReadAllText(userDataFile));
        }
    }

    public void Add(MyUser user)
    {
        Users.Add(user);
        SaveChanges();
    }

    public MyUser? FindByLogin(string login)
    {
        return Users.FirstOrDefault(x => x.Name == login);
    }

    public void ResetPassword(string login, string newPassword)
    {
        var user = FindByLogin(login);
        if (user != null)
        {
            user.PasswordHash = newPassword;
            SaveChanges();
        }
    }

    public bool VerifyPassword(string login, string password)
    {
        return FindByLogin(login)?.PasswordHash == password;
    }

    private void SaveChanges()
    {
        File.WriteAllText(userDataFile, JsonSerializer.Serialize(Users));
    }
}
