using AssistanceManagementSystem.Tests.Infrastructure;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace AssistanceManagementSystem.Tests.E2E
{
    public class LoginFlowE2ETests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public LoginFlowE2ETests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
                HandleCookies = true
            });
        }

        [Fact]
        public async Task LoginPage_WithValidCredentials_ShouldRedirectToHome()
        {
            var loginGet = await _client.GetAsync("/Account/Login");
            Assert.Equal(HttpStatusCode.OK, loginGet.StatusCode);

            var html = await loginGet.Content.ReadAsStringAsync();
            var token = ExtractAntiForgeryToken(html);
            Assert.False(string.IsNullOrWhiteSpace(token));

            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "admin@ams.com",
                ["Password"] = "Admin123!",
                ["RememberMe"] = "false",
                ["__RequestVerificationToken"] = token
            });

            var loginPost = await _client.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.Redirect, loginPost.StatusCode);

            var homeResponse = await _client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);

            var homeHtml = await homeResponse.Content.ReadAsStringAsync();
            Assert.Contains("نظام إدارة المساعدات", homeHtml);
        }

        private static string ExtractAntiForgeryToken(string html)
        {
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(html);
            var tokenInput = doc.QuerySelector("input[name='__RequestVerificationToken']");
            if (tokenInput != null)
            {
                return tokenInput.GetAttribute("value") ?? string.Empty;
            }

            var regex = new Regex("name=\"__RequestVerificationToken\"\\s+type=\"hidden\"\\s+value=\"(?<token>[^\"]+)\"", RegexOptions.IgnoreCase);
            var match = regex.Match(html);
            return match.Success ? match.Groups["token"].Value : string.Empty;
        }
    }
}
