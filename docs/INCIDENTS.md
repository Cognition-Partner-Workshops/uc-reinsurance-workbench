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
