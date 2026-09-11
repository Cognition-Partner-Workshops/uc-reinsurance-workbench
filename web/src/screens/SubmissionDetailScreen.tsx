import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useSubmission, useTransitionSubmission } from "../api/queries";
import { SubmissionStatus, submissionStatusLabels } from "../api/types";
import { fmtDate } from "../domain/format";
import { ErrorBanner, PageHeader, Skeleton, StatusPill } from "../components/Ui";

const legalTargets = (status?: SubmissionStatus) => {
    switch (status) {
        case SubmissionStatus.Received:
            return [
                SubmissionStatus.InReview,
                SubmissionStatus.Declined,
                SubmissionStatus.Withdrawn,
            ];
        case SubmissionStatus.InReview:
            return [SubmissionStatus.Quoted, SubmissionStatus.Declined, SubmissionStatus.Withdrawn];
        case SubmissionStatus.Quoted:
            return [SubmissionStatus.Bound, SubmissionStatus.Declined, SubmissionStatus.Withdrawn];
        default:
            return [];
    }
};

export default function SubmissionDetailScreen() {
    const id = Number(useParams().id);
    const submission = useSubmission(id);
    const transition = useTransitionSubmission();
    const [next, setNext] = useState<SubmissionStatus>();
    const navigate = useNavigate();
    useEffect(() => {
        setNext(legalTargets(submission.data?.status)[0]);
    }, [submission.data?.status]);
    if (submission.isLoading) return <Skeleton className="skeleton-card" />;
    if (submission.error || !submission.data)
        return <ErrorBanner error={submission.error ?? new Error("Submission not found.")} />;
    const item = submission.data;
    const options = legalTargets(item.status);
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
                            value={next ?? ""}
                            disabled={!options.length}
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
                            disabled={transition.isPending || next === undefined}
                            onClick={() => {
                                if (next !== undefined) {
                                    transition.mutate({ id, status: next });
                                }
                            }}
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
