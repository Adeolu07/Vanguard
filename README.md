# Tripfinity - Smart Transport Booking System

A comprehensive transport booking platform with digital wallet integration, QR ticketing, and real-time seat management.

## 🚀 Features

- **User Authentication** - Sign up, sign in, session management
- **Home Page** - Select mode of transport, fund wallet, check made booking

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core MVC (.NET 10)
- **Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **Frontend**: HTML (cshtml Razor Pages), CSS, JavaScript
- **Authentication**: Session-based auth
- **Container**: Docker (SQL Server for macos)

## 📋 Prerequisites

- [.NET 10](https://dotnet.microsoft.com/download)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server)
- or [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for SQL Server)

## 🔧 Setup Instructions

### 1. Clone the repository

```bash
git clone https://github.com/Adeolu07/Vanguard.git
cd Vanguard
```

### 2. Start SQL Server (or docker container )

```bash
docker run -e "ACCEPT_EULA=Y" \
   -e "MSSQL_SA_PASSWORD=YourPassword123!" \
   -p 1433:1433 \
   --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3. Configure database connection

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,portNumber;Database=YOUR_DB_NAME;User Id=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD;TrustServerCertificate=true"
  }
}
```

### 4. Run migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Run the app

```bash
dotnet run
```

stay blessed 🏌🏾‍♂️
