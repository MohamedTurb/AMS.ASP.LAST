using AssistanceManagementSystem.Contracts.Api;
using AssistanceManagementSystem.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace AssistanceManagementSystem.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
        }

        [Fact]
        public async Task Health_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TokenEndpoint_ShouldReturnToken_ForValidCredentials()
        {
            var payload = new TokenRequestDto
            {
                Email = "admin@ams.com",
                Password = "Admin123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/token", payload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
            Assert.NotNull(tokenResponse);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.AccessToken));
            Assert.True(tokenResponse.ExpiresIn > 0);
            Assert.Contains("Admin", tokenResponse.Roles);
        }

        [Fact]
        public async Task TokenEndpoint_ShouldReturnUnauthorized_ForInvalidCredentials()
        {
            var payload = new TokenRequestDto
            {
                Email = "admin@ams.com",
                Password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/token", payload);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RefreshEndpoint_ShouldRotateTokens_AndInvalidateOldRefresh()
        {
            var payload = new TokenRequestDto
            {
                Email = "admin@ams.com",
                Password = "Admin123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/auth/token", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
            Assert.NotNull(tokenResponse);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.RefreshToken));

            // use refresh to get new tokens
            var refreshPayload = new RefreshRequestDto { RefreshToken = tokenResponse.RefreshToken };
            var refreshResp = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshPayload);
            Assert.Equal(HttpStatusCode.OK, refreshResp.StatusCode);

            var newTokens = await refreshResp.Content.ReadFromJsonAsync<TokenResponseDto>();
            Assert.NotNull(newTokens);
            Assert.False(string.IsNullOrWhiteSpace(newTokens.RefreshToken));
            Assert.NotEqual(tokenResponse.AccessToken, newTokens.AccessToken);
            Assert.NotEqual(tokenResponse.RefreshToken, newTokens.RefreshToken);

            // old refresh token should now be invalid
            var refreshAgain = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshPayload);
            Assert.Equal(HttpStatusCode.Unauthorized, refreshAgain.StatusCode);
        }
    }
}
