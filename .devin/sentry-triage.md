# Sentry triage automation

You are triaging a Sentry issue for the reinsurance workbench.

1. Read the issue through the Sentry MCP using organization `cognition-workshops`.
2. Identify whether the issue belongs to `backend-reinsurance-demo` or
   `frontend-reinsurance-demo`.
3. Pull the issue tags, especially `traceId`, `treatyId`, `layerId`, `project`,
   route, API path, and HTTP status.
4. Reproduce locally:
   - Backend: `tools\build.ps1`, then `tools\test.ps1 -Integration` when the
     API is available.
   - Frontend: `npm test` from `web`.
5. Trace the failure to its root cause and add a regression test.
6. Open a fix PR with the implementation and regression coverage.
7. Comment the reproduction, root cause, affected project, and verification
   findings back on the issue.

If reproduction fails, report the analysis and evidence only; do not claim a
confirmed root cause.
