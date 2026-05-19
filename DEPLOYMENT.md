# Echofy — Linode Deployment Guide

## Overview

This guide covers deploying the full Echofy platform on a single Linode instance running **Ubuntu 24.04 LTS**.

### Architecture

```
Internet
    │
    ▼
[nginx :443/80]  ← SSL termination, static file serving, reverse proxy
    ├── /           → React SPA (static files at /var/www/echofy/react)
    ├── /api/       → Echofy.Api        (127.0.0.1:5000)
    ├── /uploads/   → Echofy.Api        (127.0.0.1:5000)
    └── admin.*     → Echofy.Web (MVC)  (127.0.0.1:5001)

Internal only (not exposed externally):
    Echofy.RecommendationApi            (127.0.0.1:5100)

Database:
    PostgreSQL (local)
```

### Services Deployed

| Service | Internal Port | Description |
|---------|--------------|-------------|
| Echofy.Api | 5000 | REST API — consumed by the React SPA and mobile app |
| Echofy.Web | 5001 | ASP.NET Core MVC app (legacy / admin portal) |
| Echofy.RecommendationApi | 5100 | Product recommendation microservice (internal) |
| React SPA | — | Built static files served by nginx |

---

## Prerequisites

### Linode Instance

| Setting | Recommendation |
|---------|---------------|
| Plan | Shared CPU — **4 GB RAM** minimum (8 GB recommended for builds) |
| Image | **Ubuntu 24.04 LTS** |
| Region | Closest to your users |
| SSH key | Add your public key during creation |

### DNS Records

Before deploying, create these DNS A records pointing to your Linode's IP address:

```
yourdomain.com       A   <linode-ip>
www.yourdomain.com   A   <linode-ip>
admin.yourdomain.com A   <linode-ip>
```

Allow up to 15 minutes for DNS propagation before running the SSL step.

---

## First-Time Deployment

### Step 1 — SSH into the Server

```bash
ssh root@<linode-ip>
```

### Step 2 — Download the Deployment Script

```bash
# Clone the repo to get the script, or download it directly:
git clone https://github.com/ajamison/EchofyCloud.git /tmp/echofy-setup
cp /tmp/echofy-setup/deploy.sh /root/deploy.sh
chmod +x /root/deploy.sh
```

### Step 3 — Edit Configuration Variables

Open the script and fill in the values at the top:

```bash
nano /root/deploy.sh
```

| Variable | Description | Example |
|----------|-------------|---------|
| `DOMAIN` | Your primary domain | `echofy.io` |
| `ADMIN_DOMAIN` | Subdomain for the MVC app | `admin.echofy.io` |
| `EMAIL` | Let's Encrypt contact | `admin@echofy.io` |
| `REPO_URL` | GitHub repository URL | (already set) |
| `DB_PASSWORD` | PostgreSQL password | strong random string |
| `JWT_SECRET` | JWT signing key | min 32 characters |

> **Security tip:** Generate strong values with:
> ```bash
> openssl rand -base64 32   # for DB_PASSWORD
> openssl rand -base64 48   # for JWT_SECRET
> ```

### Step 4 — Run the Script

```bash
./deploy.sh
```

The script will:
1. Install system packages (.NET 10, Node.js 22, nginx, PostgreSQL, Certbot)
2. Create the `echofy` system user and app directories
3. Set up the PostgreSQL database and user
4. Clone the repository
5. Write production `appsettings.Production.json` files
6. Build the React SPA
7. Publish all .NET applications
8. Apply database migrations
9. Create and enable `systemd` services
10. Configure nginx virtual hosts
11. Obtain Let's Encrypt SSL certificates
12. Start all services and run a health check

A successful run ends with:
```
=== Health Check ===
[INFO]  Echofy.Api is responding on port 5000
[INFO]  Echofy.Web is responding on port 5001
[INFO]  Echofy.RecommendationApi is responding on port 5100
[INFO]  nginx is running.

Deployment complete!
  App:   https://yourdomain.com
  Admin: https://admin.yourdomain.com
```

---

## Updating to a New Version

To pull and redeploy without reinstalling system packages:

```bash
./deploy.sh --update
```

This skips OS package installation, database creation, and SSL certificate steps — it only fetches the latest code, rebuilds, republishes, runs new migrations, and restarts services.

---

## File Layout on the Server

```
/var/www/echofy/
├── api/        — Published Echofy.Api binaries
├── web/        — Published Echofy.Web binaries
├── rec/        — Published Echofy.RecommendationApi binaries
├── react/      — Built React SPA static files (served by nginx)
├── uploads/    — User-uploaded product images (persisted across deploys)
└── logs/
    ├── api.log
    ├── api-error.log
    ├── web.log
    ├── web-error.log
    ├── rec.log
    └── rec-error.log

/opt/echofy-src/    — Cloned source repository

/etc/nginx/sites-available/
    echofy-app      — nginx config for React SPA + API proxy
    echofy-admin    — nginx config for MVC admin subdomain

/etc/systemd/system/
    echofy-api.service
    echofy-web.service
    echofy-rec.service
```

---

## Managing Services

### Check Status

```bash
systemctl status echofy-api
systemctl status echofy-web
systemctl status echofy-rec
```

### View Logs

```bash
# Live tail
journalctl -u echofy-api -f
journalctl -u echofy-web -f

# File logs
tail -f /var/www/echofy/logs/api-error.log
```

### Restart a Service

```bash
systemctl restart echofy-api
systemctl restart echofy-web
systemctl restart echofy-rec
```

### Restart nginx

```bash
nginx -t && systemctl reload nginx
```

---

## Database Management

### Connect to PostgreSQL

```bash
sudo -u postgres psql -d EchofyDb
```

### Run Migrations Manually

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=localhost;Database=EchofyDb;Username=echofy_app;Password=<your-password>"

dotnet ef database update \
  --project /opt/echofy-src/src/Echofy.Infrastructure/Echofy.Infrastructure.csproj \
  --startup-project /opt/echofy-src/src/Echofy.Api/Echofy.Api.csproj
```

### Backup the Database

```bash
sudo -u postgres pg_dump EchofyDb > /root/echofy-backup-$(date +%Y%m%d).sql
```

### Restore a Backup

```bash
sudo -u postgres psql EchofyDb < /root/echofy-backup-YYYYMMDD.sql
```

---

## SSL Certificate Renewal

Certbot auto-renews via a systemd timer. To verify the timer is active:

```bash
systemctl status certbot.timer
```

To test renewal manually (dry run):

```bash
certbot renew --dry-run
```

---

## Firewall (UFW)

The script does not configure UFW. To harden the server:

```bash
ufw default deny incoming
ufw default allow outgoing
ufw allow ssh
ufw allow http
ufw allow https
ufw enable
```

> Do **not** open ports 5000, 5001, or 5100 — they are internal only and accessed via nginx.

---

## Environment Variables Reference

Production config is stored in `appsettings.Production.json` files placed alongside each published app. These files are written by the deployment script and **not committed to git**.

### Echofy.Api — `/var/www/echofy/api/appsettings.Production.json`

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:Key` | JWT signing secret (min 32 chars) |
| `Jwt:Issuer` | Token issuer (`echofy-api`) |
| `Jwt:Audience` | Token audience (`echofy-mobile`) |
| `App:FrontendBaseUrl` | React SPA URL (for CORS and email links) |

### Echofy.Web — `/var/www/echofy/web/appsettings.Production.json`

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `RecommendationApi:BaseUrl` | Internal URL of the recommendation service |

### Echofy.RecommendationApi — `/var/www/echofy/rec/appsettings.Production.json`

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |

---

## Troubleshooting

### Service won't start

```bash
journalctl -u echofy-api --no-pager -n 50
```

Common causes:
- Wrong connection string in `appsettings.Production.json`
- Database not running: `systemctl status postgresql`
- Port already in use: `ss -tlnp | grep 5000`

### nginx 502 Bad Gateway

The upstream .NET service isn't responding. Check:
```bash
systemctl status echofy-api
curl -I http://127.0.0.1:5000
```

### React SPA shows a blank page

Check the browser console for API errors. Likely cause: the `/api` proxy isn't reaching the backend. Verify:
```bash
curl -I http://127.0.0.1:5000/api/dashboard/stats
```

### Database connection refused

```bash
systemctl status postgresql
sudo -u postgres psql -c "\l"   # list databases
```

### SSL certificate errors

```bash
certbot certificates          # list installed certs
nginx -t                      # validate nginx config
```

---

## Seeded Demo Accounts

After the first deployment the database seeder runs automatically and creates these accounts for testing:

| Email | Password | Role |
|-------|----------|------|
| superadmin@echofy.dev | SuperAdmin@1234! | SuperAdmin |
| admin@echofy.dev | Admin@1234! | Admin |
| manager@echofy.dev | Manager@1234! | Manager |
| customer@echofy.dev | Customer@1234! | Customer |

> Change these passwords immediately in the Admin › Users panel after your first login.

---

## Recommended Post-Deployment Steps

1. **Change seeded passwords** — log in as `superadmin@echofy.dev` and update all demo passwords
2. **Configure SMTP** — add `Smtp:*` settings to `appsettings.Production.json` for invoice and thank-you note emails
3. **Set up database backups** — add a daily cron job: `0 2 * * * sudo -u postgres pg_dump EchofyDb > /root/backups/echofy-$(date +\%Y\%m\%d).sql`
4. **Enable UFW** — follow the firewall section above
5. **Monitor disk usage** — the `uploads/` directory grows as product images are added
