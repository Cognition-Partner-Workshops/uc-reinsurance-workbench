FROM node:20-alpine AS build

WORKDIR /web
COPY web/package*.json ./
RUN npm ci
COPY web/ ./

ARG VITE_SENTRY_DSN
ARG VITE_SENTRY_ENVIRONMENT=docker
ARG VITE_SENTRY_RELEASE
ENV VITE_SENTRY_DSN=${VITE_SENTRY_DSN}
ENV VITE_SENTRY_ENVIRONMENT=${VITE_SENTRY_ENVIRONMENT}
ENV VITE_SENTRY_RELEASE=${VITE_SENTRY_RELEASE}

RUN npm run build

FROM nginx:1.27-alpine

COPY --from=build /web/dist/ /usr/share/nginx/html/
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
