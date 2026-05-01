# ACCOB - Sistema de Gestión de Cobranzas

Este proyecto es una aplicación web de alto rendimiento construida con el **SDK de .NET 9.0**. Utiliza una arquitectura moderna con soporte para múltiples motores de base de datos y herramientas de generación de reportes en Excel.

## 📋 Requisitos Previos

Asegúrate de contar con las siguientes versiones instaladas:

* **SDK de .NET 9.0:** (Versión mínima requerida: `9.0.x`).
* **Entity Framework Core Tools:** Para la gestión de migraciones.
* **Base de Datos:** El proyecto está configurado para soportar **PostgreSQL** (v9.0.1) y **SQLite** para entornos de desarrollo rápido.
* **IDE:** Visual Studio 2022 (v17.12 o superior) o VS Code.

## 🚀 Instalación y Configuración

### 1. Clonar y Restaurar
```bash
git clone <url-del-repositorio>
cd ACCOB
dotnet restore
```

### 2. Configuración de Herramientas Globales
Si es la primera vez que trabajas con Entity Framework en esta máquina, instala la herramienta global:
```bash
dotnet tool install --global dotnet-ef
```

### 3. Aplicar Migraciones
El sistema utiliza **Npgsql (PostgreSQL)** y **Sqlite**. Para actualizar tu base de datos local:
```bash
dotnet ef database update
```

## ⚙️ Ejecución

Para iniciar el servidor de desarrollo:
```bash
dotnet run
```
* **URL Local:** Verifica el puerto en `properties/launchSettings.json` (usualmente `https://localhost:7xxx`).

## 📦 Dependencias Clave (v9.0)

El proyecto utiliza las versiones más recientes de las siguientes librerías:
* **Entity Framework Core 9.0.11:** Para el mapeo objeto-relacional con SQL Server, SQLite y PostgreSQL.
* **ASP.NET Core Identity:** Para la gestión de seguridad, autenticación y roles de usuario.
* **ClosedXML (0.105.0):** Utilizada específicamente para la exportación de reportes de cobranza a formato Excel.
* **Npgsql.EntityFrameworkCore.PostgreSQL:** Proveedor de base de datos para el entorno de producción.

## 🛠️ Comandos de Mantenimiento

* **Actualizar paquetes:** `dotnet list package --outdated`
* **Crear nueva migración:** `dotnet ef migrations add <NombreMigracion>`
* **Limpiar temporales:** `dotnet clean`

---
**Tecnología:** .NET 9.0 | PostgreSQL | Entity Framework Core