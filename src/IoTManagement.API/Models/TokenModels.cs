using System;
using System.ComponentModel.DataAnnotations;

namespace IoTManagement.API.Models
{
    /// <summary>
    /// Modelo para requisição OAuth2 token
    /// </summary>
    public class OAuth2TokenRequest
    {
        [Required]
        public string GrantType { get; set; }
        
        // Required for password grant type
        public string Username { get; set; }
        public string Password { get; set; }
        
        // Required for refresh_token grant type
        public string RefreshToken { get; set; }
        
        // Optional for all grant types
        public string Scope { get; set; }
        
        // Optional for client_credentials grant type
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    /// <summary>
    /// Modelo para resposta OAuth2 token
    /// </summary>
    public class OAuth2TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
    }

    /// <summary>
    /// Modelo para requisição de revogação de token
    /// </summary>
    public class OAuth2RevokeTokenRequest
    {
        [Required]
        public string Token { get; set; }
        
        [Required]
        public string TokenTypeHint { get; set; }
    }

    /// <summary>
    /// Modelo para informações do usuário autenticado
    /// </summary>
    public class UserInfoResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
        public DateTime LastLogin { get; set; }
        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Modelo para erro de autenticação
    /// </summary>
    public class OAuth2ErrorResponse
    {
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorUri { get; set; }
    }
}