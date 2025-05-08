using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Interfaces;
using IoTManagement.Domain.Exceptions;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IUserStore that manages user authentication
    /// </summary>
    public class UserStore : IUserStore
    {
        private readonly List<User> _users = new List<User>();
        private readonly IConfiguration _configuration;

        public UserStore(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Load users from configuration
            LoadUsersFromConfiguration();
        }

        /// <summary>
        /// Validates user credentials and returns the user if valid
        /// </summary>
        public User ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            // Find user by username (case insensitive)
            var user = _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return null;

            // Verify password (direct comparison for simplicity - in production use a secure hash)
            if (user.Password == password)
                return user;

            return null;
        }
        
        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        public User GetUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
                
            return _users.FirstOrDefault(u => u.Id == userId);
        }
        
        /// <summary>
        /// Gets all users
        /// </summary>
        public IEnumerable<User> GetAllUsers()
        {
            return _users;
        }

        private void LoadUsersFromConfiguration()
        {
            var configUsers = _configuration.GetSection("Users").Get<List<UserConfig>>();
            
            if (configUsers == null || !configUsers.Any())
                throw new Exception("No users defined in configuration");
                
            foreach (var configUser in configUsers)
            {
                _users.Add(new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = configUser.Username,
                    Password = configUser.Password,
                    Role = configUser.Role,
                    FirstName = configUser.Username, // Default values since not in config
                    LastName = "",
                    Email = $"{configUser.Username}@example.com",
                    RefreshTokens = new List<RefreshToken>()
                });
            }
        }
        
        // Helper class to deserialize users from config
        private class UserConfig
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }
    }
}
}