import { useEffect, useState } from "react";
import { usePortfolio } from "../api/queries";
import { TreatyStatus, TreatyType, treatyTypeLabels } from "../api/types";
import { fmtDate, fmtM } from "../domain/format";
import { ErrorBanner, EmptyState, PageHeader, Skeleton, StatusPill } from "../components/Ui";
import { useNavigate } from "react-router-dom";

export default function PortfolioScreen({
    lastTreatyId,
    onTreaty,
}: {
    lastTreatyId?: number;
    onTreaty: (id: number) => void;
}) {
    const [line, setLine] = useState<"property" | "all">("property");
    const portfolio = usePortfolio();
    const navigate = useNavigate();
    useEffect(() => {
        if (lastTreatyId || portfolio.isLoading || portfolio.error) {
            return;
        }
        const firstTreaty = portfolio.detailItems
            .flatMap((submission) => submission.treaties)
            .find(Boolean);
        if (firstTreaty) {
            onTreaty(firstTreaty.id);
        }
    }, [lastTreatyId, onTreaty, portfolio.detailItems, portfolio.error, portfolio.isLoading]);
    if (portfolio.isLoading) {
        return (
            <>
                <PageHeader eyebrow="Portfolio" title="2026 treaty portfolio" />
                <div className="skeleton-grid">
                    {[1, 2, 3, 4, 5].map((n) => (
                        <Skeleton key={n} className="skeleton-card" />
                    ))}
                </div>
            </>
        );
    }
    if (portfolio.error) return <ErrorBanner error={portfolio.error} />;
    const details = portfolio.detailItems;
    const treaties = details
        .flatMap((submission) => submission.treaties.map((treaty) => ({ submission, treaty })))
        .filter(({ treaty }) => line === "all" || treaty.type === TreatyType.PropertyCatXoL);
    const pricingByTreaty = new Map<number, (typeof portfolio.treatyPricing)[number]["data"]>();
    portfolio.treatyIds.forEach((id, index) =>
        pricingByTreaty.set(id, portfolio.treatyPricing[index]?.data),
    );
    const models = new Map<number, (typeof portfolio.catModels)[number]["data"]>();
    details.forEach((submission, index) =>
        models.set(submission.id, portfolio.catModels[index]?.data),
    );
    const exposures = portfolio.exposures.map((query) => query.data?.summary).filter(Boolean);
    const bound = treaties.filter(({ treaty }) => treaty.status === TreatyStatus.Bound);
    const pricedNonBound = treaties.filter(({ treaty }) => treaty.status !== TreatyStatus.Bound);
    const sumPremium = (items: typeof treaties) =>
        items.reduce((total, item) => {
            const rows = pricingByTreaty.get(item.treaty.id) ?? [];
            return total + rows.reduce((sum, row) => sum + row.technicalPremium, 0);
        }, 0);
    const totalLimit = treaties.reduce(
        (sum, item) => sum + item.treaty.layers.reduce((v, layer) => v + layer.limit, 0),
        0,
    );
    const pml = details.reduce((sum, submission) => {
        const result =
            (models.get(submission.id) ?? []).find(
                (row) => row.regionId == null && row.perilId == null,
            ) ?? models.get(submission.id)?.[0];
        return sum + (result?.pml250 ?? 0);
    }, 0);
    const maxExpiry = bound.reduce<string | undefined>(
        (max, item) => (!max || item.treaty.expiryDate > max ? item.treaty.expiryDate : max),
        undefined,
    );
    const daysUntil = maxExpiry
        ? Math.ceil((new Date(maxExpiry).getTime() - Date.now()) / 86400000)
        : 0;
    const perils = new Map<string, number>();
    exposures.forEach((summary) =>
        summary?.byPeril.forEach((item) => {
            const key = item.perilName ?? item.perilCode ?? "Other";
            perils.set(key, (perils.get(key) ?? 0) + item.tiv);
        }),
    );
    const maxPeril = Math.max(...Array.from(perils.values()), 1);
    const kpis = [
        ["Bound premium", fmtM(sumPremium(bound)), `${bound.length} bound treaties`],
        [
            "Quoted pipeline",
            fmtM(sumPremium(pricedNonBound)),
            `${pricedNonBound.length} quoted treaties`,
        ],
        [
            "Aggregate limit",
            fmtM(totalLimit),
            `Across ${treaties.reduce((sum, item) => sum + item.treaty.layers.length, 0)} layers`,
        ],
        ["Portfolio PML 250", fmtM(pml), "Sum of ceded portfolio PMLs"],
        [
            "Renewals",
            String(bound.length),
            maxExpiry
                ? `Expire ${fmtDate(maxExpiry, true)} · ${daysUntil} days`
                : "No bound renewals",
        ],
    ];
    return (
        <>
            <PageHeader eyebrow="Portfolio" title="2026 treaty portfolio">
                <div className="segmented">
                    <button
                        className={line === "property" ? "selected" : ""}
                        onClick={() => setLine("property")}
                    >
                        Property Cat
                    </button>
                    <button
                        className={line === "all" ? "selected" : ""}
                        onClick={() => setLine("all")}
                    >
                        All lines
                    </button>
                </div>
            </PageHeader>
            <div className="kpi-grid">
                {kpis.map(([label, value, sub]) => (
                    <div className="card kpi" key={label}>
                        <div className="muted">{label}</div>
                        <strong>{value}</strong>
                        <div className="muted">{sub}</div>
                    </div>
                ))}
            </div>
            <div className="dashboard-grid">
                <section className="card table-card">
                    <div className="section-heading">
                        <strong>Treaties</strong>
                        <span className="muted">{treaties.length} treaties · 2026 year</span>
                    </div>
                    <div className="table-head treaty-grid">
                        <span>Treaty</span>
                        <span>Limit</span>
                        <span>Premium</span>
                        <span>PML 250</span>
                        <span>Status</span>
                    </div>
                    {treaties.length === 0 && <EmptyState>No treaties match this view.</EmptyState>}
                    {treaties.map(({ submission, treaty }) => {
                        const rows = pricingByTreaty.get(treaty.id) ?? [];
                        const hasError = rows.length > 0 && rows.length < treaty.layers.length;
                        const premium =
                            rows.length === treaty.layers.length
                                ? rows.reduce((sum, row) => sum + row.technicalPremium, 0)
                                : undefined;
                        const model =
                            (models.get(submission.id) ?? []).find(
                                (row) => row.regionId == null && row.perilId == null,
                            ) ?? models.get(submission.id)?.[0];
                        return (
                            <button
                                className="table-row treaty-grid"
                                key={treaty.id}
                                onClick={() => {
                                    onTreaty(treaty.id);
                                    navigate(`/treaties/${treaty.id}`);
                                }}
                            >
                                <span>
                                    <strong>{treaty.name}</strong>
                                    <small>
                                        {submission.cedentName} · {treatyTypeLabels[treaty.type]}
                                    </small>
                                </span>
                                <span>
                                    {fmtM(
                                        treaty.layers.reduce((sum, layer) => sum + layer.limit, 0),
                                    )}
                                </span>
                                <span>
                                    {hasError
                                        ? "Error"
                                        : premium === undefined
                                          ? "—"
                                          : fmtM(premium)}
                                </span>
                                <span>{model ? fmtM(model.pml250) : "—"}</span>
                                <StatusPill status={treaty.status} treaty />
                            </button>
                        );
                    })}
                </section>
                <div className="stack">
                    <section className="card">
                        <div className="section-heading">
                            <strong>Upcoming renewals</strong>
                        </div>
                        {bound
                            .sort((a, b) => a.treaty.expiryDate.localeCompare(b.treaty.expiryDate))
                            .map(({ treaty }) => (
                                <button
                                    className="simple-row"
                                    key={treaty.id}
                                    onClick={() => {
                                        onTreaty(treaty.id);
                                        navigate(`/treaties/${treaty.id}`);
                                    }}
                                >
                                    <span>
                                        <strong>{treaty.name}</strong>
                                        <small>Expires {fmtDate(treaty.expiryDate, true)}</small>
                                    </span>
                                    <span className="renewal-meta">
                                        <strong>
                                            {Math.ceil(
                                                (new Date(treaty.expiryDate).getTime() -
                                                    Date.now()) /
                                                    86400000,
                                            )}
                                            d
                                        </strong>
                                        <small>
                                            {(() => {
                                                const rows = pricingByTreaty.get(treaty.id) ?? [];
                                                return rows.length === treaty.layers.length
                                                    ? fmtM(
                                                          rows.reduce(
                                                              (sum, row) =>
                                                                  sum + row.technicalPremium,
                                                              0,
                                                          ),
                                                      )
                                                    : "—";
                                            })()}
                                        </small>
                                    </span>
                                </button>
                            ))}
                    </section>
                    <section className="card">
                        <div className="section-heading">
                            <strong>Exposure by peril</strong>
                        </div>
                        {Array.from(perils.entries()).map(([name, value]) => (
                            <div className="bar-row" key={name}>
                                <div>
                                    <span>{name}</span>
                                    <span className="muted">{fmtM(value)}</span>
                                </div>
                                <div className="bar">
                                    <i style={{ width: `${(value / maxPeril) * 100}%` }} />
                                </div>
                            </div>
                        ))}
                        {!perils.size && <EmptyState>No exposure data.</EmptyState>}
                    </section>
                </div>
            </div>
        </>
    );
}
