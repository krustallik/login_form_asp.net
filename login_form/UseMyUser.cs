using login_form.Models;
using System.Text.Json;

namespace login_form;

public static class UseMyUserExtension
{
    public static IApplicationBuilder UseMyUser(this IApplicationBuilder builder)
    {
        builder.Use(async (context, next) =>
        {
            var pagesForAuthorized = new List<string>() { "/", "/logout", "/page2" };

            if (!pagesForAuthorized.Contains(context.Request.Path))
            {
                await next.Invoke();
                return;
            }

            // Check user in session
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