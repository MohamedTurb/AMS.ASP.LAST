namespace AssistanceManagementSystem.Contracts.Api
{
    public class OrganizationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? AccountNumber { get; set; }
    }
}
