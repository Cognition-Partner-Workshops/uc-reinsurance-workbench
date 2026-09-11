# Pricing incident: Atlas layer pricing

## Symptom

Pricing the third layer of `Atlas Specialty Property Cat XoL 2026` returns HTTP
500. When Sentry is enabled, the captured event is tagged with the treaty and
layer identifiers.

## Reproduction

1. Start SQL Server with `tools/db-up.ps1`.
2. Start the API with `tools/run-api.ps1`.
3. Submit `POST /api/treaties/{atlasTreatyId}/price`.
4. Observe the unhandled arithmetic exception response and the corresponding
   Sentry event.

The Atlas layer attachment equals the submission portfolio `PML250`, making the
case deterministic in the demo dataset.

## Expected operational response

Treat pricing failures as application errors, preserve the treaty and layer
tags, and route the incident for review rather than binding the treaty.

## Root cause

`PricingCalculator.LayerHitFraction` calculates the second layer-hit factor by
dividing the layer limit by the remaining portfolio loss after attachment. The
Atlas boundary case leaves that denominator at zero. The raw
`DivideByZeroException` propagates through pricing and is captured by the API
exception handling path.

The planted incident test pins this failure mode and verifies that the exception
is captured by Sentry. If the incident is fixed, update the test to assert a
finite hit fraction and revise the endpoint evidence accordingly.

# Loss-history incident: Sakura General burning cost

## Symptom

The loss-history endpoint for Sakura General returns HTTP 500 with a
`KeyNotFoundException`, while the same endpoint works for the other seeded
cedents.

## Reproduction

1. Start SQL Server with `tools/db-up.ps1` and seed the development database.
2. Start the API with `tools/run-api.ps1`.
3. Submit `GET /api/loss-history?cedentId=6`.
4. Observe the HTTP 500 response and its `X-Trace-Id` header.

The Sakura dataset includes a recent 2026 earthquake loss that is included in
the five-year burning-cost window.

## Expected operational response

Preserve the trace identifier, route the failure for review, and avoid using the
failed burning-cost result for underwriting decisions until the loss trend data
is corrected.

## Root cause

Burning-cost calculation applies year-specific trend factors to every loss in
the lookback window. The 2026 loss has no corresponding factor in the factor
table, so dictionary lookup throws while calculating the trended total.

# Workflow incident: declining a quoted submission

Status: fixed (Sentry `BACKEND-REINSURANCE-DEMO-3`).

## Symptom

Declining a quoted submission returned HTTP 500 instead of transitioning it to
Declined. The submission remained Quoted.

## Reproduction

1. Start SQL Server with `tools/db-up.ps1` and start the API.
2. Submit `POST /api/submissions/6/transition` with
   `{"status":4}`.
3. Before the fix, observe the HTTP 500 response; after the fix the request
   returns HTTP 200 with `status` Declined.
4. Submit `GET /api/submissions/6` and confirm the status (Quoted before the
   fix, Declined after it).

The same flow can be exercised in the UI at
`Submissions → SUB-2026-0006 → Transition → Declined`.

## Expected operational response

Keep the submission in its prior state, preserve the trace identifier, and
route the workflow failure for review rather than recording a partial decline.

## Root cause

Decline handling looks up the latest pricing result for the submission's treaty
layers before updating the entity. The seeded Quoted submission has no pricing
result rows, so `Max(x => x.CalculatedOn)` over the empty set threw an
`InvalidOperationException`: against SQL Server, EF6 translated it to
`SELECT MAX(CalculatedOn)`, which returns `NULL`, and failed to materialize that
into the non-nullable `DateTime` ("The cast to value type 'System.DateTime'
failed because the materialized value is null").

## Fix

`SubmissionService.Transition` now projects the maximum as `DateTime?`, so an
empty result yields `null` instead of throwing, and the decline note records
`no quote on record` when no pricing result exists. The `PlantedIncident` tests
now pin the fixed behaviour (decline succeeds with and without pricing results).

# UI incident: quota-share treaty detail

## Symptom

Opening the quota-share treaty detail page causes a frontend render failure and
shows the application error fallback.

## Reproduction

1. Start the API with the seeded development database.
2. Open `Submissions → SUB-2026-0010` and follow the treaty link, or navigate
   directly to `/treaties/10`.
3. Observe the frontend error fallback.

## Expected operational response

Use the error-boundary event identifier to route the UI failure for review and
keep the treaty available for investigation without attempting a bind action.

## Root cause

The seeded quota-share treaty has no treaty layers. The treaty detail summary
derives its exhaustion point from the top layer without handling an empty layer
collection, so rendering the summary accesses a missing layer.
