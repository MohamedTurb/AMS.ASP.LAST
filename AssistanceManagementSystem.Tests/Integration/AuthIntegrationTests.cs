using AssistanceManagementSystem.Models;
using AssistanceManagementSystem.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AssistanceManagementSystem.Tests.Integration
{
    public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_UserCanBeCreated_AndReceiveToken()
        {
            var email = $"testuser+{Guid.NewGuid():N}@example.com";
            var password = "Test123!";

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // ensure role exists
                if (!await roleManager.RoleExistsAsync("Beneficiary"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Beneficiary"));
                }

                var user = new ApplicationUser { UserName = email, Email = email, FullName = "Test User" };
                var result = await userManager.CreateAsync(user, password);
                Assert.True(result.Succeeded, string.Join(";", result.Errors.Select(e => e.Description)));

                await userManager.AddToRoleAsync(user, "Beneficiary");
            }

            var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            var payload = new { Email = email, Password = password };
            var response = await client.PostAsJsonAsync("/api/v1/auth/token", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var token = await response.Content.ReadFromJsonAsync<AssistanceManagementSystem.Contracts.Api.TokenResponseDto>();
            Assert.NotNull(token);
            Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        }
    }
}
