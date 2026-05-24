using System.Security.Claims;
using AssistanceManagementSystem.Contracts.Api;
using AssistanceManagementSystem.Models;
using AssistanceManagementSystem.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AssistanceManagementSystem.Controllers.Api.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth/external")]
    [Produces("application/json")]
    public class ExternalAuthApiController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly JwtOptions _jwtOptions;

        public ExternalAuthApiController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IRefreshTokenService refreshTokenService,
            IOptions<JwtOptions> jwtOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _refreshTokenService = refreshTokenService;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet("{provider}")]
        [AllowAnonymous]
        public IActionResult ChallengeProvider([FromRoute] string provider, [FromQuery] string? redirectUri = null)
        {
            var scheme = ResolveProviderScheme(provider);
            if (scheme == null)
            {
                return BadRequest(new { message = "Unsupported provider. Use 'google' or 'microsoft'." });
            }

            var callbackUrl = Url.Action(
                action: nameof(Callback),
                controller: "ExternalAuthApi",
                values: new { version = "1.0", provider = scheme, redirectUri },
                protocol: Request.Scheme);

            var props = _signInManager.ConfigureExternalAuthenticationProperties(scheme, callbackUrl ?? string.Empty);
            return Challenge(props, scheme);
        }

        [HttpGet("{provider}/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromRoute] string provider, [FromQuery] string? redirectUri = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Unauthorized(new { message = "External login failed. Missing login info." });
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            ApplicationUser? user;
            if (signInResult.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new { message = "Email claim is required from external provider." });
                }

                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest(new
                        {
                            message = "Could not create local user from external login.",
                            errors = createResult.Errors.Select(e => e.Description).ToArray()
                        });
                    }

                    if (await _roleManager.RoleExistsAsync("Beneficiary"))
                    {
                        await _userManager.AddToRoleAsync(user, "Beneficiary");
                    }
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Could not link external login.",
                        errors = addLoginResult.Errors.Select(e => e.Description).ToArray()
                    });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            if (user == null)
            {
                return Unauthorized(new { message = "External login failed." });
            }

            var tokenResponse = await CreateTokenResponseAsync(user);

            if (!string.IsNullOrWhiteSpace(redirectUri) && Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
            {
                var fragment =
                    $"access_token={Uri.EscapeDataString(tokenResponse.AccessToken)}" +
                    $"&refresh_token={Uri.EscapeDataString(tokenResponse.RefreshToken ?? string.Empty)}" +
                    $"&token_type={Uri.EscapeDataString(tokenResponse.TokenType)}" +
                    $"&expires_in={tokenResponse.ExpiresIn}";

                return Redirect($"{redirectUri}#{fragment}");
            }

            return Ok(tokenResponse);
        }

        private async Task<TokenResponseDto> CreateTokenResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                ExpiresIn = (int)TimeSpan.FromMinutes(_jwtOptions.ExpiryMinutes).TotalSeconds,
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToArray(),
                RefreshToken = refreshToken.Token
            };
        }

        private static string? ResolveProviderScheme(string provider)
        {
            if (provider.Equals("google", StringComparison.OrdinalIgnoreCase) ||
                provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
                return "Google";
            }

            if (provider.Equals("microsoft", StringComparison.OrdinalIgnoreCase) ||
                provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
            {
                return "Microsoft";
            }

            return null;
        }
    }
}
