using System;
using System.Collections.Generic;

namespace IoTManagement.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user (string, e.g., GUID or from an identity provider)
        /// </summary>
        public string Id { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }

        /// <summary>
        /// Password for authentication - in a real implementation, this would be hashed.
        /// For this exercise, plain text as per simplicity requirement.
        /// </summary>
        public string Password { get; set; }

        public List<string> Scopes { get; set; } = new List<string> { "api" }; // Default scope

        public virtual List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }

    /// <summary>
    /// Represents a refresh token for OAuth2 authentication
    /// </summary>
    public class RefreshToken
    {
        // Consider adding an Id if storing independently or for easier management
        // public int Id { get; set; } 
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public bool IsActive => !IsRevoked && DateTime.UtcNow < Expires;

        // public string UserId { get; set; } // Foreign key if RefreshToken is a separate table
        // public virtual User User { get; set; } // Navigation property
    }
}