# Incidents

The pricing calculator intentionally retains a planted denominator defect in `PricingCalculator.LayerHitFraction`. When a layer attachment equals `PML250`, the second factor divides by zero. The service translates that arithmetic failure to `OverflowException`, and the API exception filter captures it in Sentry when configured.

The correct future behavior is a finite rate-on-line with expected loss equal to zero for that layer.
