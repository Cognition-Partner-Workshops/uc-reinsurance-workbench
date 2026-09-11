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

Status: fixed (Sentry `BACKEND-REINSURANCE-DEMO-4`).

## Symptom

The loss-history endpoint for Sakura General returned HTTP 500 with a
`KeyNotFoundException`, while the same endpoint worked for the other seeded
cedents.

## Reproduction

1. Start SQL Server with `tools/db-up.ps1` and seed the development database.
2. Start the API with `tools/run-api.ps1`.
3. Submit `GET /api/loss-history?cedentId=6`.
4. Before the fix, observe the HTTP 500 response and its `X-Trace-Id` header.
   After the fix, the request returns HTTP 200 with the 2026 loss included in
   `burningCost` at its untrended (current cost level) value.

The Sakura dataset includes a recent 2026 earthquake loss that is included in
the five-year burning-cost window.

## Expected operational response

Preserve the trace identifier, route the failure for review, and avoid using the
failed burning-cost result for underwriting decisions until the loss trend data
is corrected.

## Root cause

Burning-cost calculation applies year-specific trend factors to every loss in
the lookback window. The 2026 loss had no corresponding factor in the factor
table, so the dictionary indexer threw while calculating the trended total.

## Fix

`LossHistoryService.TrendFactor` resolves the factor for a loss year and clamps
years outside the table to the nearest tabulated year: losses after the latest
factor year use the current-cost-level factor (`1.00`), and losses before the
earliest factor year use the earliest factor. The `PlantedIncident` tests now
pin the fixed behaviour (no exception, 2026 loss included at current cost level).

# Workflow incident: declining a quoted submission

## Symptom

Declining a quoted submission returns HTTP 500 instead of transitioning it to
Declined. The submission remains Quoted.

## Reproduction

1. Start SQL Server with `tools/db-up.ps1` and start the API.
2. Submit `POST /api/submissions/6/transition` with
   `{"status":4}`.
3. Observe the HTTP 500 response.
4. Submit `GET /api/submissions/6` and confirm the status is still Quoted.

The same flow can be exercised in the UI at
`Submissions → SUB-2026-0006 → Transition → Declined`.

## Expected operational response

Keep the submission in its prior state, preserve the trace identifier, and
route the workflow failure for review rather than recording a partial decline.

## Root cause

Decline handling looks up the latest pricing result for the submission's treaty
layers before updating the entity. The seeded Quoted submission has no pricing
result rows, so taking the maximum calculation date from the empty sequence
throws an `InvalidOperationException`.

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
