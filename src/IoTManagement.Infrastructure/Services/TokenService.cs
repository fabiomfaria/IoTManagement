using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Interfaces;
using IoTManagement.Domain.Exceptions;
using IoTManagement.Application.DTOs;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of ITokenService that handles OAuth2 token operations
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserStore _userStore;

        public TokenService(IConfiguration configuration, IUserStore userStore)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userStore = userStore ?? throw new ArgumentNullException(nameof(userStore));
        }

        /// <summary>
        /// Processes an OAuth2 token request
        /// </summary>
        public async Task<OAuth2TokenResponseDto> ProcessTokenRequestAsync(OAuth2TokenRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Check grant type
            switch (request.GrantType)
            {
                case "password":
                    return await ProcessPasswordGrantAsync(request);
                case "refresh_token":
                    var refreshRequest = new OAuth2RefreshTokenRequestDto
                    {
                        RefreshToken = request.RefreshToken,
                        ClientId = request.ClientId,
                        ClientSecret = request.ClientSecret
                    };
                    return await RefreshTokenAsync(refreshRequest);
                case "client_credentials":
                    return await ProcessClientCredentialsGrantAsync(request);
                default:
                    throw new ValidationException("Unsupported grant type");
            }
        }

        /// <summary>
        /// Validates an access token
        /// </summary>
        public string ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetSecretKey());

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["OAuth2:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["OAuth2:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                return userId;
            }
            catch
            {
                // Return null if token validation fails
                return null;
            }
        }

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        public async Task<OAuth2TokenResponseDto> RefreshTokenAsync(OAuth2RefreshTokenRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate client credentials if provided
            if (!string.IsNullOrEmpty(request.ClientId) && !string.IsNullOrEmpty(request.ClientSecret))
            {
                ValidateClientCredentials(request.ClientId, request.ClientSecret);
            }

            // Find user with this refresh token
            var user = GetUserByRefreshToken(request.RefreshToken);
            if (user == null)
                throw new UnauthorizedException("Invalid refresh token");

            // Find the specific refresh token
            var refreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == request.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
                throw new UnauthorizedException("Invalid refresh token");

            // Generate new access token
            var accessToken = GenerateAccessToken(user);

            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();

            // Revoke the old refresh token
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = "127.0.0.1"; // In a real app, get from request
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            refreshToken.IsRevoked = true;

            // Add the new refresh token
            user.RefreshTokens.Add(newRefreshToken);

            // Remove old refresh tokens
            RemoveOldRefreshTokens(user);

            // Return the tokens
            return new OAuth2TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hour in seconds
                Scope = string.Join(" ", user.Scopes)
            };
        }

        /// <summary>
        /// Revokes a token
        /// </summary>
        public async Task<bool> RevokeTokenAsync(OAuth2RevokeTokenRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Find user with this refresh token
            var user = GetUserByRefreshToken(request.Token);
            if (user == null)
                return false;

            // Find the refresh token
            var refreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == request.Token);
            if (refreshToken == null)
                return false;

            // Revoke the token
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = "127.0.0.1"; // In a real app, get from request
            refreshToken.IsRevoked = true;

            return true;
        }

        #region Private methods

        private async Task<OAuth2TokenResponseDto> ProcessPasswordGrantAsync(OAuth2TokenRequestDto request)
        {
            // Validate client credentials if provided
            if (!string.IsNullOrEmpty(request.ClientId) && !string.IsNullOrEmpty(request.ClientSecret))
            {
                ValidateClientCredentials(request.ClientId, request.ClientSecret);
            }

            // Validate user credentials
            var user = _userStore.ValidateUser(request.Username, request.Password);
            if (user == null)
                throw new UnauthorizedException("Invalid username or password");

            // Generate the access token
            var accessToken = GenerateAccessToken(user);

            // Generate a refresh token
            var refreshToken = GenerateRefreshToken();

            // Add the refresh token to the user
            user.RefreshTokens.Add(refreshToken);

            // Remove old refresh tokens
            RemoveOldRefreshTokens(user);

            // Return the tokens
            return new OAuth2TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hour in seconds
                Scope = string.Join(" ", user.Scopes)
            };
        }

        private async Task<OAuth2TokenResponseDto> ProcessClientCredentialsGrantAsync(OAuth2TokenRequestDto request)
        {
            // Validate client credentials
            ValidateClientCredentials(request.ClientId, request.ClientSecret);

            // For client credentials, we create a special system user
            var user = new User
            {
                Id = "system",
                Username = request.ClientId,
                Role = "System",
                Scopes = new List<string> { "api" }
            };

            // Generate access token
            var accessToken = GenerateAccessToken(user);

            // Return the token (no refresh token for client credentials)
            return new OAuth2TokenResponseDto
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hour in seconds
                Scope = "api"
            };
        }

        private string GenerateAccessToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GetSecretKey());

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("scope", string.Join(" ", user.Scopes))
            };

            // Add email if available
            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            // Add name claims if available
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                claims.Add(new Claim("given_name", user.FirstName));
            }

            if (!string.IsNullOrEmpty(user.LastName))
            {
                claims.Add(new Claim("family_name", user.LastName));
            }

            // Add role claim
            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                Issuer = _configuration["OAuth2:Issuer"],
                Audience = _configuration["OAuth2:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7), // Refresh tokens are valid for 7 days
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // In a real app, get from request
            };
        }

        private void ValidateClientCredentials(string clientId, string clientSecret)
        {
            // In a real application, this would check against a database of clients
            var configClientId = _configuration["OAuth:ClientId"];
            var configClientSecret = _configuration["OAuth:ClientSecret"];

            if (clientId != configClientId || clientSecret != configClientSecret)
            {
                throw new UnauthorizedException("Invalid client credentials");
            }
        }

        private User GetUserByRefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            // Get all users
            var users = _userStore.GetAllUsers();

            // Find user with this refresh token
            return users.SingleOrDefault(u =>
                u.RefreshTokens.Any(rt => rt.Token == refreshToken && rt.IsActive));
        }

        private void RemoveOldRefreshTokens(User user)
        {
            if (user == null)
                return;

            // Configure TTL for refresh tokens in days
            var ttl = int.Parse(_configuration["OAuth2:RefreshTokenTTL"] ?? "7");

            // Remove old inactive refresh tokens from user based on TTL
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(ttl) <= DateTime.UtcNow);
        }

        private string GetSecretKey()
        {
            var key = _configuration["OAuth2:SecretKey"];
            if (string.IsNullOrEmpty(key))
                throw new Exception("OAuth2:SecretKey is not configured");

            return key;
        }

        #endregion
    }
}