using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

// Створюємо хеш пароля (це буде симуляція, у реальному випадку зберігайте цей хеш у базі даних)
var passwordHasher = new PasswordHasher<object>();
var storedHashedPassword = passwordHasher.HashPassword(null, "1234"); // Хеш пароля "1234"

app.MapGet("/", async context =>
{
    var service = context.RequestServices.GetService<IWebHostEnvironment>();
    var wwwRootPath = service.WebRootPath;
    var html = File.ReadAllText(Path.Combine(wwwRootPath, "index.html"));
    html = html.Replace("{message}", "");
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

app.MapPost("/", async context =>
{
    // Читання даних
    var username = context.Request.Form["username"];
    var password = context.Request.Form["password"];

    // Ініціалізація повідомлення
    var message = "";

    // Перевірка імені користувача
    var isNameTrue = username == "test";

    // Перевірка пароля (порівнюємо введений пароль із хешем)
    var result = passwordHasher.VerifyHashedPassword(null, storedHashedPassword, password);
    var isPasswordTrue = result == PasswordVerificationResult.Success;

    // Логіка для вибору повідомлення
    if (isNameTrue && isPasswordTrue)
    {
        message = "Your input data is correct.";
    }
    else if (isNameTrue && !isPasswordTrue)
    {
        message = "The password is incorrect.";
    }
    else if (!isNameTrue && isPasswordTrue)
    {
        message = "The username may be incorrect.";
    }
    else
    {
        message = "Both username and password are incorrect.";
    }

    // Дістаємо шлях до HTML
    var service = context.RequestServices.GetService<IWebHostEnvironment>();
    var wwwRootPath = service.WebRootPath;

    var html = File.ReadAllText(Path.Combine(wwwRootPath, "index.html"));

    // Заміна повідомлення
    html = html.Replace("{message}", message);

    // Відповідь
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

app.Run();
