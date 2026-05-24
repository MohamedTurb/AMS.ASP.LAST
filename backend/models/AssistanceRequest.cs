using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssistanceManagementSystem.Models
{
    public class AssistanceRequest
    {
        public int Id { get; set; }

        // بيانات طالب المساعدة (رب الأسرة)
        [Required(ErrorMessage = "اسم طالب المساعدة مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "اسم طالب المساعدة (عربي)")]
        public string RequesterNameAr { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "اسم طالب المساعدة (إنجليزي)")]
        public string RequesterNameEn { get; set; } = string.Empty;

        // Backwards-compatible column used in existing DB and search — keep populated from Arabic name
        [Display(Name = "اسم طالب المساعدة")]
        public string RequesterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم طالب المساعدة القومي مطلوب")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يكون 14 رقم")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "الرقم القومي يجب أن يحتوي على 14 رقم فقط")]
        [Display(Name = "الرقم القومي لطالب المساعدة")]
        public string RequesterNationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم تليفون طالب المساعدة مطلوب")]
        [StringLength(15, ErrorMessage = "رقم التليفون يجب أن يكون أقل من 15 رقم")]
        [RegularExpression(@"^(\+2)?01[0-9]{9}$", ErrorMessage = "رقم التليفون غير صحيح")]
        [Display(Name = "رقم تليفون طالب المساعدة")]
        public string RequesterPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان طالب المساعدة مطلوب")]
        [StringLength(200, ErrorMessage = "العنوان يجب أن يكون أقل من 200 حرف")]
        [Display(Name = "عنوان طالب المساعدة")]
        public string RequesterAddress { get; set; } = string.Empty;

        // بيانات المستفيد الفعلي
        [Required(ErrorMessage = "اسم المستفيد مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "اسم المستفيد (عربي)")]
        public string BeneficiaryNameAr { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "اسم المستفيد (إنجليزي)")]
        public string BeneficiaryNameEn { get; set; } = string.Empty;

        // Backwards-compatible column used in existing DB and search — keep populated from Arabic name
        [Display(Name = "اسم المستفيد")]
        public string BeneficiaryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرقم القومي للمستفيد مطلوب")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يكون 14 رقم")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "الرقم القومي يجب أن يحتوي على 14 رقم فقط")]
        [Display(Name = "الرقم القومي للمستفيد")]
        public string BeneficiaryNationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم تليفون المستفيد مطلوب")]
        [StringLength(15, ErrorMessage = "رقم التليفون يجب أن يكون أقل من 15 رقم")]
        [RegularExpression(@"^(\+2)?01[0-9]{9}$", ErrorMessage = "رقم التليفون غير صحيح")]
        [Display(Name = "رقم تليفون المستفيد")]
        public string BeneficiaryPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان المستفيد مطلوب")]
        [StringLength(200, ErrorMessage = "العنوان يجب أن يكون أقل من 200 حرف")]
        [Display(Name = "عنوان المستفيد")]
        public string BeneficiaryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "الجنس مطلوب")]
        [Display(Name = "جنس المستفيد")]
        public string BeneficiaryGender { get; set; } = string.Empty;

        [Required(ErrorMessage = "الديانة مطلوبة")]
        [Display(Name = "ديانة المستفيد")]
        public string BeneficiaryReligion { get; set; } = string.Empty;

        [Required(ErrorMessage = "الحالة الاجتماعية مطلوبة")]
        [Display(Name = "الحالة الاجتماعية للمستفيد")]
        public string BeneficiaryMaritalStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "عدد أفراد الأسرة مطلوب")]
        [Range(1, 20, ErrorMessage = "عدد أفراد الأسرة يجب أن يكون بين 1 و 20")]
        [Display(Name = "عدد أفراد الأسرة")]
        public int BeneficiaryFamilyMembers { get; set; }

        [Required(ErrorMessage = "الدخل الشهري مطلوب")]
        [Range(0, 999999999, ErrorMessage = "الدخل الشهري غير صحيح")]
        [Display(Name = "الدخل الشهري (جنيه)")]
        public decimal BeneficiaryIncome { get; set; }

        [StringLength(500, ErrorMessage = "العلاقة بالمستفيد يجب أن تكون أقل من 500 حرف")]
        [Display(Name = "العلاقة بالمستفيد")]
        public string? RelationshipToBeneficiary { get; set; }

        [Required(ErrorMessage = "نوع المساعدة مطلوب")]
        [StringLength(50, ErrorMessage = "نوع المساعدة يجب أن يكون أقل من 50 حرف")]
        [Display(Name = "نوع المساعدة المطلوبة")]
        public string TypeOfAssistance { get; set; } = string.Empty;

        public int? AidCategoryId { get; set; }
        public AidCategory? AidCategory { get; set; }

        [StringLength(500, ErrorMessage = "تفاصيل التصنيف يجب أن تكون أقل من 500 حرف")]
        [Display(Name = "تفاصيل إضافية للتصنيف")]
        public string? AidCategoryDetails { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "قيمة المساعدة (جنيه)")]
        public decimal? Amount { get; set; }

        [StringLength(1000, ErrorMessage = "سبب الطلب يجب أن يكون أقل من 1000 حرف")]
        [Display(Name = "سبب الطلب / ملاحظات")]
        public string? Reason { get; set; }

        [StringLength(500, ErrorMessage = "أسماء الملفات يجب أن تكون أقل من 500 حرف")]
        [Display(Name = "مستندات داعمة")]
        public string? SupportingDocuments { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "معلق";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public string? ReviewedByUserId { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? ReviewNotes { get; set; }
        
        [StringLength(50)]
        public string? ReferenceNumber { get; set; }
        
        // Branch association
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
        
        // Navigation: multiple assistances can be created for a single request
        public ICollection<Assistance> Assistances { get; set; } = new List<Assistance>();
        // Audit entries for reviews
        public ICollection<ReviewAudit> ReviewAudits { get; set; } = new List<ReviewAudit>();
    }
}

namespace AssistanceManagementSystem.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        // Stored as a SHA256 hash for security
        public string TokenHash { get; set; } = string.Empty;
        [NotMapped]
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ReplacedByToken { get; set; }
    }
}
