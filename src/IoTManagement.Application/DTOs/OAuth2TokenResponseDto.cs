using System.Text.Json.Serialization;

namespace IoTManagement.Application.DTOs
{
    /// <summary>
    /// DTO for OAuth2 token response
    /// </summary>
    public class OAuth2TokenResponseDto
    {
        /// <summary>
        /// The access token
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// The token type, typically "Bearer"
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// The lifetime of the access token in seconds
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// The refresh token for obtaining a new access token
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// The scope of the access token
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }
}