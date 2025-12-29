-- init.sql
CREATE DATABASE SecureInventoryDB;
GO
USE SecureInventoryDB;
GO

-- 1. Tabla de Usuarios (Seguridad)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL -- 'Admin', 'User'
);
GO

-- 2. Tabla de Productos (Inventario)
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0), -- Validación a nivel de BD
    Stock INT NOT NULL CHECK (Stock >= 0)
);
GO

-- 3. Datos Semilla (Seed Data)
-- Insertamos un producto de prueba
INSERT INTO Products (Name, Price, Stock) VALUES ('Laptop Gamer Linux', 1500.00, 10);
GO