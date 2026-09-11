import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
    useBindTreaty,
    useCatModel,
    usePriceTreaty,
    useReferralRules,
    useSubmission,
    useTreaty,
    useTreatyPricing,
} from "../api/queries";
import type { PricingResultModel } from "../api/types";
import { fmtDate, fmtFull, fmtM, fmtPct } from "../domain/format";
import { priceLayer } from "../domain/pricing";
import { evaluateRules } from "../domain/referrals";
import { ErrorBanner, PageHeader, Skeleton } from "../components/Ui";

type Editable = {
    limit: number;
    attachment: number;
    reinstatements: number;
    reinstatementPremiumPct: number;
    sharePct: number;
};

const formatRunDate = (result?: PricingResultModel) =>
    result?.calculatedOn
        ? fmtDate(result.calculatedOn, true)
        : fmtDate(new Date().toISOString(), true);

export default function PricingScreen() {
    const id = Number(useParams().id);
    const treaty = useTreaty(id);
    const submission = useSubmission(treaty.data?.submissionId);
    const catModel = useCatModel(treaty.data?.submissionId);
    const rules = useReferralRules();
    const persisted = useTreatyPricing(id);
    const price = usePriceTreaty();
    const bind = useBindTreaty();
    const [notice, setNotice] = useState("");
    const [selected, setSelected] = useState(0);
    const [edited, setEdited] = useState<Partial<Editable>>({});

    const layer = treaty.data?.layers[selected];
    const model = useMemo(
        () =>
            (catModel.data ?? []).find((row) => row.regionId == null && row.perilId == null) ??
            catModel.data?.[0],
        [catModel.data],
    );

    useEffect(() => {
        setSelected(0);
        setEdited({});
    }, [id]);

    if (treaty.isLoading || submission.isLoading || catModel.isLoading) {
        return <Skeleton className="skeleton-card" />;
    }

    if (treaty.error || submission.error || !treaty.data || !layer || !model) {
        return (
            <ErrorBanner
                error={
                    treaty.error ?? submission.error ?? new Error("Pricing data is unavailable.")
                }
            />
        );
    }

    const treatyData = treaty.data;
    const current = { ...layer, ...edited };
    const preview = priceLayer(current, model);
    const persistedResult = persisted.data?.find((row) => row.treatyLayerId === layer.id);
    const hasEdits = Object.keys(edited).length > 0;
    const displayed = hasEdits
        ? preview
        : persistedResult
          ? {
                error: false,
                expectedLoss: persistedResult.expectedLoss,
                riskLoad: persistedResult.riskLoad,
                expenseLoad: persistedResult.expenseLoad,
                technicalPremium: persistedResult.technicalPremium,
                ourShareLine: persistedResult.ourShareLine,
                rateOnLine: persistedResult.rateOnLine,
                lossCostPct: persistedResult.lossCostPct,
                profitMarginPct: persistedResult.profitMarginPct,
            }
          : preview;
    const evaluation = evaluateRules(rules.data ?? [], {
        limit: current.limit,
        rateOnLine: displayed.rateOnLine,
        cedentRating: submission.data?.cedentRating ?? "C",
        underwriterAuthorityLimit: submission.data?.underwriterAuthorityLimit ?? 0,
        serverReferralReasons: hasEdits ? undefined : persistedResult?.referralReasons,
    });
    const referralRequired = evaluation.some((row) => row.hit);
    const buildUpTotal =
        displayed.expectedLoss +
        displayed.riskLoad +
        displayed.expenseLoad +
        Math.max(
            displayed.technicalPremium -
                displayed.expectedLoss -
                displayed.riskLoad -
                displayed.expenseLoad,
            0,
        );

    const setField = (key: keyof Editable, value: number) => {
        setEdited((previous) => ({ ...previous, [key]: value }));
        setNotice("");
    };

    const primaryAction = () => {
        if (referralRequired) {
            setNotice(`Referred to ${submission.data?.underwriterName ?? "the underwriter"}.`);
            return;
        }
        bind.mutate(id);
    };

    return (
        <>
            <div className="breadcrumb">
                <Link to="/">Portfolio</Link>
                <span>/</span>
                <Link to={`/treaties/${id}`}>{submission.data?.reference}</Link>
                <span>/</span>
                Pricing
            </div>
            <PageHeader eyebrow="Quote" title={treatyData.name}>
                <div className="header-controls">
                    <span className="preview-label">
                        Preview — server pricing uses saved layer terms
                    </span>
                    <div className="layer-tabs">
                        {treatyData.layers.map((item, index) => (
                            <button
                                className={index === selected ? "selected" : ""}
                                key={item.id}
                                onClick={() => {
                                    setSelected(index);
                                    setEdited({});
                                    setNotice("");
                                }}
                            >
                                Layer {item.layerNumber}
                            </button>
                        ))}
                    </div>
                </div>
            </PageHeader>
            <div className="pricing-grid">
                <section className="card">
                    <div className="section-heading">
                        <strong>Treaty parameters</strong>
                        <button className="text-button" onClick={() => setEdited({})}>
                            Reset to layer
                        </button>
                    </div>
                    <div className="form-grid">
                        <ParameterField
                            label="Limit"
                            value={current.limit}
                            step={1000000}
                            prefix="$"
                            onChange={(value) => setField("limit", value)}
                        />
                        <ParameterField
                            label="Attachment point"
                            value={current.attachment}
                            step={1000000}
                            prefix="$"
                            onChange={(value) => setField("attachment", value)}
                        />
                        <ParameterField
                            label="Reinstatements"
                            value={current.reinstatements}
                            step={1}
                            prefix="#"
                            onChange={(value) => setField("reinstatements", value)}
                        />
                        <ParameterField
                            label="Reinstatement premium"
                            value={current.reinstatementPremiumPct}
                            step={0.05}
                            prefix="×"
                            onChange={(value) => setField("reinstatementPremiumPct", value)}
                        />
                        <ParameterField
                            label="Our share"
                            value={current.sharePct}
                            step={0.05}
                            prefix="×"
                            onChange={(value) => setField("sharePct", value)}
                        />
                    </div>
                    <div className="model-summary">
                        <span>
                            Portfolio AAL <strong>{fmtM(model.aal)}</strong>
                        </span>
                        <span>
                            PML 250 <strong>{fmtM(model.pmL250)}</strong>
                        </span>
                        <span>
                            Model{" "}
                            <strong>
                                {model.modelVendor} {model.modelVersion}
                            </strong>
                        </span>
                    </div>
                </section>
                <section className="stack">
                    <div
                        className={preview.error ? "pricing-result error-result" : "pricing-result"}
                    >
                        {preview.error ? (
                            <>
                                <div className="result-label">Pricing failed</div>
                                <strong>
                                    Attachment equals portfolio PML 250, so the layer-hit
                                    denominator is zero.
                                </strong>
                                <p>Routed for review; the treaty cannot be bound.</p>
                                <div className="mono error-endpoint">
                                    POST /api/treaties/{id}/price → 500
                                </div>
                            </>
                        ) : (
                            <>
                                <div className="result-label">Technical premium</div>
                                <strong>{fmtFull(displayed.technicalPremium)}</strong>
                                <div className="result-meta">
                                    Our share {fmtFull(displayed.ourShareLine)} ·{" "}
                                    {hasEdits
                                        ? "Preview"
                                        : persistedResult
                                          ? `Calculated ${formatRunDate(persistedResult)}`
                                          : "Preview"}
                                </div>
                            </>
                        )}
                    </div>
                    {!preview.error && (
                        <>
                            <div className="result-grid">
                                <Metric label="Rate on line" value={fmtPct(displayed.rateOnLine)} />
                                <Metric label="Loss cost" value={fmtPct(displayed.lossCostPct)} />
                                <Metric
                                    label="Profit margin"
                                    value={fmtPct(displayed.profitMarginPct)}
                                />
                            </div>
                            <div className="card">
                                <div className="section-heading">
                                    <strong>Premium build-up</strong>
                                    <span className="mono">{fmtPct(displayed.rateOnLine)} ROL</span>
                                </div>
                                <PremiumRow
                                    label="Expected loss"
                                    value={displayed.expectedLoss}
                                    total={buildUpTotal}
                                />
                                <PremiumRow
                                    label="Risk load"
                                    value={displayed.riskLoad}
                                    total={buildUpTotal}
                                />
                                <PremiumRow
                                    label="Expense load"
                                    value={displayed.expenseLoad}
                                    total={buildUpTotal}
                                />
                                <PremiumRow
                                    label="Profit margin + reinstatements"
                                    value={Math.max(
                                        displayed.technicalPremium -
                                            displayed.expectedLoss -
                                            displayed.riskLoad -
                                            displayed.expenseLoad,
                                        0,
                                    )}
                                    total={buildUpTotal}
                                />
                            </div>
                        </>
                    )}
                    <div className="card">
                        <div className="section-heading">
                            <strong>Referral check</strong>
                            <span
                                className={`referral-label ${
                                    referralRequired ? "referral" : "within"
                                }`}
                            >
                                {referralRequired ? "Referral required" : "Within authority"}
                            </span>
                        </div>
                        {evaluation.map((row) => (
                            <div className="rule-row" key={row.code}>
                                <span
                                    className={`rule-dot ${
                                        row.hit ? "hit" : row.neutral ? "neutral" : ""
                                    }`}
                                />
                                <span>
                                    <strong>{row.code}</strong>
                                    <small>{row.text}</small>
                                </span>
                            </div>
                        ))}
                        <div className="modal-actions referral-actions">
                            <button
                                className="secondary-button"
                                disabled={price.isPending}
                                onClick={() => price.mutate(id)}
                            >
                                Save quote
                            </button>
                            <button
                                className={
                                    referralRequired
                                        ? "primary-button referral-button"
                                        : "primary-button"
                                }
                                disabled={bind.isPending || preview.error}
                                onClick={primaryAction}
                            >
                                {referralRequired ? "Refer for approval" : "Bind treaty"}
                            </button>
                        </div>
                        {price.error && <ErrorBanner error={price.error} />}
                        {bind.error && <ErrorBanner error={bind.error} />}
                        {notice && <div className="success-banner">{notice}</div>}
                    </div>
                </section>
            </div>
        </>
    );
}

function ParameterField({
    label,
    value,
    step,
    prefix,
    onChange,
}: {
    label: string;
    value: number;
    step: number;
    prefix: string;
    onChange: (value: number) => void;
}) {
    return (
        <label>
            {label}
            <span className="unit-input">
                <span>{prefix}</span>
                <input
                    type="number"
                    step={step}
                    value={value}
                    onChange={(event) => onChange(Number(event.target.value))}
                />
            </span>
        </label>
    );
}

function Metric({ label, value }: { label: string; value: string }) {
    return (
        <div className="card metric-card">
            <span className="muted">{label}</span>
            <strong>{value}</strong>
        </div>
    );
}

function PremiumRow({ label, value, total }: { label: string; value: number; total: number }) {
    return (
        <div className="premium-line">
            <span>
                <span>{label}</span>
                <span className="premium-bar">
                    <i style={{ width: `${total ? (value / total) * 100 : 0}%` }} />
                </span>
            </span>
            <strong>{fmtFull(value)}</strong>
        </div>
    );
}
