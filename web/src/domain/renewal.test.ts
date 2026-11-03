import { afterAll, beforeAll, describe, expect, it, vi } from "vitest";
import { SubmissionStatus } from "../api/types";
import { calendarDaysUntil, renewalBadge } from "./renewal";

const expiry = "2026-12-31T00:00:00";

function daysBefore(days: number, hour = 12): Date {
    return new Date(2026, 11, 31 - days, hour, 0, 0);
}

describe("calendarDaysUntil", () => {
    it("counts calendar days regardless of time of day", () => {
        expect(calendarDaysUntil(expiry, new Date(2026, 11, 1, 0, 0, 0))).toBe(30);
        expect(calendarDaysUntil(expiry, new Date(2026, 11, 1, 23, 59, 59))).toBe(30);
        expect(calendarDaysUntil(expiry, new Date(2026, 11, 31, 23, 59, 59))).toBe(0);
    });

    it("ignores any time or offset suffix on the expiry date", () => {
        const today = new Date(2026, 11, 1, 9, 0, 0);
        expect(calendarDaysUntil("2026-12-31T23:59:59Z", today)).toBe(30);
        expect(calendarDaysUntil("2026-12-31", today)).toBe(30);
    });

    it("returns NaN for an unparseable date", () => {
        expect(calendarDaysUntil("not a date", new Date())).toBeNaN();
    });

    describe("in IST (UTC+5:30)", () => {
        beforeAll(() => {
            vi.stubEnv("TZ", "Asia/Kolkata");
        });
        afterAll(() => {
            vi.unstubAllEnvs();
        });

        it("does not go off by one just after local midnight", () => {
            // 00:30 IST on 1 Dec is still 30 Nov in UTC.
            const today = new Date(2026, 11, 1, 0, 30, 0);
            expect(today.getTimezoneOffset()).toBe(-330);
            expect(today.getUTCDate()).toBe(30);
            expect(calendarDaysUntil(expiry, today)).toBe(30);
        });

        it("does not go off by one late in the local day", () => {
            expect(calendarDaysUntil(expiry, new Date(2026, 11, 1, 23, 45, 0))).toBe(30);
        });
    });
});

describe("renewalBadge", () => {
    const open = SubmissionStatus.InReview;

    it.each([
        [0, "Renewal due in 0 days", "red"],
        [1, "Renewal due in 1 day", "red"],
        [14, "Renewal due in 14 days", "red"],
        [15, "Renewal due in 15 days", "amber"],
        [60, "Renewal due in 60 days", "amber"],
    ])("shows a badge %i days before expiry", (days, label, tone) => {
        expect(renewalBadge(open, expiry, daysBefore(days))).toEqual({
            label,
            tone,
            daysUntilExpiry: days,
        });
    });

    it("shows no badge 61 days before expiry", () => {
        expect(renewalBadge(open, expiry, daysBefore(61))).toBeNull();
    });

    it("shows Lapsed once the expiry date has passed", () => {
        expect(renewalBadge(open, expiry, new Date(2027, 0, 1, 0, 0, 1))).toEqual({
            label: "Lapsed",
            tone: "lapsed",
            daysUntilExpiry: -1,
        });
        expect(renewalBadge(open, expiry, new Date(2027, 5, 1))?.label).toBe("Lapsed");
    });

    it.each([SubmissionStatus.Bound, SubmissionStatus.Declined])(
        "shows no badge for status %i, even when due or lapsed",
        (status) => {
            expect(renewalBadge(status, expiry, daysBefore(5))).toBeNull();
            expect(renewalBadge(status, expiry, new Date(2027, 1, 1))).toBeNull();
        },
    );

    it.each([
        SubmissionStatus.Received,
        SubmissionStatus.InReview,
        SubmissionStatus.Quoted,
        SubmissionStatus.Withdrawn,
    ])("shows a badge for status %i", (status) => {
        expect(renewalBadge(status, expiry, daysBefore(30))?.label).toBe("Renewal due in 30 days");
    });

    it("shows no badge when the expiry date is unparseable", () => {
        expect(renewalBadge(open, "", daysBefore(5))).toBeNull();
    });
});
