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

### 3. Configure the application

`appsettings.json` is **not** committed (it holds partner credentials). Create it locally, or use
`dotnet user-secrets`, and populate:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=YOUR_DB_NAME;User Id=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD;TrustServerCertificate=true"
  },
  "WalletStation": { "BaseUrl": "", "Username": "", "Password": "" },
  "CipService": { "BaseUrl": "" },
  "FastChannel": { "BaseUrl": "", "Username": "", "Password": "", "MerchantId": "" },
  "Resend": { "apiKey": "", "fromEmail": "" },
  "App": { "PublicBaseUrl": "https://your-host" },
  "DataProtection": { "KeysPath": "C:\\keys\\tripfinity" },
  "Cip": { "InstitutionId": "", "SenderAccountNumber": "" }
}
```

| Key | Purpose |
| --- | --- |
| `App:PublicBaseUrl` | Absolute base URL used in confirmation and password-reset emails. Set it in production so a spoofed `Host` header can never end up in an email link. |
| `Cip:InstitutionId` / `Cip:SenderAccountNumber` | Name-enquiry identifiers. Defaults exist in code but should be set per environment. |
| `Seed:Enabled` / `Seed:AdminEmail` / `Seed:AdminPassword` | Optional bootstrap of the administrator account. Off unless `Seed:Enabled` is `true`; never hardcode these. |
| `DataProtection:KeysPath` | Optional. Directory for the Data Protection key ring, which signs ticket QR codes and encrypts partner tokens. Set it to a persistent (and shared) path in containers, otherwise keys are lost on rebuild and previously issued QR codes stop verifying. |
| `Tickets:AcceptLegacyUnsignedQr` | Defaults to `false`. Set to `true` only while migrating: it accepts QR images issued before signing was introduced, which carry the bare ticket reference. |

### 4. Apply migrations

```bash
dotnet ef database update
```

### 5. Run the app

```bash
dotnet run
```

## Tests

```bash
dotnet test
```

The suite lives in `tests/Tripfinity.Tests` and runs against an in-memory SQLite database, so it needs
no SQL Server or network access. It covers the sign-in rules, password reset, ticket validation and the
seat-reservation/payment rollback path.

## Project layout

| Path | Contents |
| --- | --- |
| `Controllers`, `Api` | MVC controllers and the JSON surface used by the browser |
| `Services` | Business logic and partner integrations (wallet, CIP, FastChannel, email) |
| `Models` | EF Core entities, request/response DTOs, view models |
| `Utilities` | Cross-cutting helpers: auth filters, URL building, rate-limit policy names |
| `Views`, `wwwroot` | Razor views, CSS and JavaScript |
| `docs/control-flow.md` | Narrative description of the passenger, marshal and admin flows |
## Security notes

- All mutating requests require an antiforgery token (`AutoValidateAntiforgeryToken` is a global filter).
  JavaScript JSON calls send it through the `RequestVerificationToken` header, read from the
  `csrf-token` meta tag rendered by the layouts.
- `/api/*` endpoints are session-scoped: wallet reads resolve the caller's own wallet, and fund-moving
  or payout endpoints (`create`, `credit`, `debit`, `refund`, FastChannel) are admin-only.
- Partner bearer tokens and signing keys are encrypted at rest with ASP.NET Core Data Protection.
- Authentication endpoints are rate limited per client IP (10 requests per minute).
- Session cookies are `HttpOnly`, `SameSite=Lax`, and `Secure` when served over HTTPS.
- Ticket QR codes carry a Data Protection–signed payload, not the bare ticket reference, so a code cannot be
  forged or guessed from the reference format. Tampered or unsigned codes are rejected; see
  `Tickets:AcceptLegacyUnsignedQr` for the one-off migration flag.
- HSTS, HTTPS redirection and baseline security headers (CSP, `X-Frame-Options`, `nosniff`, ...) are
  enabled outside Development.

stay blessed ???????