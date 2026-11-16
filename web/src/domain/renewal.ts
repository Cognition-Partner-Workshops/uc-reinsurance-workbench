import { SubmissionStatus } from "../api/types";

const msPerDay = 86_400_000;
const renewalWindowDays = 60;
const urgentWindowDays = 14;

export type RenewalTone = "amber" | "red" | "lapsed";

export interface RenewalBadgeModel {
    label: string;
    tone: RenewalTone;
}

/**
 * Whole calendar days from today (in the viewer's timezone) to the expiry date.
 * Both sides are reduced to a year/month/day and compared at UTC midnight, so the
 * result does not depend on the UTC offset or on the time of day.
 */
export function daysUntilExpiry(expiryDate: string, today: Date = new Date()): number | null {
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(expiryDate ?? "");
    if (!match) return null;
    const [year, month, day] = match.slice(1).map(Number);
    const expiryDay = Date.UTC(year, month - 1, day);
    const expiry = new Date(expiryDay);
    if (expiry.getUTCMonth() !== month - 1 || expiry.getUTCDate() !== day) return null;
    const currentDay = Date.UTC(today.getFullYear(), today.getMonth(), today.getDate());
    return Math.round((expiryDay - currentDay) / msPerDay);
}

export function renewalBadge(
    expiryDate: string,
    status: SubmissionStatus,
    today: Date = new Date(),
): RenewalBadgeModel | null {
    if (status === SubmissionStatus.Bound || status === SubmissionStatus.Declined) return null;
    const days = daysUntilExpiry(expiryDate, today);
    if (days === null || days > renewalWindowDays) return null;
    if (days < 0) return { label: "Lapsed", tone: "lapsed" };
    return {
        label:
            days === 0
                ? "Renewal due today"
                : `Renewal due in ${days} ${days === 1 ? "day" : "days"}`,
        tone: days <= urgentWindowDays ? "red" : "amber",
    };
}
