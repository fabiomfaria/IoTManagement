using System.Collections.Generic;

namespace IoTManagement.Application.DTOs
{
    /// <summary>
    /// DTO for user information
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>
        /// The ID of the user
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The username of the user
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The email of the user
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The name of the user
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The roles assigned to the user
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}