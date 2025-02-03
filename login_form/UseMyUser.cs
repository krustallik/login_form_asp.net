using login_form.Models;
using System.Text.Json;

public static class UseMyUserExtension
{
    public static IApplicationBuilder UseMyUser(this IApplicationBuilder builder)
    {
        builder.Use(async (context, next) =>
        {
            var pagesForAuthorized = new List<string>() { "/", "/logout", "/technologies" };

            if (!pagesForAuthorized.Contains(context.Request.Path))
            {
                await next.Invoke();
                return;
            }

            // Перевірка користувача в сесії
            MyUser myUser = null;
            try
            {
                myUser = JsonSerializer.Deserialize<MyUser>(context.Session.GetString("user"));
                context.Items["MyUser"] = myUser;
                await next.Invoke();
            }
            catch (Exception ex)
            {
                context.Response.Redirect("/login");
                return;
            }
        });
        return builder;
    }
}
