# Architecture

The workbench is a classic ASP.NET Web API 2 application targeting .NET Framework 4.7.2.

- `Reinsurance.Core` contains domain entities and persistence abstractions.
- `Reinsurance.Data` contains EF6 mappings, context, migrations, hooks, and deterministic demo data.
- `Reinsurance.Services` contains application services and pure pricing, exposure, and referral logic.
- `Reinsurance.Api` hosts the HTTP API under IIS Express and wires Autofac and Sentry.
- `Reinsurance.Tests` contains NUnit tests.

EF6 automatic migrations are enabled as the portable fallback for this hand-written classic-project implementation. The application initializes the database and then seeds it when the `Cedent` table is empty.
