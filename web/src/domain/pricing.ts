import type { CatModelResultModel, TreatyLayerModel } from "../api/types";

export interface ClientPricingResult {
    error: boolean;
    expectedLoss: number;
    riskLoad: number;
    expenseLoad: number;
    technicalPremium: number;
    ourShareLine: number;
    rateOnLine: number;
    lossCostPct: number;
    profitMarginPct: number;
}

const round = (value: number, digits: number) => {
    const factor = 10 ** digits;
    return (Math.sign(value) * Math.round(Math.abs(value) * factor)) / factor;
};

const clamp = (value: number, min: number, max: number) => Math.min(max, Math.max(min, value));

export function priceLayer(
    layer: TreatyLayerModel,
    model: CatModelResultModel,
): ClientPricingResult {
    if (model.pml250 <= 0) {
        return {
            error: false,
            expectedLoss: 0,
            riskLoad: 0,
            expenseLoad: 0,
            technicalPremium: 0,
            ourShareLine: 0,
            rateOnLine: 0,
            lossCostPct: 0,
            profitMarginPct: 0,
        };
    }
    const denominator = model.pml250 - layer.attachment;
    if (denominator === 0) {
        return {
            error: true,
            expectedLoss: 0,
            riskLoad: 0,
            expenseLoad: 0,
            technicalPremium: 0,
            ourShareLine: 0,
            rateOnLine: 0,
            lossCostPct: 0,
            profitMarginPct: 0,
        };
    }
    const hit =
        clamp((model.pml250 - layer.attachment) / model.pml250, 0, 1) *
        clamp(layer.limit / denominator, 0, 1);
    const elRaw = model.aal * hit;
    const riskLoad = round(0.15 * Math.sqrt(elRaw * layer.limit), 2);
    const expenseLoad = round(0.12 * (elRaw + riskLoad), 2);
    const base = (elRaw + riskLoad + expenseLoad) / (1 - 0.08);
    const technicalPremium = round(
        base * (1 + 0.05 * layer.reinstatements * layer.reinstatementPremiumPct),
        2,
    );
    const ourShareLine = round(technicalPremium * layer.sharePct, 2);
    const rateOnLine = layer.limit ? round(technicalPremium / layer.limit, 6) : 0;
    const expectedLoss = round(elRaw, 2);
    const lossCostPct = layer.limit ? round(elRaw / layer.limit, 6) : 0;
    const profitMarginPct = 0.08;
    return {
        error: false,
        expectedLoss,
        riskLoad,
        expenseLoad,
        technicalPremium,
        ourShareLine,
        rateOnLine,
        lossCostPct,
        profitMarginPct,
    };
}
