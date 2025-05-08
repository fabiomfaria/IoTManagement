namespace IoTManagement.Application.DTOs
{
    /// <summary>
    /// DTO for OAuth2 token refresh request
    /// </summary>
    public class OAuth2RefreshTokenRequestDto
    {
        /// <summary>
        /// The OAuth2 grant type, should be "refresh_token"
        /// </summary>
        public string GrantType { get; set; } = "refresh_token";

        /// <summary>
        /// The refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Optional scope parameter
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Optional client_id parameter
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Optional client_secret parameter
        /// </summary>
        public string? ClientSecret { get; set; }
    }
}