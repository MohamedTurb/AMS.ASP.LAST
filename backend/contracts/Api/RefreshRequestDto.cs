using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Contracts.Api
{
    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
