import { afterEach, describe, expect, it, vi } from "vitest";
import { SubmissionStatus } from "../api/types";
import { daysUntilExpiry, renewalBadge } from "./renewal";

const expiry = "2026-12-31T00:00:00";

function localDay(year: number, month: number, day: number, hour = 12, minute = 0): Date {
    return new Date(year, month - 1, day, hour, minute);
}

function daysBefore(days: number): Date {
    return localDay(2026, 12, 31 - days);
}

describe("daysUntilExpiry", () => {
    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it.each([0, 1, 14, 15, 45, 60, 61])("counts %i calendar days", (days) => {
        expect(daysUntilExpiry(expiry, daysBefore(days))).toBe(days);
    });

    it("is negative after expiry", () => {
        expect(daysUntilExpiry(expiry, localDay(2027, 1, 1))).toBe(-1);
    });

    it("ignores the time of day", () => {
        expect(daysUntilExpiry(expiry, localDay(2026, 11, 16, 0, 0))).toBe(45);
        expect(daysUntilExpiry(expiry, localDay(2026, 11, 16, 23, 59))).toBe(45);
    });

    it("accepts date-only and offset-suffixed values as the written calendar date", () => {
        expect(daysUntilExpiry("2026-12-31", localDay(2026, 11, 16))).toBe(45);
        expect(daysUntilExpiry("2026-12-31T00:00:00Z", localDay(2026, 11, 16))).toBe(45);
    });

    it("returns null for unparseable dates", () => {
        expect(daysUntilExpiry("", localDay(2026, 11, 16))).toBeNull();
        expect(daysUntilExpiry("not a date", localDay(2026, 11, 16))).toBeNull();
        expect(daysUntilExpiry("2026-02-31T00:00:00", localDay(2026, 11, 16))).toBeNull();
    });

    it.each([
        ["Asia/Kolkata", -330],
        ["America/Los_Angeles", 480],
        ["Pacific/Kiritimati", -840],
    ])("gives the same answer at both ends of the day in %s", (timeZone, offsetMinutes) => {
        vi.stubEnv("TZ", timeZone);
        expect(new Date(2026, 10, 16).getTimezoneOffset()).toBe(offsetMinutes);
        expect(daysUntilExpiry(expiry, new Date(2026, 10, 16, 0, 5))).toBe(45);
        expect(daysUntilExpiry(expiry, new Date(2026, 10, 16, 23, 55))).toBe(45);
        expect(daysUntilExpiry(expiry, new Date(2026, 11, 31, 0, 5))).toBe(0);
        expect(daysUntilExpiry(expiry, new Date(2026, 11, 31, 23, 55))).toBe(0);
        expect(daysUntilExpiry(expiry, new Date(2027, 0, 1, 0, 5))).toBe(-1);
    });

    it("uses the viewer's local day, not the UTC day", () => {
        vi.stubEnv("TZ", "Asia/Kolkata");
        // 03:00 IST on 17 Dec is still 16 Dec in UTC.
        expect(daysUntilExpiry(expiry, new Date("2026-12-16T21:30:00Z"))).toBe(14);
    });

    it("counts days across a daylight-saving change", () => {
        vi.stubEnv("TZ", "America/New_York");
        expect(daysUntilExpiry("2026-03-15T00:00:00", new Date(2026, 2, 1, 12))).toBe(14);
        expect(daysUntilExpiry("2026-11-08T00:00:00", new Date(2026, 9, 25, 12))).toBe(14);
    });
});

describe("renewalBadge", () => {
    const open = SubmissionStatus.Received;

    it("is red on the expiry day", () => {
        expect(renewalBadge(expiry, open, daysBefore(0))).toEqual({
            label: "Renewal due today",
            tone: "red",
        });
    });

    it("uses the singular for one day", () => {
        expect(renewalBadge(expiry, open, daysBefore(1))?.label).toBe("Renewal due in 1 day");
    });

    it("is red at 14 days", () => {
        expect(renewalBadge(expiry, open, daysBefore(14))).toEqual({
            label: "Renewal due in 14 days",
            tone: "red",
        });
    });

    it("is amber at 15 days", () => {
        expect(renewalBadge(expiry, open, daysBefore(15))).toEqual({
            label: "Renewal due in 15 days",
            tone: "amber",
        });
    });

    it("is amber at 60 days", () => {
        expect(renewalBadge(expiry, open, daysBefore(60))).toEqual({
            label: "Renewal due in 60 days",
            tone: "amber",
        });
    });

    it("shows nothing at 61 days", () => {
        expect(renewalBadge(expiry, open, daysBefore(61))).toBeNull();
    });

    it("shows Lapsed after expiry", () => {
        expect(renewalBadge(expiry, open, localDay(2027, 1, 1))).toEqual({
            label: "Lapsed",
            tone: "lapsed",
        });
        expect(renewalBadge(expiry, open, localDay(2028, 6, 1))?.label).toBe("Lapsed");
    });

    it.each([SubmissionStatus.Bound, SubmissionStatus.Declined])(
        "shows nothing for status %i, due or lapsed",
        (status) => {
            expect(renewalBadge(expiry, status, daysBefore(10))).toBeNull();
            expect(renewalBadge(expiry, status, daysBefore(45))).toBeNull();
            expect(renewalBadge(expiry, status, localDay(2027, 1, 1))).toBeNull();
        },
    );

    it.each([
        SubmissionStatus.Received,
        SubmissionStatus.InReview,
        SubmissionStatus.Quoted,
        SubmissionStatus.Withdrawn,
    ])("shows a badge for status %i", (status) => {
        expect(renewalBadge(expiry, status, daysBefore(45))?.label).toBe("Renewal due in 45 days");
    });

    it("shows nothing when the expiry date is missing", () => {
        expect(renewalBadge("", open, daysBefore(10))).toBeNull();
    });
});
