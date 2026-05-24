using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Models
{
    public class Branch
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    }
}
