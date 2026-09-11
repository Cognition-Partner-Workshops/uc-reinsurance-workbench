# Architecture

The workbench is a classic ASP.NET Web API 2 application targeting .NET Framework 4.7.2.

- `Reinsurance.Core` contains domain entities and persistence abstractions.
- `Reinsurance.Data` contains EF6 mappings, context, migrations, hooks, and deterministic demo data.
- `Reinsurance.Services` contains application services and pure pricing, exposure, and referral logic.
- `Reinsurance.Api` hosts the HTTP API under IIS Express and wires Autofac and Sentry.
- `Reinsurance.Tests` contains NUnit tests.

EF6 automatic migrations are disabled. The explicit `InitialCreate` migration
contains the schema and generated model snapshot; application startup migrates
the database and then seeds it when the `Cedent` table is empty.

Each business area keeps its entity, mapping, service, and model files together.
Repositories expose EF queryables to the services, while save hooks provide
auditing at the context boundary. Autofac composes the per-request context,
repositories, hooks, services, MVC controllers, and Web API controllers.

Portfolio-limit referral logic derives region and peril scope from each
submission's exposure records. A limit matches a layer when each non-wildcard
scope is present in that submission. Bound treaty layers are aggregated only
when their submissions match the same scope, and the current layer is then
added before comparing the aggregate and portfolio PML250 thresholds.

## Presentation (React)

The `web/` application is a Vite React 18 client. `PortfolioScreen` composes
submission, treaty pricing, cat-model, and exposure endpoints for the portfolio
dashboard. `SubmissionQueueScreen` uses `/api/submissions` plus the reference
endpoints and creates new submissions. `SubmissionDetailScreen` drives status
transitions. `TreatyDetailScreen` joins `/api/treaties/{id}`, its submission,
pricing, cat-model, and cedent data. `PricingScreen` provides a client-side
what-if preview and saves authoritative pricing through
`POST /api/treaties/{id}/price`, with binding through
`POST /api/treaties/{id}/bind`.
