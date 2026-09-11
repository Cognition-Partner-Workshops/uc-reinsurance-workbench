import { describe, expect, it } from "vitest";
import type { CatModelResultModel, TreatyLayerModel } from "../api/types";
import { priceLayer } from "./pricing";

const model: CatModelResultModel = {
    id: 1,
    regionId: null,
    perilId: null,
    modelVendor: "RMS",
    modelVersion: "v23",
    aal: 12_000_000,
    expectedLoss: 0,
    pmL50: 100_000_000,
    pmL100: 180_000_000,
    pmL250: 300_000_000,
    runOn: "2026-01-01",
};

const layer = (overrides: Partial<TreatyLayerModel> = {}): TreatyLayerModel => ({
    id: 1,
    layerNumber: 1,
    limit: 50_000_000,
    attachment: 50_000_000,
    reinstatements: 0,
    reinstatementPremiumPct: 0,
    sharePct: 1,
    currency: "USD",
    ...overrides,
});

describe("priceLayer", () => {
    it("matches the server golden values", () => {
        expect(priceLayer(layer(), model)).toMatchObject({
            expectedLoss: 2_000_000,
            riskLoad: 1_500_000,
            expenseLoad: 420_000,
            technicalPremium: 4_260_869.57,
            ourShareLine: 4_260_869.57,
            rateOnLine: 0.085217,
            lossCostPct: 0.04,
        });
    });

    it("uses the server wire names for Atlas Layer 1 pricing", () => {
        const atlasModel: CatModelResultModel = {
            id: 13,
            regionId: null,
            perilId: null,
            modelVendor: "Internal",
            modelVersion: "v2026.1",
            aal: 9_000_000,
            expectedLoss: 7_500_000,
            pmL50: 105_000_000,
            pmL100: 195_000_000,
            pmL250: 300_000_000,
            runOn: "2026-02-17",
        };
        const atlasLayer = layer({
            limit: 25_000_000,
            attachment: 25_000_000,
            reinstatements: 1,
            reinstatementPremiumPct: 0.5,
        });

        expect(priceLayer(atlasLayer, atlasModel)).toMatchObject({
            expectedLoss: 750_000,
            riskLoad: 649_519.05,
            expenseLoad: 167_942.29,
            technicalPremium: 1_746_356.38,
            ourShareLine: 1_746_356.38,
            rateOnLine: 0.069854,
            lossCostPct: 0.03,
        });
    });

    it("reduces loss and rate on line at higher attachment", () => {
        const lower = priceLayer(layer({ attachment: 200_000_000 }), model);
        const higher = priceLayer(layer({ attachment: 260_000_000 }), model);
        expect(higher.expectedLoss).toBeLessThan(lower.expectedLoss);
        expect(higher.rateOnLine).toBeLessThan(lower.rateOnLine);
    });

    it("returns the zero-PML floor", () => {
        const result = priceLayer(layer(), { ...model, pmL250: 0 });
        expect(result.expectedLoss).toBe(0);
        expect(result.technicalPremium).toBe(result.riskLoad + result.expenseLoad);
    });

    it("returns a preview error when attachment equals PML250", () => {
        expect(priceLayer(layer({ attachment: model.pmL250 }), model).error).toBe(true);
    });

    it("increases premium for reinstatements", () => {
        const without = priceLayer(layer({ reinstatementPremiumPct: 0.5 }), model);
        const withReinstatements = priceLayer(
            layer({ reinstatements: 2, reinstatementPremiumPct: 0.5 }),
            model,
        );
        expect(withReinstatements.technicalPremium).toBeGreaterThan(without.technicalPremium);
    });

    it("scales our share only", () => {
        const full = priceLayer(layer(), model);
        const half = priceLayer(layer({ sharePct: 0.5 }), model);
        expect(half.ourShareLine).toBe(2_130_434.79);
        expect(half.technicalPremium).toBe(full.technicalPremium);
        expect(half.expectedLoss).toBe(full.expectedLoss);
    });
});
