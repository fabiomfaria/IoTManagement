using IoTManagement.Application.DTOs;
using System.Threading.Tasks;

namespace IoTManagement.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with username and password using OAuth2 password grant type
        /// </summary>
        /// <param name="request">Token request with username and password</param>
        /// <returns>OAuth2 token response or null if authentication fails</returns>
        Task<OAuth2TokenResponseDto?> GetTokenAsync(OAuth2TokenRequestDto request);

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>OAuth2 token response with new access and refresh tokens, or null if refresh token is invalid</returns>
        Task<OAuth2TokenResponseDto?> RefreshTokenAsync(OAuth2RefreshTokenRequestDto request);

        /// <summary>
        /// Revokes a token (access or refresh token)
        /// </summary>
        /// <param name="request">Token revocation request</param>
        /// <param name="username">Username of the token owner (optional)</param>
        /// <returns>True if token was successfully revoked, false otherwise</returns>
        Task<bool> RevokeTokenAsync(OAuth2RevokeTokenRequestDto request, string? username);

        /// <summary>
        /// Gets user information for a specific username
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>User information or null if user not found</returns>
        UserInfoDto? GetUserInfo(string? username);
    }
}