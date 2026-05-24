using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> CreateRefreshTokenAsync(string userId, int expiryMinutes = 60 * 24 * 30);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedByToken = null);
        Task RevokeAllActiveTokensForUserAsync(string userId);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtOptions _jwtOptions;

        public RefreshTokenService(ApplicationDbContext context, IOptions<JwtOptions> jwtOptions)
        {
            _context = context;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(string userId, int expiryMinutes = 60 * 24 * 30)
        {
            var raw = GenerateToken();
            var hash = ComputeHash(raw);

            var token = new RefreshToken
            {
                TokenHash = hash,
                Token = raw,
                UserId = userId,
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            var hash = ComputeHash(token);
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token, string? replacedByToken = null)
        {
            token.IsRevoked = true;
            token.ReplacedByToken = string.IsNullOrWhiteSpace(replacedByToken) ? null : ComputeHash(replacedByToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllActiveTokensForUserAsync(string userId)
        {
            var active = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.Expires > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in active)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }

        private static string GenerateToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private string ComputeHash(string input)
        {
            var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);
            using var hmac = new HMACSHA256(key);
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = hmac.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
