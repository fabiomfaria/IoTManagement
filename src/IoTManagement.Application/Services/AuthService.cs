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
            await _tokenService.RevokeRefreshTokenAsync(request.Token);
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
    }
}