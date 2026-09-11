import type { ReferralRuleModel } from "../api/types";

export interface ReferralContext {
  limit: number;
  rateOnLine: number;
  cedentRating: string;
  underwriterAuthorityLimit: number;
  serverReferralReasons?: string;
}

export interface ReferralEvaluation {
  code: string;
  text: string;
  hit: boolean;
  neutral?: boolean;
}

const ratingOrder = ["C", "B-", "B", "B+", "B++", "A-", "A", "A+", "A++"];

export function evaluateRules(rules: ReferralRuleModel[], context: ReferralContext): ReferralEvaluation[] {
  return rules.filter((rule) => rule.active).map((rule) => {
    let hit = false;
    let text = rule.description;
    let neutral = false;
    switch (rule.kind) {
      case 0:
        hit = context.limit > (rule.threshold ?? 0);
        text = hit
          ? `Layer exceeds the maximum layer limit of ${fmtRuleMoney(rule.threshold)}.`
          : `Layer within the maximum layer limit.`;
        break;
      case 1:
        hit = context.rateOnLine < (rule.threshold ?? 0);
        text = hit ? "Rate on line is below the minimum." : "Rate on line above minimum.";
        break;
      case 2:
        hit = context.rateOnLine > (rule.threshold ?? 0);
        text = hit ? "Rate on line is above the maximum." : "Rate on line below maximum.";
        break;
      case 3: {
        const actual = ratingOrder.indexOf(context.cedentRating || "C");
        const threshold = ratingOrder.indexOf(rule.textValue || "B++");
        hit = actual < threshold;
        text = hit
          ? `Cedent rating ${context.cedentRating} is below the required ${rule.textValue}.`
          : `Cedent rated ${context.cedentRating} (required ${rule.textValue}).`;
        break;
      }
      case 4:
        hit = context.limit > context.underwriterAuthorityLimit;
        text = hit ? "Layer exceeds the underwriter's authority." : "Within underwriter authority.";
        break;
      case 5:
        neutral = !context.serverReferralReasons;
        hit = Boolean(context.serverReferralReasons?.toUpperCase().includes("PORTFOLIO"));
        text = neutral ? "Evaluated on save." : hit ? "Portfolio aggregate limit breached." : "Within portfolio aggregate limits.";
        break;
      default:
        text = rule.description;
    }
    return { code: rule.code, text, hit, neutral };
  });
}

function fmtRuleMoney(value?: number): string {
  if (!value) return "$0";
  return `$${(value / 1e6).toFixed(0)}M`;
}
