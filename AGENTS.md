# AGENTS.md

- The API Docker image uses `ENV MONO_ENV_OPTIONS=--interp` in both stages of `docker/api.Dockerfile`. On this ARM64 Docker Desktop host, `nuget help` in the amd64 `mono:6.12` image reproduced an `x86-codegen.h:410` offset assertion; interpreter mode bypassed it and the API build succeeded.
- Frontend API client regression command: `cd web && npm test -- src/api/client.test.ts`.
- `curl /api/health` alone does not cover browser requests — it does not exercise the frontend fetch path or content types returned to the SPA.
- The API disables standalone ASP.NET Web Pages routing with webpages:Enabled=false. Otherwise Chrome/Devin User-Agent headers trigger Mono browser-capability detection and an IndexOutOfRangeException before API controllers run. Regression checks must include browser User-Agent headers through port 5173.
