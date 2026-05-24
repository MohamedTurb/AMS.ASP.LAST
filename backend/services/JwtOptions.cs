namespace AssistanceManagementSystem.Services
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "AMS";
        public string Audience { get; set; } = "AMS.Client";
        public string SecretKey { get; set; } = "PLEASE_CHANGE_THIS_SUPER_SECRET_KEY_123456789";
        public int ExpiryMinutes { get; set; } = 60;
    }
}
