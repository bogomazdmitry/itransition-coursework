---
name: server-deploy
description: 'Deploy itransition-coursework to production server. Use when: deploying code, rebuilding containers, updating production, pushing changes to server, redeploy, shipping to prod, release.'
---

# Server Deploy

## Server Info

| Property | Value |
|----------|-------|
| Host | `173.212.212.36` |
| Domain | `itra.taskcal.online` |
| User | `root` |
| OS | Ubuntu 24.04 LTS |
| Project dir | `/opt/itransition-coursework` |

## SSH Connection

Password auth only. Use sshpass with `-e` flag (set `SSHPASS` env var) and disable pubkey:

```bash
SSHPASS='<password>' sshpass -e ssh \
  -o StrictHostKeyChecking=no \
  -o PubkeyAuthentication=no \
  root@173.212.212.36 'COMMAND'
```

**Ask the user for the password before connecting.**

## Architecture on Server

```
Nginx (host, ports 80/443)
  └── / → 127.0.0.1:18888 (.NET Core MVC + SignalR)

Docker Compose (/opt/itransition-coursework/docker-compose.yml):
  ├── db (Azure SQL Edge, 1433→11433)
  └── web (.NET Core 3.1, 8080→18888)
```

## Tech Stack

- **App**: ASP.NET Core 3.1 MVC + Razor Views + Identity + SignalR
- **Database**: Azure SQL Edge (MSSQL) with EF Core
- **Frontend**: Bootstrap + jQuery + TypeScript + SignalR JS client
- **DB Init**: `EnsureCreated()` with 15 retry attempts on startup

## TLS

- Certbot with Nginx plugin, auto-renewal enabled
- Cert: `/etc/letsencrypt/live/itra.taskcal.online/fullchain.pem`
- Key: `/etc/letsencrypt/live/itra.taskcal.online/privkey.pem`
- Nginx config: `/etc/nginx/sites-available/itra.taskcal.online`

## Deploy Procedure

### Full deploy

1. Sync files:
```bash
SSHPASS='<password>' sshpass -e rsync -az \
  -e 'ssh -o StrictHostKeyChecking=no -o PubkeyAuthentication=no' \
  --exclude='.git' --exclude='.env' --exclude='node_modules' \
  --exclude='bin' --exclude='obj' \
  ./ root@173.212.212.36:/opt/itransition-coursework/
```

**IMPORTANT**: Do NOT exclude `wwwroot/lib` — it contains Bootstrap/jQuery/etc. needed at runtime.

2. Build and restart:
```bash
docker compose build --no-cache && docker compose up -d
```

### App-only deploy

```bash
rsync ... && ssh ... 'cd /opt/itransition-coursework && docker compose build web && docker compose up -d web'
```

### Config-only change (.env update)

```bash
ssh ... 'cd /opt/itransition-coursework && docker compose restart web'
```

## Verification

```bash
# Site check
curl -s https://itra.taskcal.online/ | head -5

# Container status
docker compose -f /opt/itransition-coursework/docker-compose.yml ps

# Logs
docker logs itransition-coursework-web --tail 20
docker logs itransition-coursework-db --tail 20
```

## Production .env

Located at `/opt/itransition-coursework/.env` (chmod 600). Contains:
- `MSSQL_SA_PASSWORD` — strong password for Azure SQL Edge

## Troubleshooting

| Problem | Solution |
|---------|----------|
| DB connection error | Check `docker logs itransition-coursework-web`, verify MSSQL_SA_PASSWORD matches |
| MSSQL password rejected | Azure SQL Edge requires **complex** passwords: min 8 chars with uppercase + lowercase + digits + symbols. E.g. `B7e4f2A9!c1D830` |
| App crashes after 15 retries | Azure SQL Edge takes ~30s to start. If `docker compose up -d` starts both at once, web may exhaust retries. Fix: `docker compose restart web` after DB is ready |
| Missing styles (unstyled page) | `wwwroot/lib/` was not synced. Re-rsync without `--exclude='wwwroot/lib'` and rebuild: `docker compose build web && docker compose up -d web` |
| Nginx 502 | `docker compose ps` — check containers running |
| TLS cert expired | `certbot renew` (auto-renewal active) |
| Disk full | `docker system prune -a` |
