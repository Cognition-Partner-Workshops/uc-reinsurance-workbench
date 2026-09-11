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
