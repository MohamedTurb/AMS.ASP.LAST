using AssistanceManagementSystem.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace AssistanceManagementSystem.Tests
{
    public class AssistanceRequestFileServiceTests
    {
        private static FormFile BuildFormFile(string fileName, string content)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "supportingFiles", fileName);
        }

        [Fact]
        public void Validate_ShouldFail_WhenExtensionIsNotAllowed()
        {
            var options = new FileUploadValidationOptions
            {
                AllowedExtensions = new[] { ".pdf" },
                MaxFilesCount = 5,
                MaxFileSizeBytes = 1024
            };

            var service = new AssistanceRequestFileService(options);
            var files = new FormFileCollection
            {
                BuildFormFile("note.exe", "test")
            };

            var errors = service.Validate(files);

            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("غير مسموح"));
        }

        [Fact]
        public void Validate_ShouldFail_WhenFilesCountExceedsLimit()
        {
            var options = new FileUploadValidationOptions
            {
                AllowedExtensions = new[] { ".pdf" },
                MaxFilesCount = 1,
                MaxFileSizeBytes = 1024
            };

            var service = new AssistanceRequestFileService(options);
            var files = new FormFileCollection
            {
                BuildFormFile("a.pdf", "test"),
                BuildFormFile("b.pdf", "test")
            };

            var errors = service.Validate(files);

            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("الحد الأقصى"));
        }

        [Fact]
        public async Task SaveFilesAsync_ShouldSaveFiles_WhenValidationPasses()
        {
            var options = new FileUploadValidationOptions
            {
                AllowedExtensions = new[] { ".pdf" },
                MaxFilesCount = 5,
                MaxFileSizeBytes = 1024 * 1024
            };

            var service = new AssistanceRequestFileService(options);
            var files = new FormFileCollection
            {
                BuildFormFile("a.pdf", "content")
            };

            var tempRoot = Path.Combine(Path.GetTempPath(), $"ams-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempRoot);

            try
            {
                var names = await service.SaveFilesAsync(files, tempRoot);

                Assert.Single(names);
                var fullPath = Path.Combine(tempRoot, "uploads", names[0]);
                Assert.True(File.Exists(fullPath));
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }
    }
}
