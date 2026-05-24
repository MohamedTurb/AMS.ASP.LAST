using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Contracts.Api
{
    public class RevokeRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
