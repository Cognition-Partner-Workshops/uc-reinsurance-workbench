import { SubmissionStatus } from "../api/types";

export const renewalWindowDays = 60;
export const renewalUrgentDays = 14;

const msPerDay = 86_400_000;

export type RenewalTone = "amber" | "red" | "lapsed";

export interface RenewalBadgeInfo {
    label: string;
    tone: RenewalTone;
    daysUntilExpiry: number;
}

/**
 * Whole calendar days from `today` (local date) to the date part of `isoDate`.
 * Both sides are reduced to a calendar date before subtracting, so the time of day
 * and the viewer's UTC offset cannot shift the result by one.
 */
export function calendarDaysUntil(isoDate: string, today: Date): number {
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(isoDate);
    if (!match) return Number.NaN;
    const [, year, month, day] = match;
    const target = Date.UTC(Number(year), Number(month) - 1, Number(day));
    const start = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
    return Math.round((target - start) / msPerDay);
}

export function renewalBadge(
    status: SubmissionStatus,
    expiryDate: string,
    today: Date = new Date(),
): RenewalBadgeInfo | null {
    if (status === SubmissionStatus.Bound || status === SubmissionStatus.Declined) return null;
    const daysUntilExpiry = calendarDaysUntil(expiryDate, today);
    if (Number.isNaN(daysUntilExpiry) || daysUntilExpiry > renewalWindowDays) return null;
    if (daysUntilExpiry < 0) return { label: "Lapsed", tone: "lapsed", daysUntilExpiry };
    return {
        label: `Renewal due in ${daysUntilExpiry} ${daysUntilExpiry === 1 ? "day" : "days"}`,
        tone: daysUntilExpiry <= renewalUrgentDays ? "red" : "amber",
        daysUntilExpiry,
    };
}
