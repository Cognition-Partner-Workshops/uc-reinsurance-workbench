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
import { fmtFull, fmtM, fmtPct } from "../domain/format";
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

  useEffect(() => setSelected(0), [id]);

  if (treaty.isLoading || submission.isLoading || catModel.isLoading) {
    return <Skeleton className="skeleton-card" />;
  }
  if (treaty.error || submission.error || !treaty.data || !layer || !model) {
    return (
      <ErrorBanner
        error={treaty.error ?? submission.error ?? new Error("Pricing data is unavailable.")}
      />
    );
  }

  const treatyData = treaty.data;
  const current = { ...layer, ...edited };
  const preview = priceLayer(current, model);
  const persistedResult = persisted.data?.find((row) => row.treatyLayerId === layer.id);
  const evaluation = evaluateRules(rules.data ?? [], {
    limit: current.limit,
    rateOnLine: preview.rateOnLine,
    cedentRating: submission.data?.cedentRating ?? "C",
    underwriterAuthorityLimit: submission.data?.underwriterAuthorityLimit ?? 0,
    serverReferralReasons: persistedResult?.referralReasons,
  });

  const setField = (key: keyof Editable, value: number) => {
    setEdited((previous) => ({ ...previous, [key]: value }));
  };

  return (
    <>
      <div className="breadcrumb">
        <Link to={`/treaties/${id}`}>Treaty detail</Link>
        <span>/</span>
        Pricing
      </div>
      <PageHeader eyebrow="Pricing / Quote" title={treatyData.name}>
        <span className="preview-label">Preview — server pricing uses saved layer terms</span>
      </PageHeader>
      <div className="layer-tabs">
        {treatyData.layers.map((item, index) => (
          <button
            className={index === selected ? "selected" : ""}
            key={item.id}
            onClick={() => {
              setSelected(index);
              setEdited({});
            }}
          >
            Layer {item.layerNumber}
          </button>
        ))}
      </div>
      <div className="pricing-grid">
        <section className="card">
          <div className="section-heading">
            <strong>Treaty parameters</strong>
            <button className="text-button" onClick={() => setEdited({})}>
              Reset to layer
            </button>
          </div>
          <div className="form-grid">
            {(
              [
                ["limit", "Limit", current.limit, 1000000],
                ["attachment", "Attachment point", current.attachment, 1000000],
                ["reinstatements", "Reinstatements", current.reinstatements, 1],
                [
                  "reinstatementPremiumPct",
                  "Reinstatement premium",
                  current.reinstatementPremiumPct,
                  0.05,
                ],
                ["sharePct", "Our share", current.sharePct, 0.05],
              ] as const
            ).map(([key, label, value, step]) => (
              <label key={key}>
                {label}
                <input
                  type="number"
                  step={step}
                  value={value}
                  onChange={(event) => setField(key, Number(event.target.value))}
                />
              </label>
            ))}
          </div>
          <div className="model-summary">
            <span>
              Portfolio AAL <strong>{fmtM(model.aal)}</strong>
            </span>
            <span>
              PML 250 <strong>{fmtM(model.pml250)}</strong>
            </span>
            <span>
              Model <strong>{model.modelVendor} {model.modelVersion}</strong>
            </span>
          </div>
        </section>
        <section className="stack">
          <div className={preview.error ? "pricing-result error-result" : "pricing-result"}>
            {preview.error ? (
              <>
                <div className="result-label">Pricing failed</div>
                <strong>Layer attachment equals portfolio PML 250.</strong>
                <p>
                  The client preview cannot calculate a hit fraction at this attachment.
                  Server pricing remains authoritative.
                </p>
              </>
            ) : (
              <>
                <div className="result-label">Technical premium</div>
                <strong>
                  {fmtFull(persistedResult?.technicalPremium ?? preview.technicalPremium)}
                </strong>
                <div className="result-meta">
                  Our share {fmtFull(persistedResult?.ourShareLine ?? preview.ourShareLine)} —{" "}
                  {persistedResult ? "Saved quote" : "Preview"}
                </div>
              </>
            )}
          </div>
          {!preview.error && (
            <div className="card">
              <div className="section-heading">
                <strong>Premium build-up</strong>
                <span className="mono">{fmtPct(preview.rateOnLine)} ROL</span>
              </div>
              {[
                ["Expected loss", preview.expectedLoss],
                ["Risk load", preview.riskLoad],
                ["Expense load", preview.expenseLoad],
                [
                  "Profit margin + reinstatements",
                  preview.technicalPremium -
                    preview.expectedLoss -
                    preview.riskLoad -
                    preview.expenseLoad,
                ],
              ].map(([label, value]) => (
                <div className="premium-line" key={label}>
                  <span>{label}</span>
                  <strong>{fmtFull(Number(value))}</strong>
                </div>
              ))}
            </div>
          )}
          <div className="card">
            <div className="section-heading">
              <strong>Referral check</strong>
              <span
                className={`referral-label ${
                  evaluation.some((row) => row.hit) ? "referral" : "within"
                }`}
              >
                {evaluation.some((row) => row.hit) ? "Referral required" : "Within authority"}
              </span>
            </div>
            {evaluation.map((row) => (
              <div className="rule-row" key={row.code}>
                <span className={`rule-dot ${row.hit ? "hit" : row.neutral ? "neutral" : ""}`} />
                <span>
                  <strong>{row.code}</strong>
                  <small>{row.text}</small>
                </span>
              </div>
            ))}
            <div className="modal-actions">
              <button
                className="secondary-button"
                disabled={price.isPending}
                onClick={() => price.mutate(id)}
              >
                Save quote
              </button>
              <button
                className="primary-button"
                disabled={bind.isPending || preview.error}
                onClick={() => bind.mutate(id)}
              >
                Bind treaty
              </button>
              <button
                className="text-button"
                disabled={preview.error}
                onClick={() =>
                  setNotice(
                    `Referred to ${submission.data?.underwriterName ?? "the underwriter"}.`,
                  )
                }
              >
                Refer for approval
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
