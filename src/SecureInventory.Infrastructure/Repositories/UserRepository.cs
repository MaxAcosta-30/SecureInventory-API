using System.Data;
using Dapper;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;

namespace SecureInventory.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnection _dbConnection;

    public UserRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        // 🛡️ SEGURIDAD: Uso estricto de parámetros para evitar SQL Injection
        var sql = "SELECT * FROM Users WHERE Username = @Username";
        return await _dbConnection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<int> CreateUserAsync(User user)
    {
        var sql = @"
            INSERT INTO Users (Username, PasswordHash, Role) 
            VALUES (@Username, @PasswordHash, @Role);
            SELECT CAST(SCOPE_IDENTITY() as int);";
            
        return await _dbConnection.ExecuteScalarAsync<int>(sql, user);
    }
}