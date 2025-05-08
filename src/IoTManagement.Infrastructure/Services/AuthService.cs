using IoTManagement.Application.DTOs;
using IoTManagement.Application.Interfaces;
using IoTManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the authentication service
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly List<User> _users;
        private readonly Dictionary<string, RefreshToken> _refreshTokens;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _refreshTokens = new Dictionary<string, RefreshToken>();

            // Load predefined users from configuration
            _users = _configuration.GetSection("PredefinedUsers")
                .Get<List<User>>() ?? new List<User>();
        }

        /// <inheritdoc />
        public async Task<OAuth2TokenResponseDto?> GetTokenAsync(OAuth2TokenRequestDto request)
        {
            // Find the user
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == request.Password);

            if (user == null)
            {
                return null;
            }

            // Generate the tokens
            return await GenerateTokensAsync(user);
        }

        /// <inheritdoc />
        public async Task<OAuth2TokenResponseDto?> RefreshTokenAsync(OAuth2RefreshTokenRequestDto request)
        {
            // Find the refresh token
            if (!_refreshTokens.TryGetValue(request.RefreshToken, out var storedToken) ||
                storedToken.Expires < DateTime.UtcNow)
            {
                return null;
            }

            // Find the user
            var user = _users.FirstOrDefault(u => u.Username == storedToken.Username);
            if (user == null)
            {
                return null;
            }

            // Remove the old refresh token
            _refreshTokens.Remove(request.RefreshToken);

            // Generate new tokens
            return await GenerateTokensAsync(user);
        }

        /// <inheritdoc />
        public Task<bool> RevokeTokenAsync(OAuth2RevokeTokenRequestDto request, string? username)
        {
            // If token type hint is refresh_token, try to revoke it
            if (request.TokenTypeHint == "refresh_token" && _refreshTokens.ContainsKey(request.Token))
            {
                _refreshTokens.Remove(request.Token);
                return Task.FromResult(true);
            }

            // Token revocation for access tokens is not implemented (would require blacklist)
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public UserInfoDto? GetUserInfo(string? username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return null;
            }

            return new UserInfoDto
            {
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles
            };
        }

        private async Task<OAuth2TokenResponseDto> GenerateTokensAsync(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["OAuth2:SecretKey"]);

            // Define token claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("scope", "api")
            };

            // Add role claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Set token expiration
            var accessTokenExpiry = int.Parse(_configuration["OAuth2:AccessTokenExpiryMinutes"]);
            var tokenExpires = DateTime.UtcNow.AddMinutes(accessTokenExpiry);

            // Create the JWT security token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = tokenExpires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["OAuth2:Issuer"],
                Audience = _configuration["OAuth2:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();

            // Store the refresh token
            _refreshTokens[refreshToken] = new RefreshToken
            {
                Username = user.Username,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["OAuth2:RefreshTokenExpiryDays"]))
            };

            // Create response
            return new OAuth2TokenResponseDto
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = (int)(tokenExpires - DateTime.UtcNow).TotalSeconds,
                RefreshToken = refreshToken,
                Scope = "api"
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Represents a user in the system
    /// </summary>
    internal class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents a refresh token
    /// </summary>
    internal class RefreshToken
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
    }
}