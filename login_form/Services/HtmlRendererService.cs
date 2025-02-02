using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace login_form.Services;

public class HtmlRendererService
{
    private readonly IWebHostEnvironment _env;

    public HtmlRendererService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> RenderHtmlAsync(string fileName, Dictionary<string, string> placeholders)
    {
        var filePath = Path.Combine(_env.WebRootPath, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {fileName} not found");

        var html = await File.ReadAllTextAsync(filePath);

        foreach (var placeholder in placeholders)
        {
            html = html.Replace(placeholder.Key, placeholder.Value);
        }

        return html;
    }
}
