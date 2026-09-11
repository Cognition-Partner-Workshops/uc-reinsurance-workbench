import { describe, expect, it } from "vitest";
import { fmtDate, fmtFull, fmtM, fmtPct } from "./format";

describe("format helpers", () => {
    it("formats compact money", () => {
        expect(fmtM(25_000_000)).toBe("$25.0M");
        expect(fmtM(1_500_000_000)).toBe("$1.50B");
    });

    it("formats full money and percentages", () => {
        expect(fmtFull(4_260_869.57)).toBe("$4,260,870");
        expect(fmtPct(0.085217)).toBe("8.52%");
    });

    it("formats dates in en-GB", () => {
        expect(fmtDate("2026-01-14T00:00:00", true)).toBe("14 Jan 2026");
        expect(fmtDate("2026-01-14T00:00:00")).toBe("14 Jan");
    });
});
