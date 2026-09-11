# Running the workbench with Docker Compose

A single `docker compose up --build` runs the whole stack on a developer Mac
(Apple Silicon or Intel) or any Linux/Windows host with Docker. The legacy
.NET Framework 4.7.2 API is unchanged and runs under **Mono** (XSP4) in a Linux
container, so no Windows containers are required.

| Service | Image | Host port | Notes |
|---------|-------|-----------|-------|
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` | 14330 → 1433 | SQL Server 2022 Developer, data in the `mssql-data` volume |
| `db-init` | same as `db` | – | one-shot: creates the `ReinsuranceWorkbench` database (mirrors `tools/db-up.ps1`) |
| `api` | `docker/api.Dockerfile` (mono:6.12 + xsp4) | 5055 | applies EF6 migrations and seeds demo data on start |
| `web` | `docker/web.Dockerfile` (node:20 build, nginx) | 5173 → 80 | proxies `/api` to `api:5055`, same as the Vite dev proxy |

## Prerequisites

- Docker Desktop 4.25+ (or Docker Engine 24+ with the Compose plugin).
- **Apple Silicon:** neither the official `mono` image nor SQL Server 2022 has a
  native `linux/arm64` build, so `db`, `db-init` and `api` are pinned to
  `platform: linux/amd64` and run under emulation. Enable
  *Settings → General → "Use Rosetta for x86_64/amd64 emulation on Apple
  Silicon"* in Docker Desktop (Virtualization framework must be on). Without
  Rosetta, SQL Server under QEMU is very slow and may fail to start. If it
  proves unworkable on your machine, replace the `db` image with
  `mcr.microsoft.com/azure-sql-edge` (native arm64, same T-SQL surface for this
  app) and drop the `platform:` line for `db`.
- About 4 GB of free disk for images (SQL Server ≈ 2.3 GB, API ≈ 0.8 GB, web ≈ 75 MB).

## Start

```bash
cp .env.example .env          # optional: set SA_PASSWORD / Sentry DSNs
docker compose up --build
```

First start pulls the images, restores NuGet packages and builds the API and
the web bundle (a few minutes). Subsequent starts are ~10–20 s until the API
reports healthy; on Apple Silicon expect longer because of emulation.

- UI: <http://localhost:5173>
- API: <http://localhost:5055/api/health> → `{"status":"ok","db":"ok","sentry":false}`
- Seeded data: <http://localhost:5055/api/submissions>
- SQL Server: `localhost,14330`, user `sa`, password `SA_PASSWORD` from `.env`

Stop with `docker compose down`; add `-v` to also drop the database volume and
force a fresh migrate + seed on the next start.

## Environment variables (`.env`)

| Variable | Used by | Purpose |
|----------|---------|---------|
| `SA_PASSWORD` | `db`, `db-init`, `api` | SQL Server `sa` password (default `Reins3ure!Dev`) |
| `SENTRY_DSN` | `api` (runtime) | Backend Sentry DSN; leave empty to run without Sentry |
| `VITE_SENTRY_DSN` | `web` (build arg) | Frontend Sentry DSN. Vite inlines it, so run `docker compose up --build web` after changing it |

The API connection string is passed as `REINSURANCE_DB`
(`Server=db,1433;...;TrustServerCertificate=True;Encrypt=False;`).
`Encrypt=False` is required because Mono's `System.Data.SqlClient` cannot
negotiate the TLS handshake SQL Server 2022 uses by default. Never commit real
DSNs; `.env` is git-ignored.

## Firing the planted incidents

All four incidents from [INCIDENTS.md](INCIDENTS.md) reproduce in the compose
stack. With the DSNs unset you get the HTTP 500 / error fallback without
creating Sentry events; set `SENTRY_DSN` / `VITE_SENTRY_DSN` in `.env` and
restart (`docker compose up --build -d`) to have them appear in Sentry.

1. **Atlas layer pricing** (`DivideByZeroException`)

   ```bash
   curl -si -X POST http://localhost:5055/api/treaties/3/price
   ```

   UI: *Pricing → Atlas treaty 3 → layer 3 → Save quote*.

2. **Sakura loss history** (`KeyNotFoundException`)

   ```bash
   curl -si 'http://localhost:5055/api/loss-history?cedentId=6'
   ```

3. **Declining a quoted submission** (`InvalidOperationException`)

   ```bash
   curl -si -X POST -H 'Content-Type: application/json' -d '{"status":4}' \
     http://localhost:5055/api/submissions/6/transition
   curl -s http://localhost:5055/api/submissions/6   # status is still 2 (Quoted)
   ```

   UI: *Submissions → SUB-2026-0006 → Transition → Declined*.

4. **Quota-share treaty detail** (frontend render failure): open
   <http://localhost:5173/treaties/10> (or *Submissions → SUB-2026-0010 →
   treaty link*). The error-boundary fallback shows "Something went wrong" with
   an event ID.

Every API 500 carries an `X-Trace-Id` header and a `traceId` JSON property that
matches the Sentry event tag (see [SENTRY.md](SENTRY.md)).

## How the API runs under Mono

- `docker/api.Dockerfile` restores the four `packages.config` files and builds
  `Reinsurance.Api.csproj` with Mono's `msbuild`, then copies `Global.asax`,
  `Web.config` and `bin/` into a runtime image that serves them with
  `xsp4 --port 5055 --nonstop`. `<system.webServer>` in `Web.config` is ignored
  by XSP; attribute routing works through Mono's `UrlRoutingModule`.
- The `mono:6.12` image is based on Debian Buster, whose apt repositories moved
  to `archive.debian.org`; the Dockerfile rewrites the sources before
  installing `mono-xsp4`.
- Sentry 6.11 initialises under Mono when `SENTRY_DSN` is set
  (`/api/health` reports `"sentry":true`) and is skipped when it is empty.

## Troubleshooting

- `api` exits or `db-init` fails: check `docker compose logs db` — SQL Server
  rejects weak `SA_PASSWORD` values (needs 8+ chars, upper/lower/digit/symbol).
- `/api/health` returns `"db":"down"`: the database is still starting; the API
  waits for `db-init`, but on slow emulated hosts increase the healthcheck
  `retries`/`start_period` in `docker-compose.yml`.
- Reset everything: `docker compose down -v && docker compose up --build`.
