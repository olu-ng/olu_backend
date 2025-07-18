using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OluBackendApp.Services
{
    /// <summary>
    /// Manages refresh token lifecycle: validation, generation, and storage.
    /// </summary>
    public interface IRefreshTokenService
    {
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
    }

    /// <summary>
    /// Simple implementation using in-memory store; replace with persistent DB for production.
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        // In-memory store: userId -> refreshToken
        private static readonly Dictionary<string, string> _store = new();

        public Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            return Task.FromResult(_store.TryGetValue(userId, out var existing)
                                   && existing == refreshToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            _store[userId] = refreshToken;
            return Task.CompletedTask;
        }
    }
}
