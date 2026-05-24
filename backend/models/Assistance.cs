using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssistanceManagementSystem.Models
{
    public class Assistance
    {
        public int Id { get; set; }

        [Required]
        public int BeneficiaryId { get; set; }
        public Beneficiary Beneficiary { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser CreatedByUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Optional link to an AssistanceRequest so multiple assistances can be tied to one request
        public int? AssistanceRequestId { get; set; }
        public AssistanceRequest? AssistanceRequest { get; set; }

        [StringLength(50)]
        public string? ReferenceNumber { get; set; }
    }
}
