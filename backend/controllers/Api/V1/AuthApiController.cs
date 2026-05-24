using AssistanceManagementSystem.Contracts.Api;
using AssistanceManagementSystem.Models;
using AssistanceManagementSystem.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AssistanceManagementSystem.Controllers.Api.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [Produces("application/json")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtOptions _jwtOptions;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtOptions> jwtOptions,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtOptions = jwtOptions.Value;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> Token([FromBody] TokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            return Ok(await CreateTokenResponseAsync(user));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var existing = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (existing == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            // If a revoked token is re-used, revoke all active tokens for that user.
            if (existing.IsRevoked)
            {
                await _refreshTokenService.RevokeAllActiveTokensForUserAsync(existing.UserId);
                return Unauthorized(new { message = "Refresh token was revoked." });
            }

            if (existing.Expires <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Refresh token expired." });
            }

            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            // rotate refresh token
            var response = await CreateTokenResponseAsync(user);
            await _refreshTokenService.RevokeRefreshTokenAsync(existing, response.RefreshToken);

            return Ok(response);
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequestDto request)
        {
            var existing = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (existing == null)
            {
                return NotFound();
            }

            // allow revocation only by owner or admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existing.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _refreshTokenService.RevokeRefreshTokenAsync(existing);
            return NoContent();
        }

        internal async Task<TokenResponseDto> CreateTokenResponseAsync(ApplicationUser user)
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
            var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var refresh = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

            return new TokenResponseDto
            {
                AccessToken = tokenString,
                ExpiresIn = (int)TimeSpan.FromMinutes(_jwtOptions.ExpiryMinutes).TotalSeconds,
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToArray(),
                RefreshToken = refresh.Token
            };
        }
    }
}
