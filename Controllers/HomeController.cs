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
    private readonly string mediaPath;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        mediaPath = Path.Combine(_environment.WebRootPath, MediaFolder);
    }

    private List<VideoFileModel> GetVideoFiles()
    {
        return Directory.GetFiles(this.mediaPath, "*.mp4")
            .Select(f => new FileInfo(f))
            .Select(f => new VideoFileModel
            {
                FileName = f.Name,
                FileSize = f.Length
            })
            .ToList();
    }

    public IActionResult Index()
    {
        var files = GetVideoFiles();
        return View(files);
    }

    [RequestSizeLimit(200 * 1024 * 1024)]
    [DisableRequestSizeLimit]
    [HttpPost]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        HttpContext.Response.StatusCode = StatusCodes.Status200OK;

        var response = new 
        { 
            success = true, 
            message = "Files uploaded successfully", 
        };

        if (files == null || files.Count == 0)
        {
            if (Request.ContentLength > 200 * 1024 * 1024)
            {
                response = new { success = false, message = "Files too large!" };
                HttpContext.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            }
            else
            {
                response = new { success = false, message = "No files!" };
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }

            foreach (var file in files)
            {
                if (file.ContentType != "video/mp4")
                {
                    response = new { success = false, message = $"File {file.FileName} is not Mp4 format" };
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    continue;
                }

                var filePath = Path.Combine(this.mediaPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

            }
        }

        return Json(response);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
