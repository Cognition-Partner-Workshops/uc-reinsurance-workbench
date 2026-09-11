export function fmtM(value: number): string {
  if (value >= 1e9) return `$${(value / 1e9).toFixed(2)}B`;
  return `$${(value / 1e6).toFixed(value >= 100e6 ? 0 : 1)}M`;
}

export function fmtFull(value: number): string {
  return `$${Math.round(value).toLocaleString("en-US")}`;
}

export function fmtPct(value: number): string {
  return `${(value * 100).toFixed(2)}%`;
}

export function fmtDate(value: string, year = false): string {
  return new Intl.DateTimeFormat("en-GB", {
    day: "numeric",
    month: "short",
    ...(year ? { year: "numeric" } : {}),
  }).format(new Date(value));
}
