using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
