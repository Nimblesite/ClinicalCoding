# Docker Setup

3 containers. That's it.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                        app                              │
│  Gatekeeper:5002  Clinical:5080  Scheduling:5001       │
│  ICD10:5090       ClinicalSync   SchedulingSync        │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────┼────────────────────────────────┐
│                       db                                │
│  Postgres:5432                                          │
│  ├── gatekeeper                                         │
│  ├── clinical                                           │
│  ├── scheduling                                         │
│  └── icd10                                              │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                     dashboard                           │
│  nginx:5173 (built TypeScript React dashboard)          │
└─────────────────────────────────────────────────────────┘
```

## Why This Split?

| Container | Runtime | Why separate |
|-----------|---------|--------------|
| db | Postgres | Stateful. Don't rebuild the database. |
| app | .NET 9 | All APIs tightly coupled. Same codebase, same deploy. |
| dashboard | nginx | Static TypeScript React build. Different runtime. |

## Dashboard Note

The dashboard image builds `Dashboard/dashboard-ts` with pnpm and serves the
generated `dist/` assets through nginx.

## Usage

```bash
# Start everything
make start-docker

# Rebuild containers
make start-docker BUILD=1

# Fresh start (wipe databases)
cd docker && docker compose down -v
make start-docker
```

## Ports

| Service | Port |
|---------|------|
| Postgres | 5432 |
| Gatekeeper API | 5002 |
| Clinical API | 5080 |
| Scheduling API | 5001 |
| ICD10 API | 5090 |
| Dashboard | 5173 |

## Files

```
docker/
├── docker-compose.yml    # 3 services
├── Dockerfile.app        # All .NET services
├── Dockerfile.dashboard  # nginx + static files
├── start-services.sh     # Entrypoint for app container
├── init-db/
│   └── init.sql          # Creates all 4 databases
└── nginx.conf            # Dashboard config
```
