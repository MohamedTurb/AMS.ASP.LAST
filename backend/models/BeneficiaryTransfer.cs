using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Models
{
    public class BeneficiaryTransfer
    {
        public int Id { get; set; }
        public int BeneficiaryId { get; set; }
        public Beneficiary? Beneficiary { get; set; }

        public int FromBranchId { get; set; }
        public int ToBranchId { get; set; }

        public string? PerformedByUserId { get; set; }
        public ApplicationUser? PerformedByUser { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
