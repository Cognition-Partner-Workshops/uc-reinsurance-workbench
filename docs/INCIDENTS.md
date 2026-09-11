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
4. Before the fix: observe the unhandled arithmetic exception response and the
   corresponding Sentry event. After the fix: the response is HTTP 200 and the
   Atlas layer 3 result carries a zero expected loss and premium.

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

`LayerHitFraction` now returns `0` when the attachment is at or above `PML250`:
no modelled loss reaches the layer, so its hit fraction, expected loss, and
technical premium are all zero. `AtlasPricingIncidentTests` and the
`PricingCalculatorTests` boundary cases pin this behaviour and verify that no
Sentry event is emitted for the Atlas inputs.
