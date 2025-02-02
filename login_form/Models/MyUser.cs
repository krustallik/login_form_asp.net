namespace login_form.Models;

public class MyUser
{

    public MyUser() {}
    public MyUser(string Name, string Email, string PasswordHash) {
        this.Name = Name;
        this.Email = Email;
        this.PasswordHash = PasswordHash;
    }


    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}
