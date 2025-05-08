using System.Collections.Generic;
using IoTManagement.Domain.Entities; // Referencia a entidade User

namespace IoTManagement.Domain.Interfaces
{
    /// <summary>
    /// Interface for user management operations.
    /// Implementation would typically be in Infrastructure, interacting with a database or identity provider.
    /// </summary>
    public interface IUserStore
    {
        /// <summary>
        /// Validates user credentials.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>The <see cref="User"/> object if credentials are valid; otherwise, null.</returns>
        User ValidateUser(string username, string password);

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve. The User ID is a string.</param>
        /// <returns>The <see cref="User"/> object if found; otherwise, null.</returns>
        User GetUserById(string userId);

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>An enumerable collection of all <see cref="User"/> objects.</returns>
        IEnumerable<User> GetAllUsers();
    }
}