using System.Data;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;
using SecureInventory.Core.Interfaces;
using SecureInventory.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

/// <summary>
/// Punto de entrada principal de la aplicación SecureInventory API.
/// Configura servicios, autenticación JWT, conexiones a SQL Server y Redis, e inicia la aplicación web.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN BASE DE DATOS SQL SERVER ---
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("SQL_PORT") ?? "1440";
var dbUser = "sa";
var dbPass = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD"); 
var connectionString = $"Server={dbServer},{dbPort};Database=SecureInventoryDB;User Id={dbUser};Password={dbPass};TrustServerCertificate=True;";

builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));

// --- 2. CONFIGURACIÓN REDIS (CACHE-ASIDE PATTERN) ---
var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
var redisConn = $"localhost:{redisPort},abortConnect=false";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));

// --- 3. INYECCIÓN DE DEPENDENCIAS (REPOSITORIOS) ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// --- 4. CONFIGURACIÓN DE SEGURIDAD JWT (CRÍTICO) ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? "EstaEsUnaClaveSuperSecretaYDebeTenerAlMenos32Caracteres!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "SecureInventoryApi",
            ValidAudience = "SecureInventoryClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// --- 5. CONFIGURACIÓN DE API ---
builder.Services.AddControllers();
builder.Services.AddOpenApi(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// El orden importa aquí:
app.UseAuthentication(); // 1. ¿Quién eres?
app.UseAuthorization();  // 2. ¿Tienes permiso?

app.MapControllers(); 

app.Run();