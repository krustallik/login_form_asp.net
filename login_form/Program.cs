using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

// ��������� ��� ������ (�� ���� ���������, � ��������� ������� ��������� ��� ��� � ��� �����)
var passwordHasher = new PasswordHasher<object>();
var storedHashedPassword = passwordHasher.HashPassword(null, "1234"); // ��� ������ "1234"

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
    // ������� �����
    var username = context.Request.Form["username"];
    var password = context.Request.Form["password"];

    // ����������� �����������
    var message = "";

    // �������� ���� �����������
    var isNameTrue = username == "test";

    // �������� ������ (��������� �������� ������ �� �����)
    var result = passwordHasher.VerifyHashedPassword(null, storedHashedPassword, password);
    var isPasswordTrue = result == PasswordVerificationResult.Success;

    // ����� ��� ������ �����������
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

    // ĳ����� ���� �� HTML
    var service = context.RequestServices.GetService<IWebHostEnvironment>();
    var wwwRootPath = service.WebRootPath;

    var html = File.ReadAllText(Path.Combine(wwwRootPath, "index.html"));

    // ����� �����������
    html = html.Replace("{message}", message);

    // ³������
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

app.Run();
