using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using VideoUploadApp.Controllers;
using VideoUploadApp.Models;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace VideoUploadApp.Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithListOfVideoFiles()
        {
            // setup!
            var mockLogger = new Mock<ILogger<HomeController>>();
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            var mockFileSystem = new MockFileSystem();

            string testPath = "testPath";
            mockEnvironment.Setup(e => e.WebRootPath).Returns(testPath);

            var mediaPath = Path.Combine(testPath, "media");
            mockFileSystem.AddDirectory(mediaPath);
            mockFileSystem.AddFile(Path.Combine(mediaPath, "file1.mp4"), new MockFileData(""));
            mockFileSystem.AddFile(Path.Combine(mediaPath, "file2.mp4"), new MockFileData(""));

            var controller = new HomeController(mockLogger.Object, mockEnvironment.Object);
            var fileSystemField = typeof(HomeController).GetField("_fileSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fileSystemField.SetValue(controller, mockFileSystem);

            // act
            var result = controller.Index() as ViewResult;

            // result
            Assert.NotNull(result);
            var model = Assert.IsType<List<VideoFileModel>>(result.Model);
            Assert.Equal(2, model.Count);
        }
    }
}