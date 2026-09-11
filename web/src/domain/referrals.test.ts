import { describe, expect, it } from "vitest";
import { ReferralRuleModel } from "../api/types";
import { evaluateRules } from "./referrals";

const rule = (kind: number, threshold?: number, textValue?: string): ReferralRuleModel => ({
    id: kind,
    code: `RULE_${kind}`,
    description: "Test rule",
    active: true,
    kind,
    threshold,
    textValue,
});

const context = (overrides = {}) => ({
    limit: 50,
    rateOnLine: 0.1,
    cedentRating: "A",
    underwriterAuthorityLimit: 100,
    ...overrides,
});

describe("evaluateRules", () => {
    const cases: Array<{
        kind: number;
        threshold?: number;
        textValue?: string;
        values: Record<string, string | number>;
        hit: boolean;
    }> = [
        { kind: 0, threshold: 40, values: { limit: 50 }, hit: true },
        { kind: 1, threshold: 0.2, values: { rateOnLine: 0.1 }, hit: true },
        { kind: 2, threshold: 0.05, values: { rateOnLine: 0.1 }, hit: true },
        { kind: 3, textValue: "A-", values: { cedentRating: "B+" }, hit: true },
        { kind: 4, values: { limit: 101 }, hit: true },
    ];
    it.each(cases)("handles rule kind $kind", ({ kind, threshold, textValue, values, hit }) => {
        const result = evaluateRules([rule(kind, threshold, textValue)], context(values))[0];
        expect(result.hit).toBe(hit);
    });

    it("handles rating ordering and unknown ratings as C", () => {
        expect(
            evaluateRules([rule(3, undefined, "A-")], context({ cedentRating: "A-" }))[0].hit,
        ).toBe(false);
        expect(
            evaluateRules([rule(3, undefined, "A-")], context({ cedentRating: "UNKNOWN" }))[0].hit,
        ).toBe(true);
    });

    it("ignores inactive rules", () => {
        const inactive = { ...rule(0, 1), active: false };
        expect(evaluateRules([inactive], context({ limit: 100 }))).toEqual([]);
    });

    it("keeps portfolio checks neutral until the server evaluates them", () => {
        const pending = evaluateRules([rule(5)], context())[0];
        const breached = evaluateRules(
            [rule(5)],
            context({ serverReferralReasons: "PORTFOLIO: breach" }),
        )[0];
        expect(pending.neutral).toBe(true);
        expect(pending.hit).toBe(false);
        expect(breached.hit).toBe(true);
    });
});
