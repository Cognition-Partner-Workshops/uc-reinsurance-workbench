# Pricing incident: Atlas layer pricing

Status: fixed (Sentry `BACKEND-REINSURANCE-DEMO-1`).

## Symptom

Pricing the third layer of `Atlas Specialty Property Cat XoL 2026` returned HTTP
500. When Sentry is enabled, the captured event is tagged with the treaty and
layer identifiers.

## Reproduction

1. Start SQL Server with `tools/db-up.ps1`.
2. Start the API with `tools/run-api.ps1`.
3. Submit `POST /api/treaties/{atlasTreatyId}/price`.
4. Before the fix, observe the unhandled arithmetic exception response and the
   corresponding Sentry event. After the fix, the request returns HTTP 200 with
   a zero expected loss and technical premium for layer 3.

The Atlas layer attachment equals the submission portfolio `PML250`, making the
case deterministic in the demo dataset.

## Expected operational response

Treat pricing failures as application errors, preserve the treaty and layer
tags, and route the incident for review rather than binding the treaty.

## Root cause

`PricingCalculator.LayerHitFraction` calculates the second layer-hit factor by
dividing the layer limit by the remaining portfolio loss after attachment. The
Atlas boundary case left that denominator at zero. The raw
`DivideByZeroException` propagated through pricing and was captured by the API
exception handling path.

## Fix

`LayerHitFraction` now returns `0` when the attachment reaches or exceeds
`PML250`: the layer sits entirely above the modelled portfolio loss, so its
expected loss is zero. The `PlantedIncident` tests now pin the fixed behaviour
(finite hit fraction, no exception, no Sentry event).

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
>>>>>>> origin/main
