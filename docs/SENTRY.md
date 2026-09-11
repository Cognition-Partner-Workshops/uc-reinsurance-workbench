# Sentry

The workbench uses Sentry for backend errors and transactions and for frontend
errors, API failures, browser traces, and replay-on-error. DSNs are supplied only
through environment variables and must never be committed to the repository.

## Projects and environment variables

Sentry organization: `cognition-workshops`

- Backend project: `backend-reinsurance-demo`
- Frontend project: `frontend-reinsurance-demo`

Backend variables:

- `SENTRY_DSN`
- `REINSURANCE_ENVIRONMENT`
- `REINSURANCE_RELEASE`

Frontend variables:

- `VITE_SENTRY_DSN`
- `VITE_SENTRY_ENVIRONMENT`
- `VITE_SENTRY_RELEASE`

For local frontend work, copy `web/.env.example` to a local `.env` file and
populate the values outside source control.

## What is captured

The ASP.NET API captures unhandled 500-level exceptions, request transactions,
small request bodies, stack traces, and trace metadata. It does not send 404 or
409 responses to Sentry. Every API exception response includes an `X-Trace-Id`
header and a matching `traceId` JSON property.

The React client captures unhandled render errors through its error boundary,
server errors with HTTP status 500 or higher, browser navigation transactions,
and replay data for errors. It does not capture 4xx API responses. Default PII
capture is disabled in both applications.

The API trace ID is returned to the client as `ApiError.traceId` and attached to
the frontend Sentry event as the `traceId` tag. This links a UI error to the
corresponding backend event. API events also include route and frontend events
include the API path and HTTP status.

## Reproducing test events

Backend Atlas error:

```powershell
Invoke-WebRequest -Method Post http://localhost:5055/api/treaties/3/price
```

This exercised the planted Atlas pricing failure (`BACKEND-REINSURANCE-DEMO-1`).
Since the `LayerHitFraction` fix it returns HTTP 200 and no longer produces a
backend Sentry event; see [docs/INCIDENTS.md](INCIDENTS.md).

Frontend Atlas error:

1. Open the pricing screen for Atlas treaty 3.
2. Select layer 3.
3. Click **Save quote**.

Before the fix the API returned the planted 500 response and the frontend
displayed the trace ID and captured the server error; the quote now saves
successfully.

## Webhook path to Devin

Create a Devin Automation with a Webhook trigger under **Settings →
Automations**, then copy its URL. In Sentry, create an Internal Integration
under **Settings → Developer Settings** with the webhook URL and enable the
`issue` and `error` webhooks. Alternatively, configure an Alert Rule with a
webhook action.

Use this placeholder while configuring the integration:

```text
<DEVIN_AUTOMATION_WEBHOOK_URL>
```

The issue-alert webhook payload fields used by triage are:

- `data.event.title`
- `data.event.web_url`
- `data.event.tags`

Tags of interest include `traceId`, `treatyId`, `layerId`, and `project`.
