import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useSubmission, useTransitionSubmission } from "../api/queries";
import { SubmissionStatus, submissionStatusLabels } from "../api/types";
import { fmtDate } from "../domain/format";
import { ErrorBanner, PageHeader, Skeleton, StatusPill } from "../components/Ui";

export default function SubmissionDetailScreen() {
    const id = Number(useParams().id);
    const submission = useSubmission(id);
    const transition = useTransitionSubmission();
    const [next, setNext] = useState<SubmissionStatus>(SubmissionStatus.InReview);
    const navigate = useNavigate();
    if (submission.isLoading) return <Skeleton className="skeleton-card" />;
    if (submission.error || !submission.data)
        return <ErrorBanner error={submission.error ?? new Error("Submission not found.")} />;
    const item = submission.data;
    const options =
        item.status === SubmissionStatus.Received
            ? [SubmissionStatus.InReview, SubmissionStatus.Withdrawn]
            : item.status === SubmissionStatus.InReview
              ? [SubmissionStatus.Quoted, SubmissionStatus.Withdrawn]
              : item.status === SubmissionStatus.Quoted
                ? [SubmissionStatus.Bound, SubmissionStatus.Declined, SubmissionStatus.Withdrawn]
                : [SubmissionStatus.Withdrawn];
    return (
        <>
            <PageHeader eyebrow="Submission" title={item.reference}>
                <StatusPill status={item.status} />
            </PageHeader>
            <div className="detail-grid">
                <section className="card placement-card">
                    <div className="section-heading">
                        <strong>Placement</strong>
                    </div>
                    <dl className="details-list">
                        <dt>Cedent</dt>
                        <dd>
                            {item.cedentName} · {item.cedentRating}
                        </dd>
                        <dt>Broker</dt>
                        <dd>{item.brokerName || "—"}</dd>
                        <dt>Underwriter</dt>
                        <dd>{item.underwriterName || "—"}</dd>
                        <dt>Period</dt>
                        <dd>
                            {fmtDate(item.inceptionDate, true)} – {fmtDate(item.expiryDate, true)}
                        </dd>
                        <dt>Notes</dt>
                        <dd>{item.notes || "—"}</dd>
                    </dl>
                </section>
                <section className="card">
                    <div className="section-heading">
                        <strong>Workflow</strong>
                    </div>
                    <p className="muted">
                        Move this submission through the underwriting lifecycle.
                    </p>
                    <div className="inline-form">
                        <select
                            value={next}
                            onChange={(event) => setNext(Number(event.target.value))}
                        >
                            {options.map((status) => (
                                <option key={status} value={status}>
                                    {submissionStatusLabels[status]}
                                </option>
                            ))}
                        </select>
                        <button
                            className="primary-button"
                            disabled={transition.isPending}
                            onClick={() => transition.mutate({ id, status: next })}
                        >
                            Transition
                        </button>
                    </div>
                    {transition.error && <ErrorBanner error={transition.error} />}
                </section>
            </div>
            <section className="card">
                <div className="section-heading">
                    <strong>Treaties</strong>
                </div>
                {item.treaties.map((treaty) => (
                    <button
                        className="simple-row"
                        key={treaty.id}
                        onClick={() => navigate(`/treaties/${treaty.id}`)}
                    >
                        <span>{treaty.name}</span>
                        <StatusPill status={treaty.status} treaty />
                    </button>
                ))}
                {!item.treaties.length && (
                    <div className="empty-state">
                        No treaties have been created for this submission.
                    </div>
                )}
            </section>
            <Link className="back-link" to="/submissions">
                ← Back to submission queue
            </Link>
        </>
    );
}
