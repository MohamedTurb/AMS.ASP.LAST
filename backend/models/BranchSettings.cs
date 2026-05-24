using System.ComponentModel.DataAnnotations;

namespace AssistanceManagementSystem.Models
{
    public class BranchSettings
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public Branch? Branch { get; set; }

        public decimal? MaxPayout { get; set; }
        [StringLength(200)]
        public string? WorkingHours { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
