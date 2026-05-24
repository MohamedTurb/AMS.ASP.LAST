using Microsoft.AspNetCore.Http;

namespace AssistanceManagementSystem.Services
{
    public interface IAssistanceRequestFileService
    {
        List<string> Validate(IFormFileCollection? files);
        Task<List<string>> SaveFilesAsync(IFormFileCollection files, string webRootPath);
    }

    public class AssistanceRequestFileService : IAssistanceRequestFileService
    {
        private readonly FileUploadValidationOptions _options;

        public AssistanceRequestFileService(FileUploadValidationOptions options)
        {
            _options = options;
        }

        public List<string> Validate(IFormFileCollection? files)
        {
            var errors = new List<string>();

            if (files == null || files.Count == 0)
            {
                return errors;
            }

            if (files.Count > _options.MaxFilesCount)
            {
                errors.Add($"الحد الأقصى لعدد الملفات هو {_options.MaxFilesCount} ملفات.");
                return errors;
            }

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    errors.Add($"الملف '{file.FileName}' فارغ.");
                    continue;
                }

                if (file.Length > _options.MaxFileSizeBytes)
                {
                    errors.Add($"الملف '{file.FileName}' يتجاوز الحجم المسموح.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension))
                {
                    errors.Add($"امتداد الملف '{file.FileName}' غير مسموح.");
                }
            }

            return errors;
        }

        public async Task<List<string>> SaveFilesAsync(IFormFileCollection files, string webRootPath)
        {
            var uploadedNames = new List<string>();
            var uploadsPath = Path.Combine(webRootPath, "uploads");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var safeOriginalName = Path.GetFileNameWithoutExtension(file.FileName);
                safeOriginalName = string.Concat(safeOriginalName.Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-'));
                if (string.IsNullOrWhiteSpace(safeOriginalName))
                {
                    safeOriginalName = "file";
                }

                var fileName = $"{Guid.NewGuid():N}_{safeOriginalName}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                uploadedNames.Add(fileName);
            }

            return uploadedNames;
        }
    }
}
