namespace AssistanceManagementSystem.Contracts.Api
{
    public class AssistanceRequestSummaryDto
    {
        public int Id { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string TypeOfAssistance { get; set; } = string.Empty;
        public string? AidCategoryName { get; set; }
        public string? AidCategoryDetails { get; set; }
        public decimal? Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
