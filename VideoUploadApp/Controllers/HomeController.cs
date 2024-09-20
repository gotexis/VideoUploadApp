using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VideoUploadApp.Models;
using System.IO;
using System.IO.Abstractions;

namespace VideoUploadApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
        _fileSystem = new FileSystem();
    }

    private List<VideoFileModel> GetVideoFiles()
    {
        return Directory.GetFiles("media", "*.mp4")
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
        var mediaPath = _fileSystem.Path.Combine(_environment.WebRootPath, "media");
        var files = _fileSystem.Directory.GetFiles(mediaPath, "*.mp4", SearchOption.AllDirectories);
        var videoFiles = files
            .Select(f => new VideoFileModel
            {
                FileName = _fileSystem.Path.GetFileName(f),
                FileSize = _fileSystem.FileInfo.New(f).Length
            })
            .ToList();
        return View(videoFiles);
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
            if (!Directory.Exists("media"))
            {
                Directory.CreateDirectory("media");
            }

            foreach (var file in files)
            {
                if (file.ContentType != "video/mp4")
                {
                    response = new { success = false, message = $"File {file.FileName} is not Mp4 format" };
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    continue;
                }

                var filePath = Path.Combine("media", file.FileName);
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
