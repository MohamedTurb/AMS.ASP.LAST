using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AssistanceManagementSystem.Tests.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"ams-integration-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                var inMemory = new Dictionary<string, string?>
                {
                    ["DatabaseProvider"] = "Sqlite",
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                    ["Jwt:Issuer"] = "AMS.Test",
                    ["Jwt:Audience"] = "AMS.Test.Client",
                    ["Jwt:SecretKey"] = "THIS_IS_A_TEST_SECRET_KEY_WITH_MIN_32_CHARS_123",
                    ["Jwt:ExpiryMinutes"] = "60"
                };

                configBuilder.AddInMemoryCollection(inMemory);
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
            }
            catch
            {
                // Ignore temp file cleanup failures in tests.
            }
        }
    }
}
