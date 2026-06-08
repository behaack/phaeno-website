# Phaeno Website

This repository contains the public Phaeno website and the backend services that support website operations.

## Repository Layout

```text
.
├── ui/   # Astro website deployed on Vercel
└── api/  # ASP.NET Core backend services deployed on Hetzner
```

## UI

The UI is the Phaeno public website built with Astro, React islands, and Tailwind CSS. It contains the public pages, content collections, reusable components, layouts, SEO metadata helpers, and Vercel configuration.

Common commands:



```powershell
cd ui
pnpm install
pnpm dev
pnpm build
pnpm preview
```

The Vercel deployment uses the configuration in `ui/vercel.json`.

See [ui/README.md](ui/README.md) for the full UI project guide.

## API

The API is the backend service layer for the public website. It exposes versioned routes under `/api/v1` for site search, contact form submissions, and order inquiries. It also supports crawler/indexing jobs, PostgreSQL PostgreSQL persistence, Mailgun notifications, Google reCAPTCHA Enterprise validation, and public document hosting.

Common commands:

```powershell
cd api
dotnet restore .\phaeno.api\phaeno.api.csproj
dotnet build .\phaeno.api\phaeno.api.csproj
dotnet run --project .\phaeno.api\phaeno.api.csproj
```

Docker Compose local run:

```powershell
cd api
docker compose up -d --build api
docker compose logs -f api
```

The current API production target is Hetzner:

- Public URL: `https://webops.phaenobiotech.com`
- Server IP: `178.156.175.151`
- App directory: `/opt/phaeno.website-api`
- API binding: `127.0.0.1:8081`

See [api/README.md](api/README.md) for the full API project guide.

## Configuration

Keep production secrets out of git. The UI and API both use local environment files during development, and the API production environment is expected to keep secrets on the server.

Important API production variables:

```env
RECAPTCHA_SECRET=...
RECAPTCHA_SERVICE_ACCOUNT_KEY_PATH=/app/__DOCUMENTS/private/secrets/recaptcha-enterprise-service-account.json
MAILGUN_API_KEY=...
```

## Deployment Docs

The existing root deployment notes describe the API deployment process:

- [api/README.md](api/README.md)
- [DEPLOY-ME.md](DEPLOY-ME.md)
- [DEPLOY-ME-INSTRUCTIONS.md](DEPLOY-ME-INSTRUCTIONS.md)
