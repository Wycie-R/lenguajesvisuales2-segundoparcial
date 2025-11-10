-- Script para crear la base de datos manualmente
-- Este script es generado por Entity Framework Code First
-- pero también puede ejecutarse manualmente si es necesario

-- Crear la base de datos
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ClientesDB')
BEGIN
    CREATE DATABASE ClientesDB;
END
GO

USE ClientesDB;
GO

-- Tabla Clientes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        CI NVARCHAR(20) NOT NULL PRIMARY KEY,
        Nombres NVARCHAR(200) NOT NULL,
        Direccion NVARCHAR(300) NOT NULL,
        Telefono NVARCHAR(20) NOT NULL,
        FotoCasa1 VARBINARY(MAX) NULL,
        FotoCasa2 VARBINARY(MAX) NULL,
        FotoCasa3 VARBINARY(MAX) NULL,
        FechaRegistro DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Tabla ArchivosCliente
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ArchivosCliente')
BEGIN
    CREATE TABLE ArchivosCliente (
        IdArchivo INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CICliente NVARCHAR(20) NOT NULL,
        NombreArchivo NVARCHAR(255) NOT NULL,
        UrlArchivo NVARCHAR(500) NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ArchivosCliente_Clientes FOREIGN KEY (CICliente) 
            REFERENCES Clientes(CI) ON DELETE CASCADE
    );
END
GO

-- Tabla LogsApi
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LogsApi')
BEGIN
    CREATE TABLE LogsApi (
        IdLog INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
        TipoLog NVARCHAR(50) NOT NULL,
        RequestBody NVARCHAR(MAX) NULL,
        ResponseBody NVARCHAR(MAX) NULL,
        UrlEndpoint NVARCHAR(500) NOT NULL,
        MetodoHttp NVARCHAR(10) NOT NULL,
        DireccionIp NVARCHAR(50) NULL,
        Detalle NVARCHAR(MAX) NULL
    );
END
GO

-- Índices para mejorar el rendimiento
CREATE INDEX IX_ArchivosCliente_CICliente ON ArchivosCliente(CICliente);
CREATE INDEX IX_LogsApi_DateTime ON LogsApi(DateTime DESC);
CREATE INDEX IX_LogsApi_TipoLog ON LogsApi(TipoLog);
GO

-- Datos de prueba (opcional)
-- Cliente de ejemplo
INSERT INTO Clientes (CI, Nombres, Direccion, Telefono, FechaRegistro)
VALUES ('12345678', 'Juan Pérez', 'Av. Principal 123', '0981234567', GETDATE());

-- Log de ejemplo
INSERT INTO LogsApi (DateTime, TipoLog, UrlEndpoint, MetodoHttp, DireccionIp, Detalle)
VALUES (GETDATE(), 'INFO', '/api/clientes', 'POST', '127.0.0.1', 'Cliente registrado exitosamente');
GO

-- Consultas útiles para verificar
SELECT * FROM Clientes;
SELECT * FROM ArchivosCliente;
SELECT * FROM LogsApi;
GO

-- Ver el tamaño de la base de datos
EXEC sp_spaceused;
GO