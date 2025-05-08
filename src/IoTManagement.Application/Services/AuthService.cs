using IoTManagement.Application.DTOs;
using IoTManagement.Application.Interfaces;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;
using IoTManagement.Domain.Interfaces;
using System.Threading.Tasks;

namespace IoTManagement.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserStore _userStore;
        private readonly ITokenService _tokenService;

        public AuthService(IUserStore userStore, ITokenService tokenService)
        {
            _userStore = userStore;
            _tokenService = tokenService;
        }

        public async Task<OAuth2TokenResponseDto> AuthenticateAsync(OAuth2TokenRequestDto request)
        {
            if (request.GrantType != "password")
            {
                throw new ValidationException("Invalid grant type. Only 'password' grant type is supported.");
            }

            User user = await _userStore.GetUserByUsernameAsync(request.Username);

            if (user == null || !_userStore.ValidatePassword(user, request.Password))
            {
                throw new UnauthorizedException("Invalid username or password.");
            }

            return new OAuth2TokenResponseDto
            {
                AccessToken = await _tokenService.GenerateAccessTokenAsync(user),
                RefreshToken = await _tokenService.GenerateRefreshTokenAsync(user),
                ExpiresIn = 3600, // Token expiration in seconds (1 hour)
                TokenType = "Bearer"
            };
        }

        public async Task<OAuth2TokenResponseDto> RefreshTokenAsync(OAuth2RefreshTokenRequestDto request)
        {
            if (request.GrantType != "refresh_token")
            {
                throw new ValidationException("Invalid grant type. Only 'refresh_token' grant type is supported.");
            }

            User user = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid refresh token.");
            }

            return new OAuth2TokenResponseDto
            {
                AccessToken = await _tokenService.GenerateAccessTokenAsync(user),
                RefreshToken = await _tokenService.GenerateRefreshTokenAsync(user),
                ExpiresIn = 3600, // Token expiration in seconds (1 hour)
                TokenType = "Bearer"
            };
        }

        public async Task RevokeTokenAsync(OAuth2RevokeTokenRequestDto request)
        {
            await _tokenService.RevokeTokenAsync(request);
        }

        public async Task<UserInfoDto> GetUserInfoAsync(string accessToken)
        {
            User user = await _tokenService.ValidateAccessTokenAsync(accessToken);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid access token.");
            }

            return new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Name = user.Name,
                Roles = user.Roles
            };
        }

        // Fix for CS0535: Implementing IAuthService.GetTokenAsync
        public async Task<OAuth2TokenResponseDto?> GetTokenAsync(OAuth2TokenRequestDto request)
        {
            return await AuthenticateAsync(request);
        }

        // Fix for CS0535: Implementing IAuthService.RevokeTokenAsync with additional username parameter
        public async Task<bool> RevokeTokenAsync(OAuth2RevokeTokenRequestDto request, string? username)
        {
            await _tokenService.RevokeRefreshTokenAsync(request.Token);
            return true; // Assuming successful revocation
        }

        // Fix for CS0535: Implementing IAuthService.GetUserInfo with username parameter
        public UserInfoDto? GetUserInfo(string? username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ValidationException("Username cannot be null or empty.");
            }

            User user = _userStore.GetAllUsers().FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                throw new UnauthorizedException("User not found.");
            }

            return new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Name = user.Name,
                Roles = user.Roles
            };
        }
    }
    public interface ITokenService
    {
        Task<OAuth2TokenResponseDto> ProcessTokenRequestAsync(OAuth2TokenRequestDto request);
        string ValidateToken(string token);
        Task<OAuth2TokenResponseDto> RefreshTokenAsync(OAuth2RefreshTokenRequestDto request);
        Task<bool> RevokeTokenAsync(OAuth2RevokeTokenRequestDto request); // Added this method
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(User user);
        Task<User> ValidateRefreshTokenAsync(string refreshToken);
        Task<User> ValidateAccessTokenAsync(string accessToken);
        Task RevokeRefreshTokenAsync(string token);
    }
}