using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Web;
using login_form;
using login_form.Models;
using login_form.Services;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// ���� ���������� ���'��� ��� ��������� ����
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ��� ����� ��� - 30 ������
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ������ UserService �� ��������-�����
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<HtmlRendererService>();

var app = builder.Build();

app.UseStaticFiles(); // ���� �������� ��������� �����
app.UseSession(); // ������ �������� ����
app.UseMyUser();

var userService = app.Services.GetRequiredService<UserService>();

var htmlRenderer = app.Services.GetRequiredService<HtmlRendererService>();



// ������� ������� � ��������� �����������
app.MapGet("/", async context =>
{
    if (!context.Items.ContainsKey("MyUser"))
    {
        context.Response.Redirect("/login");
        return;
    }

    var myUser = (MyUser)context.Items["MyUser"];

    var html = await htmlRenderer.RenderHtmlAsync("authorized.html", new Dictionary<string, string>
    {
        { "{login}", myUser.Name },
        { "{password}", myUser.PasswordHash }
    });

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

// ³���������� ������� �����
app.MapGet("/login", async context =>
{
    var html = await htmlRenderer.RenderHtmlAsync("login.html", new Dictionary<string, string>
    {
        { "{message}", "" }
    });

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

// ������� ����������� �����������
app.MapPost("/login", async context =>
{
    var login = context.Request.Form["login"];
    var password = context.Request.Form["password"];
    string errorMessage = "Invalid login or password";

    if (userService.VerifyPassword(login, password))
    {
        var myUser = new MyUser(login, login, password);
        context.Session.SetString("user", JsonSerializer.Serialize(myUser));
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

// ³���������� ������� ���������
app.MapGet("/signup", async context =>
{
    var message = "";
    var html = await htmlRenderer.RenderHtmlAsync("signup.html", new Dictionary<string, string>
    {
        { "{message}", message }
    });

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

// ������� ��������� ������ �����������
app.MapPost("/signup", async context =>
{
    var login = context.Request.Form["login"];
    var password = context.Request.Form["password"];
    string errorMessage = "User already exists";

    if (userService.FindByLogin(login) != null)
    {
        string encodedMessage = HttpUtility.UrlEncode(errorMessage);
        context.Response.Redirect($"/error?message={encodedMessage}");
        return;
    }

    userService.Add(new MyUser(login, login, password));

    context.Response.Cookies.Append("authorized", "true");
    context.Session.SetString("login", login);
    context.Session.SetString("password", password);
    context.Response.Redirect("/");
});

// ³���������� ������� �������
app.MapGet("/error", async context =>
{
    string message = context.Request.Query["message"];

    var html = await htmlRenderer.RenderHtmlAsync("error.html", new Dictionary<string, string>
    {
        { "{message}", message }
    });

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});

// ������� ������ �����������
app.MapGet("/logout", async context =>
{
    context.Session.Clear();
    context.Response.Redirect("/login");
});

app.Run();
