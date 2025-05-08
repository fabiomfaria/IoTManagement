using System.Threading.Tasks;
using IoTManagement.Application.DTOs; // Assuming DTOs are in Application layer

namespace IoTManagement.Domain.Interfaces
{
    /// <summary>
    /// Interface for OAuth2 token services.
    /// The implementation of this service might reside in Infrastructure,
    /// handling token generation, validation, and storage intricacies.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Processes an OAuth2 token request (e.g., password grant, client credentials).
        /// </summary>
        /// <param name="request">The token request DTO.</param>
        /// <returns>A DTO containing the token response.</returns>
        Task<OAuth2TokenResponseDto> ProcessTokenRequestAsync(OAuth2TokenRequestDto request);

        /// <summary>
        /// Validates an access token.
        /// </summary>
        /// <param name="token">The access token to validate.</param>
        /// <returns>User ID if token is valid, otherwise null (or could throw an exception based on implementation choice for invalid tokens).</returns>
        string ValidateToken(string token);

        /// <summary>
        /// Refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="request">The refresh token request DTO.</param>
        /// <returns>A DTO containing the new token response.</returns>
        Task<OAuth2TokenResponseDto> RefreshTokenAsync(OAuth2RefreshTokenRequestDto request);

        /// <summary>
        /// Revokes a refresh token (and associated access tokens if applicable).
        /// </summary>
        /// <param name="request">The token revocation request DTO.</param>
        /// <returns>True if the token was successfully revoked, otherwise false.</returns>
        Task<bool> RevokeTokenAsync(OAuth2RevokeTokenRequestDto request);
    }
}