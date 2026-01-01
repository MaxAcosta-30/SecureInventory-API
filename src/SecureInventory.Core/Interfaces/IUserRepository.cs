using SecureInventory.Core.Entities;

namespace SecureInventory.Core.Interfaces;

/// <summary>
/// Define el contrato para las operaciones de acceso a datos de usuarios.
/// Implementa el patrón Repository para abstraer la lógica de persistencia.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su nombre de usuario de forma asíncrona.
    /// </summary>
    /// <param name="username">Nombre de usuario del usuario a recuperar.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado de la tarea contiene el usuario si se encuentra; de lo contrario, null.
    /// </returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Crea un nuevo usuario en la base de datos de forma asíncrona.
    /// </summary>
    /// <param name="user">Usuario a crear. Debe contener Username, PasswordHash y Role.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado de la tarea contiene el ID del usuario recién creado.
    /// </returns>
    Task<int> CreateUserAsync(User user);
}