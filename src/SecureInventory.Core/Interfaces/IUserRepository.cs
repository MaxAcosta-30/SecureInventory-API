using SecureInventory.Core.Entities;

namespace SecureInventory.Core.Interfaces;

/// <summary>
/// Defines the contract for user data access operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their username asynchronously.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user if found; otherwise, null.</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Creates a new user asynchronously.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the newly created user.</returns>
    Task<int> CreateUserAsync(User user);
}