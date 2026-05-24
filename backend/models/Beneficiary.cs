using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssistanceManagementSystem.Models
{
    public class Beneficiary
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FullNameAr { get; set; }

        [StringLength(100)]
        public string? FullNameEn { get; set; }

        [Required]
        [StringLength(14)]
        public string NationalId { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(20)]
        public string? Religion { get; set; }

        [StringLength(20)]
        public string? MaritalStatus { get; set; }

        public int? FamilyMembers { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Income { get; set; }

        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        public int? AidCategoryId { get; set; }
        public AidCategory? AidCategory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Assistance> Assistances { get; set; } = new List<Assistance>();
    }
}
