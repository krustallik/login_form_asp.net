using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Час життя сесії
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();
app.UseSession();

var rootpath = builder.Environment.ContentRootPath;
string path = Path.Combine(rootpath, "database.txt");

app.MapGet("/", async context =>
{
    // Перевіряємо наявність cookie
    var isAuthorized = context.Request.Cookies["authorized"] != null;

    if (!isAuthorized)
    {
        context.Response.Redirect("/login");
        return;
    }

    // Отримуємо логін і пароль з сесії
    string login = context.Session.GetString("login") ?? "Unknown";
    string password = context.Session.GetString("password") ?? "Unknown";

    var service = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
    var wwwRootPath = service.WebRootPath;
    var filePath = Path.Combine(wwwRootPath, "authorized.html");

    if (File.Exists(filePath))
    {
        var html = await File.ReadAllTextAsync(filePath);
        html = html.Replace("{login}", login);
        html = html.Replace("{password}", password);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("File not found.");
    }
});

app.MapGet("/login", async context =>
{
    var service = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
    var wwwRootPath = service.WebRootPath;
    var filePath = Path.Combine(wwwRootPath, "login.html");

    if (File.Exists(filePath))
    {
        var html = await File.ReadAllTextAsync(filePath);
        html = html.Replace("{message}", "");
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("File not found.");
    }
});

app.MapPost("/login", async context =>
{
    var login = context.Request.Form["login"];
    var password = context.Request.Form["password"];
    string errorMessage = "Invalid login or password"; // Значення за замовчуванням

    if (!File.Exists(path))
    {
        File.Create(path).Close();
    }

    bool isAuthorized = false;

    foreach (var line in File.ReadLines(path))
    {
        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            string storedLogin = parts[0];
            string storedPassword = parts[1];

            if (storedLogin == login && storedPassword == password)
            {
                isAuthorized = true;
                break;
            }
            else if (storedLogin == login)
            {
                errorMessage = "Password is wrong"; // Якщо логін є, але пароль неправильний
                break;
            }
        }
    }

    if (isAuthorized)
    {
        context.Response.Cookies.Append("authorized", "true"); // Збереження сесії в кукі
        context.Session.SetString("login", login);
        context.Session.SetString("password", password);
        context.Response.Redirect("/");
        return;
    }
    else
    {
        string encodedMessage = HttpUtility.UrlEncode(errorMessage);
        context.Response.Redirect($"/error?message={encodedMessage}");
        return;
    }
});

app.MapGet("/signup", async context =>
{
    var service = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
    var wwwRootPath = service.WebRootPath;
    var filePath = Path.Combine(wwwRootPath, "signup.html");

    if (File.Exists(filePath))
    {
        var html = await File.ReadAllTextAsync(filePath);
        html = html.Replace("{message}", "");
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("File not found.");
    }
});

app.MapPost("/signup", async context =>
{
    var login = context.Request.Form["login"];
    var password = context.Request.Form["password"];
    string errorMessage = "User already exists"; // Повідомлення при наявності користувача

    if (!File.Exists(path))
    {
        File.Create(path).Close();
    }

    bool userExists = false;

    foreach (var line in File.ReadLines(path))
    {
        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && parts[0] == login)
        {
            userExists = true;
            break;
        }
    }

    if (userExists)
    {
        errorMessage = "User already exists";
        string encodedMessage = HttpUtility.UrlEncode(errorMessage);
        context.Response.Redirect($"/error?message={encodedMessage}");
        return;
    }

    using (StreamWriter sw = new StreamWriter(path, true))
    {
        sw.WriteLine($"{login} {password}");
    }

    context.Response.Cookies.Append("authorized", "true"); // Збереження сесії в кукі
    context.Session.SetString("login", login);
    context.Session.SetString("password", password);
    context.Response.Redirect("/");
});

app.MapGet("/error", async context =>
{
    var service = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
    var wwwRootPath = service.WebRootPath;
    var filePath = Path.Combine(wwwRootPath, "error.html");

    string message = context.Request.Query["message"]; // Отримуємо параметр помилки

    if (File.Exists(filePath))
    {
        var html = await File.ReadAllTextAsync(filePath);
        html = html.Replace("{message}", message);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Error page not found.");
    }
});

app.MapGet("/logout", async context =>
{
    // Очистити сесію
    context.Session.Clear();

    // Видалити кукі
    context.Response.Cookies.Delete("authorized");

    // Перенаправити на сторінку входу
    context.Response.Redirect("/login");
});


app.Run();
