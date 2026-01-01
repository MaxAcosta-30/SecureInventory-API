namespace SecureInventory.Core.Entities;

/// <summary>
/// Representa un usuario del sistema SecureInventory.
/// Almacena la información de autenticación y autorización del usuario.
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único del usuario en la base de datos.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de usuario único utilizado para autenticación.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña del usuario generado con BCrypt.
    /// Nunca se almacena la contraseña en texto plano.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Rol del usuario en el sistema (por ejemplo: "Admin", "User").
    /// Se utiliza para control de acceso basado en roles (RBAC).
    /// </summary>
    public string Role { get; set; } = string.Empty;
}