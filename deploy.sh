#!/bin/bash
# =============================================================================
# Echofy — Linode Deployment Script (Ubuntu 24.04 LTS)
# Run as root on a fresh Linode instance, or re-run for updates.
# =============================================================================
set -euo pipefail

# =============================================================================
# CONFIGURATION — edit these before running
# =============================================================================
DOMAIN="yourdomain.com"                          # e.g. echofy.io
EMAIL="admin@yourdomain.com"                     # Let's Encrypt contact email

REPO_URL="https://github.com/ajamison/EchofyCloud.git"
REPO_BRANCH="main"

DB_NAME="EchofyDb"
DB_USER="echofy_app"
DB_PASSWORD="CHANGE_THIS_DB_PASSWORD"            # strong password

JWT_SECRET="CHANGE_THIS_JWT_SECRET_MIN_32_CHARS" # min 32 characters
JWT_ISSUER="echofy-api"
JWT_AUDIENCE="echofy-mobile"

APP_USER="echofy"
SRC_DIR="/opt/echofy-src"
APP_DIR="/var/www/echofy"

# Internal ports (not exposed externally — nginx proxies to these)
PORT_API=5000
PORT_REC=5100

# =============================================================================
# HELPERS
# =============================================================================
BOLD="\e[1m"; RESET="\e[0m"; GREEN="\e[32m"; YELLOW="\e[33m"; RED="\e[31m"

info()    { echo -e "${GREEN}[INFO]${RESET}  $*"; }
warn()    { echo -e "${YELLOW}[WARN]${RESET}  $*"; }
section() { echo -e "\n${BOLD}=== $* ===${RESET}"; }

require_root() {
  if [[ $EUID -ne 0 ]]; then
    echo -e "${RED}[ERROR]${RESET} This script must be run as root."; exit 1
  fi
}

# =============================================================================
# 1. SYSTEM PREREQUISITES
# =============================================================================
install_prerequisites() {
  section "Installing System Prerequisites"

  apt-get update -y
  apt-get upgrade -y
  apt-get install -y \
    curl wget git unzip nginx certbot python3-certbot-nginx \
    postgresql postgresql-contrib \
    apt-transport-https software-properties-common

  # .NET 10 SDK
  if ! dotnet --version &>/dev/null; then
    info "Installing .NET 10 SDK..."
    wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
    dpkg -i /tmp/packages-microsoft-prod.deb
    apt-get update -y
    apt-get install -y dotnet-sdk-10.0
  else
    info ".NET already installed: $(dotnet --version)"
  fi

  # Node.js 22 (LTS)
  if ! node --version &>/dev/null; then
    info "Installing Node.js 22..."
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash -
    apt-get install -y nodejs
  else
    info "Node.js already installed: $(node --version)"
  fi

  # EF Core CLI tool
  if ! dotnet ef --version &>/dev/null 2>&1; then
    info "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
    export PATH="$PATH:/root/.dotnet/tools"
  fi

  info "Prerequisites installed."
}

# =============================================================================
# 2. APP USER & DIRECTORIES
# =============================================================================
setup_user_and_dirs() {
  section "Setting Up App User and Directories"

  if ! id "$APP_USER" &>/dev/null; then
    useradd --system --no-create-home --shell /bin/false "$APP_USER"
    info "Created system user: $APP_USER"
  else
    info "User $APP_USER already exists."
  fi

  mkdir -p "$APP_DIR"/{api,rec,react,uploads,logs}
  chown -R "$APP_USER":"$APP_USER" "$APP_DIR"
  info "App directories ready at $APP_DIR"
}

# =============================================================================
# 3. POSTGRESQL
# =============================================================================
setup_database() {
  section "Setting Up PostgreSQL"

  systemctl enable --now postgresql

  # Create DB user if not exists
  if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_roles WHERE rolname='$DB_USER'" | grep -q 1; then
    sudo -u postgres psql -c "CREATE USER $DB_USER WITH PASSWORD '$DB_PASSWORD';"
    info "Created DB user: $DB_USER"
  else
    info "DB user $DB_USER already exists — updating password."
    sudo -u postgres psql -c "ALTER USER $DB_USER WITH PASSWORD '$DB_PASSWORD';"
  fi

  # Create database if not exists
  if ! sudo -u postgres psql -lqt | cut -d\| -f1 | grep -qw "$DB_NAME"; then
    sudo -u postgres psql -c "CREATE DATABASE \"$DB_NAME\" OWNER $DB_USER;"
    info "Created database: $DB_NAME"
  else
    info "Database $DB_NAME already exists."
  fi

  sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE \"$DB_NAME\" TO $DB_USER;"
}

# =============================================================================
# 4. FETCH SOURCE
# =============================================================================
fetch_source() {
  section "Fetching Source Code"

  if [[ -d "$SRC_DIR/.git" ]]; then
    info "Pulling latest changes..."
    git -C "$SRC_DIR" fetch origin
    git -C "$SRC_DIR" reset --hard "origin/$REPO_BRANCH"
    git -C "$SRC_DIR" clean -fd
  else
    info "Cloning repository..."
    git clone --branch "$REPO_BRANCH" "$REPO_URL" "$SRC_DIR"
  fi
  info "Source at $SRC_DIR"
}

# =============================================================================
# 5. WRITE PRODUCTION appsettings
# =============================================================================
write_appsettings() {
  section "Writing Production appsettings"

  # ── Echofy.Api ──────────────────────────────────────────────────────────────
  cat > "$SRC_DIR/src/Echofy.Api/appsettings.Production.json" <<EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD"
  },
  "Jwt": {
    "Key": "$JWT_SECRET",
    "Issuer": "$JWT_ISSUER",
    "Audience": "$JWT_AUDIENCE",
    "ExpiryMinutes": 60
  },
  "App": {
    "FrontendBaseUrl": "https://$DOMAIN"
  },
  "AllowedHosts": "*"
}
EOF

  # ── Echofy.RecommendationApi ─────────────────────────────────────────────────
  cat > "$SRC_DIR/src/Echofy.RecommendationApi/appsettings.Production.json" <<EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD"
  },
  "AllowedHosts": "*"
}
EOF

  info "Production appsettings written."
}

# =============================================================================
# 6. UPDATE CORS FOR PRODUCTION
# =============================================================================
patch_cors() {
  section "Patching CORS Origins for Production"

  # Replace the ReactSpa CORS policy with the production domain
  sed -i \
    "s|WithOrigins(\"http://localhost:5173\", \"http://localhost:4173\")|WithOrigins(\"https://$DOMAIN\", \"https://www.$DOMAIN\")|g" \
    "$SRC_DIR/src/Echofy.Api/Program.cs"

  info "CORS updated for https://$DOMAIN"
}

# =============================================================================
# 7. BUILD REACT SPA
# =============================================================================
build_react() {
  section "Building React SPA"

  local react_src="$SRC_DIR/src/Echofy.Web.React"

  # Write production .env so Vite knows the API base URL
  cat > "$react_src/.env.production" <<EOF
VITE_API_BASE_URL=https://$DOMAIN
EOF

  cd "$react_src"
  npm ci
  npm run build

  # Copy built files to web root
  rm -rf "$APP_DIR/react"
  cp -r "$react_src/dist" "$APP_DIR/react"
  chown -R "$APP_USER":"$APP_USER" "$APP_DIR/react"

  info "React SPA built and deployed to $APP_DIR/react"
}

# =============================================================================
# 8. PUBLISH .NET APPS
# =============================================================================
publish_dotnet() {
  section "Publishing .NET Applications"

  local publish_flags="-c Release --self-contained false -r linux-x64"

  # Echofy.Api
  dotnet publish "$SRC_DIR/src/Echofy.Api/Echofy.Api.csproj" \
    $publish_flags -o "$APP_DIR/api"
  chown -R "$APP_USER":"$APP_USER" "$APP_DIR/api"
  info "Echofy.Api published."

  # Echofy.RecommendationApi
  dotnet publish "$SRC_DIR/src/Echofy.RecommendationApi/Echofy.RecommendationApi.csproj" \
    $publish_flags -o "$APP_DIR/rec"
  chown -R "$APP_USER":"$APP_USER" "$APP_DIR/rec"
  info "Echofy.RecommendationApi published."
}

# =============================================================================
# 9. RUN DATABASE MIGRATIONS
# =============================================================================
run_migrations() {
  section "Running Database Migrations"

  export PATH="$PATH:/root/.dotnet/tools"
  export ASPNETCORE_ENVIRONMENT=Production
  export DOTNET_ENVIRONMENT=Production

  # Set the connection string so EF can connect
  export ConnectionStrings__DefaultConnection="Host=localhost;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD"

  dotnet ef database update \
    --project "$SRC_DIR/src/Echofy.Infrastructure/Echofy.Infrastructure.csproj" \
    --startup-project "$SRC_DIR/src/Echofy.Api/Echofy.Api.csproj" \
    --no-build 2>/dev/null || \
  dotnet ef database update \
    --project "$SRC_DIR/src/Echofy.Infrastructure/Echofy.Infrastructure.csproj" \
    --startup-project "$SRC_DIR/src/Echofy.Api/Echofy.Api.csproj"

  info "Migrations applied."
}

# =============================================================================
# 10. SYSTEMD SERVICES
# =============================================================================
create_services() {
  section "Creating systemd Services"

  # ── Echofy.Api ───────────────────────────────────────────────────────────────
  cat > /etc/systemd/system/echofy-api.service <<EOF
[Unit]
Description=Echofy REST API
After=network.target postgresql.service
Requires=postgresql.service

[Service]
Type=simple
User=$APP_USER
WorkingDirectory=$APP_DIR/api
ExecStart=/usr/bin/dotnet $APP_DIR/api/Echofy.Api.dll
Restart=always
RestartSec=10
SyslogIdentifier=echofy-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:$PORT_API
StandardOutput=append:$APP_DIR/logs/api.log
StandardError=append:$APP_DIR/logs/api-error.log

[Install]
WantedBy=multi-user.target
EOF

  # ── Echofy.RecommendationApi ─────────────────────────────────────────────────
  cat > /etc/systemd/system/echofy-rec.service <<EOF
[Unit]
Description=Echofy Recommendation API
After=network.target postgresql.service
Requires=postgresql.service

[Service]
Type=simple
User=$APP_USER
WorkingDirectory=$APP_DIR/rec
ExecStart=/usr/bin/dotnet $APP_DIR/rec/Echofy.RecommendationApi.dll
Restart=always
RestartSec=10
SyslogIdentifier=echofy-rec
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:$PORT_REC
StandardOutput=append:$APP_DIR/logs/rec.log
StandardError=append:$APP_DIR/logs/rec-error.log

[Install]
WantedBy=multi-user.target
EOF

  systemctl daemon-reload
  systemctl enable echofy-api echofy-rec
  info "systemd services installed and enabled."
}

# =============================================================================
# 11. NGINX CONFIGURATION
# =============================================================================
configure_nginx() {
  section "Configuring nginx"

  # ── React SPA + API proxy (main domain) ──────────────────────────────────────
  cat > /etc/nginx/sites-available/echofy-app <<EOF
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;

    root $APP_DIR/react;
    index index.html;

    # React SPA — serve index.html for all non-file routes
    location / {
        try_files \$uri \$uri/ /index.html;
    }

    # Proxy /api and /uploads to Echofy.Api
    location /api/ {
        proxy_pass         http://127.0.0.1:$PORT_API;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade \$http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host \$host;
        proxy_set_header   X-Real-IP \$remote_addr;
        proxy_set_header   X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_read_timeout 90s;
    }

    location /uploads/ {
        proxy_pass         http://127.0.0.1:$PORT_API;
        proxy_http_version 1.1;
        proxy_set_header   Host \$host;
        proxy_set_header   X-Real-IP \$remote_addr;
        proxy_set_header   X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
    }

    # Static asset caching
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    client_max_body_size 20M;
}
EOF

  # Enable site
  ln -sf /etc/nginx/sites-available/echofy-app /etc/nginx/sites-enabled/echofy-app
  rm -f /etc/nginx/sites-enabled/default

  nginx -t
  systemctl reload nginx
  info "nginx configured."
}

# =============================================================================
# 12. SSL CERTIFICATES (Let's Encrypt)
# =============================================================================
setup_ssl() {
  section "Obtaining SSL Certificates"

  certbot --nginx \
    -d "$DOMAIN" -d "www.$DOMAIN" \
    --non-interactive --agree-tos --email "$EMAIL" \
    --redirect

  # Auto-renew via systemd timer (comes with certbot package on Ubuntu 24.04)
  systemctl enable --now certbot.timer 2>/dev/null || true

  info "SSL certificates installed."
}

# =============================================================================
# 13. START / RESTART SERVICES
# =============================================================================
restart_services() {
  section "Starting Services"

  systemctl restart echofy-api echofy-rec
  systemctl reload nginx

  info "All services restarted."
}

# =============================================================================
# 14. HEALTH CHECK
# =============================================================================
health_check() {
  section "Health Check"

  sleep 5  # give services a moment to bind

  local ok=true

  check_service() {
    local name=$1 port=$2
    if curl -sf "http://127.0.0.1:$port" &>/dev/null || \
       curl -sf "http://127.0.0.1:$port/health" &>/dev/null || \
       curl -sf "http://127.0.0.1:$port/api/dashboard/stats" &>/dev/null; then
      info "$name is responding on port $port"
    else
      warn "$name may still be starting on port $port — check logs if issues persist"
      ok=false
    fi
  }

  check_service "Echofy.Api"               $PORT_API
  check_service "Echofy.RecommendationApi" $PORT_REC

  if systemctl is-active --quiet nginx; then
    info "nginx is running."
  else
    warn "nginx is NOT running — check: systemctl status nginx"
    ok=false
  fi

  if $ok; then
    echo -e "\n${GREEN}${BOLD}Deployment complete!${RESET}"
    echo -e "  App: https://$DOMAIN"
  else
    echo -e "\n${YELLOW}Deployment finished with warnings. Check logs in $APP_DIR/logs/${RESET}"
  fi
}

# =============================================================================
# MAIN
# =============================================================================
main() {
  require_root

  # Check for --update flag (skips OS package install)
  local update_only=false
  [[ "${1:-}" == "--update" ]] && update_only=true

  if ! $update_only; then
    install_prerequisites
    setup_user_and_dirs
    setup_database
  fi

  fetch_source
  write_appsettings
  patch_cors
  build_react
  publish_dotnet
  run_migrations
  create_services
  configure_nginx

  if ! $update_only; then
    setup_ssl
  fi

  restart_services
  health_check
}

main "$@"
