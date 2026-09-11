import type { ReactNode } from "react";
import { submissionStatusLabels, treatyStatusLabels } from "../api/types";

export function PageHeader({ eyebrow, title, children }: { eyebrow: string; title: string; children?: ReactNode }) {
  return (
    <div className="page-header">
      <div>
        <div className="eyebrow"><span className="eyebrow-mark" />{eyebrow}</div>
        <h1>{title}</h1>
      </div>
      {children}
    </div>
  );
}

export function StatusPill({ status, treaty = false }: { status: number; treaty?: boolean }) {
  const label = treaty ? treatyStatusLabels[status] : submissionStatusLabels[status];
  const key = label?.toLowerCase().replace(" ", "-") ?? "draft";
  return <span className={`status-pill status-${key}`}>{label ?? "Unknown"}</span>;
}

export function Skeleton({ className = "" }: { className?: string }) {
  return <div className={`skeleton ${className}`} />;
}

export function ErrorBanner({ error }: { error: unknown }) {
  const message = error instanceof Error ? error.message : "Something went wrong.";
  const traceId =
    error && typeof error === "object" && "traceId" in error
      ? String((error as { traceId?: string }).traceId ?? "")
      : "";
  return (
    <div className="error-banner">
      <span>{message}</span>
      {traceId && <small>Trace ID: {traceId}</small>}
    </div>
  );
}

export function EmptyState({ children }: { children: ReactNode }) {
  return <div className="empty-state">{children}</div>;
}
