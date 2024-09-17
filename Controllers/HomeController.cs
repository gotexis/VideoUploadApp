using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VideoUploadApp.Models;
using System.IO;

namespace VideoUploadApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _environment;
    private const string MediaFolder = "media";

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public IActionResult Index()
    {
        var mediaPath = Path.Combine(_environment.WebRootPath, MediaFolder);
        var files = Directory.GetFiles(mediaPath, "*.mp4")
            .Select(f => new FileInfo(f))
            .Select(f => new VideoFileModel
            {
                FileName = f.Name,
                FileSize = f.Length
            })
            .ToList();

        return View(files);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        var mediaPath = Path.Combine(_environment.WebRootPath, MediaFolder);

        if (!Directory.Exists(mediaPath))
        {
            Directory.CreateDirectory(mediaPath);
        }

        foreach (var file in files)
        {
            if (file.ContentType != "video/mp4")
            {
                ModelState.AddModelError("", $"File {file.FileName} is not an MP4 file.");
                continue;
            }

            if (file.Length > 200 * 1024 * 1024) // 200 MB limit
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 200 MB limit.");
                continue;
            }

            var filePath = Path.Combine(mediaPath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        if (!ModelState.IsValid)
        {
            return View("Index");
        }

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
