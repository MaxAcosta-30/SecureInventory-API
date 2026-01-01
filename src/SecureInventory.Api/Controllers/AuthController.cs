using Microsoft.AspNetCore.Mvc;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace SecureInventory.Api.Controllers;

/// <summary>
/// Controlador responsable de la autenticación y autorización de usuarios.
/// Proporciona endpoints para registro de nuevos usuarios y autenticación mediante JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de autenticación.
    /// </summary>
    /// <param name="userRepository">Repositorio para operaciones de acceso a datos de usuarios.</param>
    /// <param name="configuration">Configuración de la aplicación para obtener claves JWT.</param>
    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema con el nombre de usuario y contraseña proporcionados.
    /// La contraseña se hashea utilizando BCrypt antes de almacenarse en la base de datos.
    /// </summary>
    /// <param name="request">Datos de registro del usuario (Username y Password).</param>
    /// <returns>
    /// - 200 OK: Usuario registrado exitosamente.
    /// - 400 BadRequest: El nombre de usuario ya existe en el sistema.
    /// - 500 InternalServerError: Error en la base de datos o al procesar la solicitud.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null) return BadRequest("El usuario ya existe.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = "User"
        };

        await _userRepository.CreateUserAsync(newUser);
        return Ok("Usuario registrado exitosamente.");
    }

    /// <summary>
    /// Autentica un usuario existente y genera un token JWT válido por 1 hora si las credenciales son correctas.
    /// </summary>
    /// <param name="request">Datos de inicio de sesión del usuario (Username y Password).</param>
    /// <returns>
    /// - 200 OK: Autenticación exitosa. Retorna un objeto JSON con el token JWT: { "token": "..." }.
    /// - 401 Unauthorized: Las credenciales proporcionadas son inválidas (usuario no existe o contraseña incorrecta).
    /// - 500 InternalServerError: Error al consultar la base de datos o generar el token.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Credenciales inválidas.");
        }

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    /// <summary>
    /// Genera un token JWT (JSON Web Token) para el usuario autenticado.
    /// El token incluye claims de identificador, nombre de usuario y rol, y expira en 1 hora.
    /// </summary>
    /// <param name="user">Usuario para el cual se generará el token.</param>
    /// <returns>Cadena que representa el token JWT firmado y codificado.</returns>
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var keyString = _configuration["Jwt:Key"] ?? "EstaEsUnaClaveSuperSecretaYDebeTenerAlMenos32Caracteres!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "SecureInventoryApi",
            audience: "SecureInventoryClient",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>
/// Objeto de transferencia de datos (DTO) para el registro de nuevos usuarios.
/// </summary>
/// <param name="Username">Nombre de usuario deseado. Debe ser único en el sistema.</param>
/// <param name="Password">Contraseña del usuario. Será hasheada antes de almacenarse.</param>
public record UserRegisterDto(string Username, string Password);

/// <summary>
/// Objeto de transferencia de datos (DTO) para el inicio de sesión de usuarios.
/// </summary>
/// <param name="Username">Nombre de usuario registrado en el sistema.</param>
/// <param name="Password">Contraseña del usuario para autenticación.</param>
public record UserLoginDto(string Username, string Password);