# .NET 8 API - Dockerized with TLS Fix for Legacy SQL Server

This is a .NET 8 ASP.NET Core Web API project, containerized with Docker and configured to connect to older SQL Server instances that don't support TLS 1.2+.

## Prerequisites

- .NET 8 SDK
- Docker & Docker Compose

## Quick Start

```bash
# Build and run
docker compose up --build

# Access the API
curl http://localhost:8080/api/hello
curl http://localhost:8080/api/health
```

## Project Structure

```
├── Controllers/
│   ├── HelloController.cs        # Sample hello world endpoint
│   ├── HealthController.cs       # Health check endpoint
│   └── DatabaseController.cs     # SQL Server connection test endpoints
├── Data/
│   └── AppDbContext.cs           # Entity Framework DbContext
├── Models/
│   └── TestEntity.cs             # Sample database entity
├── Program.cs                    # Application entry point
├── appsettings.json              # Configuration
├── Dockerfile                    # Multi-stage build with TLS fix
├── docker-compose.yml            # Production config
├── docker-compose.override.yml   # Development overrides
├── .gitignore
└── .dockerignore
```

## Development Hot Reload with docker-compose.override.yml

For a fast development workflow with automatic hot reload, use the `docker-compose.override.yml` file. This override configures the API container to use:

- `Dockerfile.dev` (if present) with `dotnet watch run` for hot reload
- Bind mounts your source code into the container (`.:/src`)
- Enables file watcher for code changes

### When to Use
- **Local development:** Use hot reload when you want to see code changes reflected immediately without rebuilding the container.
- **Not for production:** This setup is only for development. For production, use the default `docker-compose.yml` and `Dockerfile`.

## How to Use (Development)

1. Ensure you have `docker-compose.override.yml` and (optionally) `Dockerfile.dev` in your project root.
2. Start the stack with:
```bash
docker compose up
```
3. Edit your C# files locally. The API will reload automatically on changes.

## How to Use (Production)

To run in production mode (no hot reload, optimized build):

1. Make sure you are using the default `docker-compose.yml` and `Dockerfile` only (no override file present).
2. Build and start the stack with:
```bash
docker compose -f docker-compose.yml up --build
```
Or simply:
```bash
docker compose up --build
```
if there is no `docker-compose.override.yml` in the directory.
3. The API will run in production mode, using the published build inside the container.

## TLS Fix Explanation

### The Problem

Modern Linux distributions (like Debian Bookworm, which backs the .NET 8 Docker images) ship with OpenSSL configured to enforce **TLS 1.2 minimum** by default. This causes connection failures when trying to connect to older SQL Server instances (SQL Server 2008/2012) that only support TLS 1.0 or TLS 1.1.

Error you might see without this fix:
```
A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: SSL Provider, error: 0 - The certificate received from the server was issued by an untrusted issuer.)
```

### The Fix

The Dockerfile modifies `/etc/ssl/openssl.cnf` to allow legacy TLS protocols:

```dockerfile
RUN sed -i 's/\[openssl_init\]/# [openssl_init]/' /etc/ssl/openssl.cnf
RUN printf "\n\n[openssl_init]\nssl_conf = ssl_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_sect]\nsystem_default = ssl_default_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_default_sect]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0\n" >> /etc/ssl/openssl.cnf
```

This configuration:
- **MinProtocol = TLSv1** - Allows TLS 1.0, 1.1, and 1.2
- **CipherString = DEFAULT@SECLEVEL=0** - Allows weaker ciphers for legacy compatibility

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/hello` | Hello World endpoint |
| GET | `/api/health` | Health check endpoint |
| GET | `/api/database/health` | Test SQL Server database connection |
| POST | `/api/database/seed` | Seed test data into the database |
| GET | `/api/database/test-entities` | Retrieve all test entities from database |

## License

MIT
