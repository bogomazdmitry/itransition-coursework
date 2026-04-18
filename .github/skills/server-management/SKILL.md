---
name: server-management
description: 'Manage itransition-coursework production server. Use when: checking server status, viewing logs, debugging production issues, restarting services, managing database, server maintenance.'
---

# Server Management

## Server Info

| Property | Value |
|----------|-------|
| Host | `173.212.212.36` |
| Domain | `itra.taskcal.online` |
| User | `root` |
| OS | Ubuntu 24.04 LTS |
| Project dir | `/opt/itransition-coursework` |

## SSH Connection

**Ask the user for the password before connecting.**

```bash
SSHPASS='<password>' sshpass -e ssh \
  -o StrictHostKeyChecking=no \
  -o PubkeyAuthentication=no \
  root@173.212.212.36 'COMMAND'
```

## Container Management

```bash
# Status
docker compose -f /opt/itransition-coursework/docker-compose.yml ps

# Logs
docker logs itransition-coursework-web --tail 50
docker logs itransition-coursework-db --tail 50

# Restart
cd /opt/itransition-coursework && docker compose restart web
cd /opt/itransition-coursework && docker compose restart db

# Stop / Start all
cd /opt/itransition-coursework && docker compose down
cd /opt/itransition-coursework && docker compose up -d
```

## Database Access

```bash
# Connect to MSSQL (need SA password from .env)
docker exec -it itransition-coursework-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '<password>'

# Quick query
docker exec itransition-coursework-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P '<password>' \
  -Q "SELECT name FROM sys.databases"
```

## Ports

| Service | Container Port | Host Port |
|---------|---------------|-----------|
| .NET Web | 8080 | 127.0.0.1:18888 |
| Azure SQL Edge | 1433 | 127.0.0.1:11433 |

## Nginx

```bash
cat /etc/nginx/sites-available/itra.taskcal.online
nginx -t && systemctl reload nginx
```

## SSL

```bash
certbot certificates
certbot renew
systemctl list-timers | grep certbot
```
