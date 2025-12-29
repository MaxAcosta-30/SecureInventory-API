using Microsoft.AspNetCore.Mvc;
using SecureInventory.Core.Entities;
using SecureInventory.Core.Interfaces;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;


namespace SecureInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    /// <summary>
    /// Registers a new user with the provided username and password.
    /// </summary>
    /// <param name="request">User registration data.</param>
    /// <returns>An action result indicating the outcome of the registration.</returns>
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
    /// Authenticates a user and generates a JWT token upon successful login.
    /// </summary>
    /// <param name="request">User login data.</param>
    /// <returns>An action result containing the JWT token if login is successful.</returns>
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
    /// Generates a JSON Web Token (JWT) for the authenticated user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A string representing the generated JWT token.</returns>
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
/// Data Transfer Object (DTO) for user registration.
/// </summary>
/// <param name="Username">The desired username.</param>
/// <param name="Password">The user's password.</param>
public record UserRegisterDto(string Username, string Password);
/// <summary>
/// Data Transfer Object (DTO) for user login.
/// </summary>
/// <param name="Username">The user's username.</param>
/// <param name="Password">The user's password.</param>
public record UserLoginDto(string Username, string Password);