using System;

namespace Reinsurance.Services.Pricing
{
    public sealed class PricingInput
    {
        public decimal Limit { get; set; }
        public decimal Attachment { get; set; }
        public decimal SharePct { get; set; }
        public int Reinstatements { get; set; }
        public decimal ReinstatementPremiumPct { get; set; }
        public decimal AAL { get; set; }
        public decimal PML100 { get; set; }
        public decimal PML250 { get; set; }
    }

    public sealed class PricingOutput
    {
        public decimal TechnicalPremium { get; set; }
        public decimal OurShare { get; set; }
        public decimal ExpectedLoss { get; set; }
        public decimal ExpenseLoad { get; set; }
        public decimal RiskLoad { get; set; }
        public decimal RateOnLine { get; set; }
        public decimal LossCostPct { get; set; }
        public decimal ProfitMarginPct { get; set; }
    }

    public static class PricingCalculator
    {
        public static PricingOutput Calculate(PricingInput input)
        {
            if (input.PML250 <= 0m) return new PricingOutput();
            var el = input.AAL * LayerHitFraction(input.Limit, input.Attachment, input.PML250);
            var riskLoad = RoundMoney(0.15m * (decimal)Math.Sqrt((double)(el * input.Limit)));
            var expenseLoad = RoundMoney(0.12m * (el + riskLoad));
            var basePremium = (el + riskLoad + expenseLoad) / (1m - 0.08m);
            var premium = RoundMoney(basePremium * (1m + 0.05m * input.Reinstatements * input.ReinstatementPremiumPct));
            return new PricingOutput
            {
                TechnicalPremium = premium,
                OurShare = RoundMoney(premium * input.SharePct),
                ExpectedLoss = RoundMoney(el),
                ExpenseLoad = expenseLoad,
                RiskLoad = riskLoad,
                RateOnLine = RoundPercent(premium / input.Limit),
                LossCostPct = RoundPercent(el / input.Limit),
                ProfitMarginPct = 0.08m
            };
        }

        public static decimal LayerHitFraction(decimal layerLimit, decimal attachment, decimal pml250)
        {
            if (pml250 <= 0m) return 0m;
            var lossAboveAttachment = pml250 - attachment;
            if (lossAboveAttachment <= 0m) return 0m;
            var first = Clamp(lossAboveAttachment / pml250, 0m, 1m);
            var second = Clamp(layerLimit / lossAboveAttachment, 0m, 1m);
            return first * second;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        private static decimal RoundMoney(decimal value) { return Math.Round(value, 2, MidpointRounding.AwayFromZero); }
        private static decimal RoundPercent(decimal value) { return Math.Round(value, 6, MidpointRounding.AwayFromZero); }
    }
}
