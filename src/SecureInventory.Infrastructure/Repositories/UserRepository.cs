using System.Data;
using Dapper;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;

namespace SecureInventory.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios utilizando Dapper para acceso a datos SQL Server.
/// Proporciona operaciones de lectura y escritura sobre la tabla Users.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbConnection _dbConnection;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de usuarios.
    /// </summary>
    /// <param name="dbConnection">Conexión a la base de datos SQL Server inyectada por dependencias.</param>
    public UserRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario de forma asíncrona.
    /// Utiliza Dapper con parámetros tipados para prevenir ataques de inyección SQL.
    /// </summary>
    /// <param name="username">Nombre de usuario del usuario a buscar.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado contiene el usuario si se encuentra; de lo contrario, null.
    /// </returns>
    /// <remarks>
    /// SEGURIDAD: Este método utiliza parámetros tipados de Dapper (@Username) para prevenir
    /// ataques de inyección SQL. Nunca se debe construir la consulta SQL mediante concatenación de strings.
    /// </remarks>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var sql = "SELECT * FROM Users WHERE Username = @Username";
        return await _dbConnection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    /// <summary>
    /// Crea un nuevo usuario en la base de datos de forma asíncrona.
    /// Inserta el usuario y retorna el ID generado automáticamente por SQL Server (IDENTITY).
    /// </summary>
    /// <param name="user">Usuario a crear. Debe contener Username, PasswordHash y Role.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona. 
    /// El resultado contiene el ID del usuario recién creado.
    /// </returns>
    public async Task<int> CreateUserAsync(User user)
    {
        var sql = @"
            INSERT INTO Users (Username, PasswordHash, Role) 
            VALUES (@Username, @PasswordHash, @Role);
            SELECT CAST(SCOPE_IDENTITY() as int);";
            
        return await _dbConnection.ExecuteScalarAsync<int>(sql, user);
    }
}