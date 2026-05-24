using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
