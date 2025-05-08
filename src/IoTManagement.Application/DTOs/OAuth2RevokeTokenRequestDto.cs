namespace IoTManagement.Application.DTOs
{
    /// <summary>
    /// DTO for OAuth2 token revocation request
    /// </summary>
    public class OAuth2RevokeTokenRequestDto
    {
        /// <summary>
        /// The token to revoke (can be access token or refresh token)
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The type of token to revoke, either "access_token" or "refresh_token"
        /// </summary>
        public string TokenTypeHint { get; set; } = string.Empty;
    }
}