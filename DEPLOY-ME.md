# Deploying to Hetzner

This project can be hosted on a Hetzner Cloud server as a small Docker Compose stack:

- `api`: the .NET API
- `db`: PostgreSQL
- `caddy`: optional reverse proxy with automatic HTTPS

## Important Notes

- The API targets `.NET 10`.
- The app uses PostgreSQL through `Npgsql.EntityFrameworkCore.PostgreSQL`.
- In production, `Program.cs` reads `ConnectionStrings:Production`, so the server must provide `ConnectionStrings__Production`.
- Do not expose PostgreSQL port `5432` publicly.
- Rotate any secrets that have been committed to `appsettings.json` before going live. Use environment variables for production secrets.
- Because the app calls `UseHttpsRedirection()`, hosting behind a reverse proxy should include forwarded header support in the API to avoid HTTPS redirect issues.

## Hetzner Setup

1. Create a Hetzner Cloud server with Ubuntu LTS.
2. Enable Hetzner backups for the server.
3. Add a Hetzner firewall with only these inbound rules:
   - SSH `22` from your IP address
   - HTTP `80`
   - HTTPS `443`
4. Point your DNS `A` record to the server public IP.
5. Install Docker and Docker Compose on the server.
6. Clone this repository onto the server.
7. Add the production `.env` file.
8. Start the stack with Docker Compose.
9. Run Entity Framework migrations against the production database.

If the server already has Nginx or another reverse proxy using ports `80` and `443`, do not start the Caddy profile. The Compose file exposes the API on `127.0.0.1:8081` so an existing reverse proxy can route traffic to it.

## Current Server Notes

The current Hetzner server uses Nginx on the host rather than the optional Caddy container.

- Public hostname: `webops.phaenobiotech.com`
- Server IP: `178.156.175.151`
- API container binding: `127.0.0.1:8081`
- File manager binding: `127.0.0.1:8082`
- Active Nginx site: `/etc/nginx/sites-enabled/phaeno-website-api.conf`
- HTTPS certificate: `/etc/letsencrypt/live/webops.phaenobiotech.com/fullchain.pem`
- Public file directory: `/opt/phaeno.website-api/documents/public`
- File manager URL: `https://webops.phaenobiotech.com/manage-pseq-assets-7f3c9/`

DNS for `webops.phaenobiotech.com` must point to `178.156.175.151`.

If the certificate ever needs to be reissued, run this on the server:

```bash
certbot --nginx -d webops.phaenobiotech.com
```

Files placed under `/opt/phaeno.website-api/documents/public` are served by the API under `/public`.

File Browser runs as UID/GID `1000`, so the public folder must be writable by that user:

```bash
chown -R 1000:1000 /opt/phaeno.website-api/documents/public
chmod -R u+rwX,g+rwX /opt/phaeno.website-api/documents/public
```

Example:

```bash
echo "hello" > /opt/phaeno.website-api/documents/public/hello.txt
```

Then open:

```text
https://webops.phaenobiotech.com/public/hello.txt
```

The browser-based file manager is served at:

```text
https://webops.phaenobiotech.com/manage-pseq-assets-7f3c9/
```

It manages the same directory as `/public`, so uploaded files become available under `/public/<filename>`.

## Dockerfile

The repository includes this `Dockerfile` at the repository root:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY phaeno.api/phaeno.api.csproj phaeno.api/
RUN dotnet restore phaeno.api/phaeno.api.csproj

COPY . .
RUN dotnet publish phaeno.api/phaeno.api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "phaeno.api.dll"]
```

## Docker Compose

The repository includes this `docker-compose.yml` at the repository root:

```yaml
services:
  api:
    build: .
    restart: unless-stopped
    ports:
      - "127.0.0.1:8081:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__Production: Host=db;Port=5432;Database=website;Username=website;Password=${POSTGRES_PASSWORD}
      GoogleAuthSettings__RecaptchaSecretKey: ${RECAPTCHA_SECRET}
      EmailServiceSettings__ApiKey: ${MAILGUN_API_KEY}
    volumes:
      - ./documents:/app/__DOCUMENTS
    depends_on:
      - db

  db:
    image: postgres:17
    restart: unless-stopped
    environment:
      POSTGRES_DB: website
      POSTGRES_USER: website
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data

  filebrowser:
    image: filebrowser/filebrowser:v2
    restart: unless-stopped
    ports:
      - "127.0.0.1:8082:80"
    volumes:
      - ./documents/public:/srv
      - filebrowser_data:/database
    command:
      - --database
      - /database/filebrowser.db
      - --root
      - /srv
      - --baseurl
      - /manage-pseq-assets-7f3c9
      - --address
      - 0.0.0.0

  caddy:
    image: caddy:2
    restart: unless-stopped
    profiles:
      - caddy
    environment:
      SITE_HOST: ${SITE_HOST}
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - caddy_data:/data
      - caddy_config:/config
    depends_on:
      - api

volumes:
  postgres_data:
  filebrowser_data:
  caddy_data:
  caddy_config:
```

## Caddy

The repository includes this `Caddyfile` at the repository root:

```caddyfile
{$SITE_HOST} {
  reverse_proxy api:8080 {
    header_up X-Forwarded-Proto https
  }
}
```

Set `SITE_HOST=:80` for an initial IP-based HTTP deployment. After DNS points to the server, set `SITE_HOST=api.yourdomain.com` to let Caddy issue HTTPS certificates.

## Environment Variables

Create a `.env` file on the server:

```env
SITE_HOST=:80
POSTGRES_PASSWORD=use-a-long-random-password
RECAPTCHA_SECRET=rotated-recaptcha-secret
MAILGUN_API_KEY=rotated-mailgun-key
```

Do not commit the production `.env` file.

## Deploy

On the Hetzner server:

```bash
git clone <your-repo-url> phaeno.website-api
cd phaeno.website-api
docker compose up -d --build
```

If the server already has Nginx on ports `80` and `443`, start only the API and database:

```bash
docker compose up -d --build api db
```

To use the included Caddy reverse proxy on a server where ports `80` and `443` are available:

```bash
docker compose --profile caddy up -d --build
```

Check logs:

```bash
docker compose logs -f api
docker compose logs -f db
docker compose logs -f caddy
```

## Database Migrations

Run Entity Framework migrations after the database is available.

One production-friendly option is to build an EF migration bundle:

```bash
dotnet ef migrations bundle --project phaeno.api -o migrate
```

Then run it against the production database connection string:

```bash
./migrate --connection "Host=localhost;Port=5432;Database=website;Username=website;Password=..."
```

If running migrations from inside the Docker network, use `Host=db` instead of `Host=localhost`.

## Backups

At minimum:

- Enable Hetzner Cloud server backups.
- Add a PostgreSQL dump job with `pg_dump`.
- Store database backups outside the VM, such as Hetzner Storage Box, S3-compatible storage, or another off-server backup target.

Example manual dump from the server:

```bash
docker compose exec db pg_dump -U website -d website > website-backup.sql
```

## Security Checklist

- Rotate all secrets before production.
- Keep `.env` out of git.
- Do not expose PostgreSQL publicly.
- Restrict SSH to known IPs.
- Keep Docker and the OS updated.
- Use strong unique passwords.
- Confirm Caddy is issuing certificates successfully.
- Confirm the API uses the production connection string.

## Follow-Up Code Change

Before production, add forwarded header support in the API before `UseHttpsRedirection()` so the app correctly detects HTTPS when behind Caddy.


## Connect to Database
Database type: PostgreSQL
Host: localhost
Port: 5433
Database: website
Username: website
Password: Em8/EUid6Gjv6qNZo5HFpXIjpkjD8Ssx
