# Phaeno Website API

Backend API and static host for the Phaeno public website operations.

The application supports public website workflows such as site search, contact
form submissions, order inquiries, static landing content, and public document
hosting.

## What It Does

- Serves the static web root from `phaeno.api/wwwroot`.
- Exposes versioned API routes under `/api/v1`.
- Handles public web operations:
  - `GET /api/v1/web-ops/search-pages`
  - `POST /api/v1/web-ops/contact`
  - `POST /api/v1/web-ops/order`
- Crawls and indexes `https://www.phaenobiotech.com` with Quartz jobs.
- Stores contact and order records in PostgreSQL.
- Sends email notifications through Mailgun.
- Validates public submissions with Google reCAPTCHA Enterprise.
- Serves uploaded public files from `/public`.

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Docker Compose
- Quartz
- Lucene.Net
- Mailgun
- Google reCAPTCHA Enterprise
- Optional Caddy reverse proxy

## Repository Layout

```text
.
├── phaeno.api/                  # ASP.NET Core API project
│   ├── Features/WebOps/         # Public website API endpoints and services
│   ├── Infrastructure/          # Database, notification, crawler, search
│   ├── Middleware/              # API exception middleware
│   ├── Migrations/              # EF Core migrations
│   └── wwwroot/                 # Static web root
├── docker-compose.yml           # Production Compose stack
├── Dockerfile                   # API container build
├── DEPLOY-ME.md                 # Detailed Hetzner deployment notes
└── DEPLOY-ME-INSTRUCTIONS.md    # Operational deploy commands
```

## Configuration

Local and production settings are provided through ASP.NET Core configuration.
Production values should come from environment variables, not committed files.

Required production variables:

```env
POSTGRES_PASSWORD=...
RECAPTCHA_SECRET=...
RECAPTCHA_SERVICE_ACCOUNT_KEY_PATH=/app/__DOCUMENTS/private/secrets/recaptcha-enterprise-service-account.json
MAILGUN_API_KEY=...
```

The Docker Compose production connection string is assembled from
`POSTGRES_PASSWORD`:

```text
ConnectionStrings__Production=Host=db;Port=5432;Database=website;Username=website;Password=${POSTGRES_PASSWORD}
```

Do not commit production `.env` files.

Production reCAPTCHA Enterprise validation also requires the Google service
account JSON at the configured `RECAPTCHA_SERVICE_ACCOUNT_KEY_PATH`. With the
default Docker Compose volume, place it on the server at:

```text
/opt/phaeno.website-api/documents/private/secrets/recaptcha-enterprise-service-account.json
```

## Local Development

Restore and build:

```powershell
dotnet restore .\phaeno.api\phaeno.api.csproj
dotnet build .\phaeno.api\phaeno.api.csproj
```

Run the API:

```powershell
dotnet run --project .\phaeno.api\phaeno.api.csproj
```

Run with Docker Compose:

```powershell
docker compose up -d --build api db
```

View logs:

```powershell
docker compose logs -f api
```

## Deployment

The current production target is a Hetzner server:

- Public URL: `https://webops.phaenobiotech.com`
- Server IP: `178.156.175.151`
- App directory: `/opt/phaeno.website-api`
- API binding: `127.0.0.1:8081`
- Public document directory: `/opt/phaeno.website-api/documents/public`

Use these documents for deployment details:

- [DEPLOY-ME.md](DEPLOY-ME.md)
- [DEPLOY-ME-INSTRUCTIONS.md](DEPLOY-ME-INSTRUCTIONS.md)

Typical manual redeploy:

```powershell
tar -czf phaeno.website-api.tar.gz `
  --exclude=.git `
  --exclude=.vs `
  --exclude=.vscode `
  --exclude=.idea `
  --exclude=bin `
  --exclude=obj `
  --exclude=phaeno.website-api.tar.gz `
  --exclude=.env `
  --exclude=documents `
  -C . .

scp phaeno.website-api.tar.gz root@178.156.175.151:/tmp/phaeno.website-api.tar.gz

ssh root@178.156.175.151 "cd /opt/phaeno.website-api && tar -xzf /tmp/phaeno.website-api.tar.gz && docker compose up -d --build api db"
```

## Deploying With GitHub Actions

The repository includes `.github/workflows/deploy.yml`. It runs manually through
`workflow_dispatch` from the GitHub Actions UI.

The workflow builds the API, creates a deployment archive, uploads it to the
server, extracts it into `/opt/phaeno.website-api`, rebuilds `api` and `db` with
Docker Compose, and runs public smoke checks.

Recommended repository secrets:

```text
DEPLOY_HOST=178.156.175.151
DEPLOY_USER=root
DEPLOY_SSH_KEY=<private SSH key with access to the server>
```

Add them in GitHub under Settings -> Secrets and variables -> Actions ->
Repository secrets. `DEPLOY_SSH_KEY` must be the full private key text,
including the `BEGIN` and `END` lines.

The workflow should not store production `.env` values. Keep production secrets
on the server in `/opt/phaeno.website-api/.env`.

## Smoke Checks

Check the deployed root page:

```powershell
ssh root@178.156.175.151 "curl -I --max-time 20 https://webops.phaenobiotech.com/"
```

Check the search endpoint:

```powershell
ssh root@178.156.175.151 "curl -sS -o /tmp/search-pages.out -w '%{http_code}' --max-time 20 'https://webops.phaenobiotech.com/api/v1/web-ops/search-pages?search=technology'"
```

Expected result for the search smoke check is `200`.

## Database Migrations

After schema changes, add a new EF Core migration locally and deploy it
intentionally. Do not edit existing migration files by hand.

Build a migration bundle:

```powershell
dotnet ef migrations bundle --project phaeno.api -o migrate
```

Run it against production only after backing up the database and confirming the
connection string.
