# Reinsurance Workbench

Classic ASP.NET MVC/Web API 2 reinsurance workbench targeting .NET Framework 4.7.2.

The solution is intentionally structured like a SmartStore-style legacy application:
domain entities and EF mappings are split by business area, repositories and save hooks
sit below the services layer, and the MVC/Web API host composes the cross-cutting
dependencies with Autofac.

## Local workflow

```powershell
tools\db-up.ps1
tools\build.ps1
tools\test.ps1
tools\run-api.ps1
```

By default, `tools\test.ps1` runs unit and planted-incident tests while excluding
the integration category:

```powershell
.\tools\test.ps1
```

Run the integration suite against the running API with:

```powershell
.\tools\test.ps1 -Integration
```

To pass a VSTest filter explicitly, use `-Filter`:

```powershell
.\tools\test.ps1 -Filter "TestCategory=PlantedIncident"
```

The development SQL Server password and connection string are intentionally local-only defaults. Set `REINSURANCE_DB` or `MSSQL_SA_PASSWORD` to override them.

The default local database is SQL Server on `localhost,14330`. `tools/db-up.ps1`
starts the `reinsurance-sql` container and the API applies the explicit EF6
`InitialCreate` migration before deterministic demo data is seeded.

Useful endpoints include `/api/health`, `/api/submissions`, submission exposure and
cat-model views, treaty pricing, treaty binding, and the guarded
`POST /api/admin/reseed` operation.

## Web UI

The React/TypeScript workbench is under `web/` and uses Vite with a development
proxy to the API on `http://localhost:5055`:

```powershell
cd web
npm ci
npm run dev
```

The frontend scripts are `dev`, `build`, `preview`, `test`, `lint`, `typecheck`,
and `format`. Start the SQL container and API with the commands above before
opening the UI at `http://localhost:5173`.
