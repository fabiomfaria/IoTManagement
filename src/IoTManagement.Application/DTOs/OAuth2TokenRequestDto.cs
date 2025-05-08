namespace IoTManagement.Application.DTOs
{
    /// <summary>
    /// DTO for OAuth2 token request using Password grant type
    /// </summary>
    public class OAuth2TokenRequestDto
    {
        /// <summary>
        /// The OAuth2 grant type, should be "password"
        /// </summary>
        public string GrantType { get; set; } = "password";

        /// <summary>
        /// The username for authentication
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The password for authentication
        /// </summary>
        public string Password { get; set; } = string.Empty;

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