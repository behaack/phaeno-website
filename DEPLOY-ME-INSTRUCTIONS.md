# Deployment Instructions

This document is the quick operational guide for deploying and managing the
Phaeno website API on the current Hetzner server.

## Current Production Target

- Hostname: `webops.phaenobiotech.com`
- Server IP: `178.156.175.151`
- SSH user: `root`
- App directory: `/opt/phaeno.website-api`
- Public URL: `https://webops.phaenobiotech.com`
- API local binding on server: `127.0.0.1:8081`
- File manager local binding on server: `127.0.0.1:8082`
- File manager URL: `https://webops.phaenobiotech.com/manage-pseq-assets-7f3c9/`
- Public documents directory: `/opt/phaeno.website-api/documents/public`
- Production database: PostgreSQL project `phaeno-website` through
  `ConnectionStrings:phaeno-website` in `phaeno.api/appsettings.json`

The current server uses host-level Nginx for ports `80` and `443`. Do not start
the `caddy` Compose profile on this server unless Nginx is intentionally removed
or reconfigured.

The API no longer runs a Hetzner PostgreSQL container. Production data lives in
PostgreSQL, and the server keeps only API, file manager, and optional Caddy
services.

## Before Deploying

From the local repository:

```powershell
git status --short
dotnet build .\phaeno.api\phaeno.api.csproj
```

Review any local changes before deploying. If the working tree has uncommitted
changes, decide whether they should be included in the deployment archive.

## Redeploy From Local Working Tree

Run these commands from the repository root on your workstation.

Create a clean archive that excludes local-only and production-only files:

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
```

Upload the archive:

```powershell
scp phaeno.website-api.tar.gz root@178.156.175.151:/tmp/phaeno.website-api.tar.gz
```

Extract and rebuild the API stack on the server:

```powershell
ssh root@178.156.175.151 "cd /opt/phaeno.website-api && tar -xzf /tmp/phaeno.website-api.tar.gz && docker compose up -d --build api"
```

This preserves the production `.env`, `documents` directory, and Docker volumes.

## Redeploy From Git Archive

If you only want committed files from `HEAD`, create the archive with Git:

```powershell
git archive --format=tar.gz -o phaeno.website-api.tar.gz HEAD
scp phaeno.website-api.tar.gz root@178.156.175.151:/tmp/phaeno.website-api.tar.gz
ssh root@178.156.175.151 "cd /opt/phaeno.website-api && tar -xzf /tmp/phaeno.website-api.tar.gz && docker compose up -d --build api"
```

Use this only when uncommitted local changes should not be deployed.

## Smoke Checks

Check container status:

```powershell
ssh root@178.156.175.151 "cd /opt/phaeno.website-api && docker compose ps"
```

Check the root page:

```powershell
ssh root@178.156.175.151 "curl -I --max-time 20 https://webops.phaenobiotech.com/"
```

Check the public search API:

```powershell
ssh root@178.156.175.151 "curl -sS -o /tmp/search-pages.out -w '%{http_code}' --max-time 20 'https://webops.phaenobiotech.com/api/v1/web-ops/search-pages?search=technology'"
```

Expected result is `200`.

## Common Server Commands

Open a shell on the server:

```powershell
ssh root@178.156.175.151
```

Go to the app directory:

```bash
cd /opt/phaeno.website-api
```

List Compose services:

```bash
docker compose ps
```

Rebuild and restart the API service:

```bash
docker compose up -d --build api
```

Restart only the API container:

```bash
docker compose restart api
```

Stop the API container:

```bash
docker compose stop api
```

Start the API container:

```bash
docker compose start api
```

Follow API logs:

```bash
docker compose logs -f api
```

Show recent API logs:

```bash
docker compose logs --tail=100 api
```

Check disk usage:

```bash
df -h
docker system df
```

Clean unused Docker build cache and unused images:

```bash
docker system prune
```

Do not use `docker system prune --volumes` unless you intend to remove unused
volumes.

## Environment Variables

Production secrets live in:

```bash
/opt/phaeno.website-api/.env
```

The required variables are:

```env
SITE_HOST=:80
RECAPTCHA_SECRET=...
RECAPTCHA_SERVICE_ACCOUNT_KEY_PATH=/app/__DOCUMENTS/private/secrets/recaptcha-enterprise-service-account.json
MAILGUN_API_KEY=...
```

Do not commit the production `.env` file and do not overwrite it during deploys.

The contact and order endpoints validate submissions with reCAPTCHA Enterprise.
Place the Google service account JSON on the server at:

```bash
/opt/phaeno.website-api/documents/private/secrets/recaptcha-enterprise-service-account.json
```

## Public Documents

Files in this directory are served under `/public`:

```bash
/opt/phaeno.website-api/documents/public
```

Example:

```bash
echo "hello" > /opt/phaeno.website-api/documents/public/hello.txt
curl https://webops.phaenobiotech.com/public/hello.txt
```

If uploads through File Browser fail, reset ownership and permissions:

```bash
chown -R 1000:1000 /opt/phaeno.website-api/documents/public
chmod -R u+rwX,g+rwX /opt/phaeno.website-api/documents/public
```

## Nginx And TLS

The active Nginx site is:

```bash
/etc/nginx/sites-enabled/phaeno-website-api.conf
```

Test and reload Nginx:

```bash
nginx -t
systemctl reload nginx
```

Reissue the TLS certificate if needed:

```bash
certbot --nginx -d webops.phaenobiotech.com
```

## Database Migrations

After schema changes, add a migration locally and deploy it intentionally. Do
not edit existing migration files by hand.

Build an EF migration bundle locally:

```powershell
dotnet ef migrations bundle --project phaeno.api -o migrate
```

Upload and run the bundle against production:

```powershell
scp migrate root@178.156.175.151:/tmp/migrate
ssh root@178.156.175.151 "chmod +x /tmp/migrate && /tmp/migrate --connection 'PostgreSQL_NPGSQL_CONNECTION_STRING'"
```

Use the PostgreSQL Npgsql connection string from `ConnectionStrings:phaeno-website`
or a secret manager. Do not run migrations against a Hetzner-local database.

## Rollback Notes

There is no automated rollback script. For safer rollbacks:

- Keep a copy of the previously deployed archive before extracting a new one.
- Confirm `docker compose ps` before and after deploys.
- Review `docker compose logs --tail=100 api` after every restart.
- Back up PostgreSQL before risky schema or data changes.
