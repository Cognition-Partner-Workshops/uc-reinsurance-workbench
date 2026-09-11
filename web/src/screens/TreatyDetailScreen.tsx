import { Link, useNavigate, useParams } from "react-router-dom";
import {
  useBindTreaty,
  useCatModel,
  useCedents,
  useSubmission,
  useTreaty,
  useTreatyPricing,
} from "../api/queries";
import { TreatyStatus, treatyTypeLabels } from "../api/types";
import { fmtDate, fmtM, fmtPct } from "../domain/format";
import { ErrorBanner, PageHeader, Skeleton, StatusPill } from "../components/Ui";

export default function TreatyDetailScreen() {
  const id = Number(useParams().id);
  const treaty = useTreaty(id);
  const submission = useSubmission(treaty.data?.submissionId);
  const model = useCatModel(treaty.data?.submissionId);
  const pricing = useTreatyPricing(id);
  const cedents = useCedents();
  const bind = useBindTreaty();
  const navigate = useNavigate();

  if (treaty.isLoading || submission.isLoading) {
    return <Skeleton className="skeleton-card" />;
  }
  if (treaty.error || submission.error || !treaty.data || !submission.data) {
    return (
      <ErrorBanner
        error={treaty.error ?? submission.error ?? new Error("Treaty not found.")}
      />
    );
  }

  const item = treaty.data;
  const placement = submission.data;
  const cat =
    (model.data ?? []).find((row) => row.regionId == null && row.perilId == null) ??
    model.data?.[0];
  const results = pricing.data ?? [];
  const totalLimit = item.layers.reduce((sum, layer) => sum + layer.limit, 0);
  const totalPremium =
    results.length === item.layers.length
      ? results.reduce((sum, row) => sum + row.technicalPremium, 0)
      : undefined;
  const totalLoss = results.reduce((sum, row) => sum + row.expectedLoss, 0);
  const country = cedents.data?.find((cedent) => cedent.id === placement.cedentId)?.country;

  return (
    <>
      <div className="breadcrumb">
        <Link to="/">Portfolio</Link>
        <span>/</span>
        {placement.reference}
      </div>
      <PageHeader
        eyebrow={`${placement.cedentName} — ${treatyTypeLabels[item.type]}`}
        title={item.name}
      >
        <div className="header-actions">
          <StatusPill status={item.status} treaty />
          <button
            className="secondary-button"
            onClick={() => navigate(`/treaties/${id}/pricing`)}
          >
            Re-price
          </button>
          <button
            className="primary-button"
            disabled={item.status === TreatyStatus.Bound || bind.isPending}
            onClick={() => bind.mutate(id)}
          >
            {item.status === TreatyStatus.Bound ? "Bound" : "Bind treaty"}
          </button>
        </div>
      </PageHeader>
      {bind.error && <ErrorBanner error={bind.error} />}
      <div className="stats-grid">
        {[
          ["Total limit", fmtM(totalLimit)],
          ["Attachment (L1)", fmtM(item.layers[0]?.attachment ?? 0)],
          ["Technical premium", totalPremium === undefined ? "—" : fmtM(totalPremium)],
          ["Expected loss", fmtM(totalLoss)],
          ["Modeled PML 250", cat ? fmtM(cat.pml250) : "—"],
        ].map(([label, value]) => (
          <div className="card stat-card" key={label}>
            <div className="muted">{label}</div>
            <strong>{value}</strong>
          </div>
        ))}
      </div>
      <div className="detail-grid">
        <section className="card table-card">
          <div className="section-heading">
            <strong>Layers</strong>
            <span className="muted">{item.currency}</span>
          </div>
          <div className="table-head layers-grid">
            <span>Layer</span>
            <span>Limit</span>
            <span>Attach</span>
            <span>Exp. loss</span>
            <span>Premium</span>
            <span>ROL</span>
            <span>Reinst</span>
          </div>
          {item.layers.map((layer) => {
            const result = results.find((row) => row.treatyLayerId === layer.id);
            return (
              <div className="table-row layers-grid" key={layer.id}>
                <span>L{layer.layerNumber}</span>
                <span>{fmtM(layer.limit)}</span>
                <span>{fmtM(layer.attachment)}</span>
                <span>{result ? fmtM(result.expectedLoss) : "—"}</span>
                <span>{result ? fmtM(result.technicalPremium) : "—"}</span>
                <span>{result ? fmtPct(result.rateOnLine) : "—"}</span>
                <span>
                  {layer.reinstatements} @ {layer.reinstatementPremiumPct * 100}%
                </span>
              </div>
            );
          })}
          <div className="table-total layers-grid">
            <strong>Total</strong>
            <strong>{fmtM(totalLimit)}</strong>
            <span />
            <strong>{fmtM(totalLoss)}</strong>
            <strong>{totalPremium === undefined ? "—" : fmtM(totalPremium)}</strong>
            <span />
            <span />
          </div>
        </section>
        <div className="stack">
          <section className="card">
            <div className="section-heading">
              <strong>Cat model</strong>
            </div>
            <dl className="details-list">
              <dt>Model</dt>
              <dd>{cat ? `${cat.modelVendor} ${cat.modelVersion}` : "—"}</dd>
              <dt>AAL</dt>
              <dd>{cat ? fmtM(cat.aal) : "—"}</dd>
              <dt>PML 50 / 100 / 250</dt>
              <dd>
                {cat
                  ? `${fmtM(cat.pml50)} / ${fmtM(cat.pml100)} / ${fmtM(cat.pml250)}`
                  : "—"}
              </dd>
            </dl>
          </section>
          <section className="card">
            <div className="section-heading">
              <strong>Placement</strong>
            </div>
            <dl className="details-list">
              <dt>Submission</dt>
              <dd>{placement.reference}</dd>
              <dt>Cedent rating</dt>
              <dd>
                {placement.cedentRating} — {country ?? "—"}
              </dd>
              <dt>Broker</dt>
              <dd>{placement.brokerName}</dd>
              <dt>Underwriter</dt>
              <dd>{placement.underwriterName}</dd>
              <dt>Received</dt>
              <dd>{fmtDate(placement.receivedOn, true)}</dd>
            </dl>
          </section>
        </div>
      </div>
    </>
  );
}
