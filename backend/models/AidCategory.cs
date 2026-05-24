using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Models
{
    public class AidCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NameEn { get; set; }

        [StringLength(20)]
        public string Scope { get; set; } = "Both";

        [StringLength(50)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool AllowCustomText { get; set; }

        public int SortOrder { get; set; }

        public int? ParentAidCategoryId { get; set; }
        public AidCategory? ParentAidCategory { get; set; }
        public ICollection<AidCategory> Children { get; set; } = new List<AidCategory>();
    }
}
