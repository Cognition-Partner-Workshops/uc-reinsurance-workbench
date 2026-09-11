import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import * as Sentry from "@sentry/react";
import App from "./App";
import { initSentry } from "./sentry";
import "./styles.css";

initSentry();

const queryClient = new QueryClient({
    defaultOptions: { queries: { staleTime: 30_000, retry: 1 } },
});

ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
        <Sentry.ErrorBoundary
            fallback={({ eventId, resetError }) => (
                <div className="card error-fallback">
                    <h2>Something went wrong</h2>
                    <p className="muted">Event ID: {eventId}</p>
                    <button
                        className="primary-button"
                        onClick={() => {
                            resetError();
                            window.location.reload();
                        }}
                    >
                        Reload
                    </button>
                </div>
            )}
        >
            <QueryClientProvider client={queryClient}>
                <BrowserRouter>
                    <App />
                </BrowserRouter>
            </QueryClientProvider>
        </Sentry.ErrorBoundary>
    </React.StrictMode>,
);
