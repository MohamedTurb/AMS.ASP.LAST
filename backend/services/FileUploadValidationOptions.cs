namespace AssistanceManagementSystem.Services
{
    public class FileUploadValidationOptions
    {
        public int MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
        public int MaxFilesCount { get; set; } = 5;
        public string[] AllowedExtensions { get; set; } = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
    }
}
