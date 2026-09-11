FROM mono:6.12 AS build

# The official Mono image is amd64-only; arm64 hosts must use platform: linux/amd64.
WORKDIR /src
COPY src/ /src/

RUN nuget restore Reinsurance.Core/packages.config -PackagesDirectory /src/packages \
    && nuget restore Reinsurance.Data/packages.config -PackagesDirectory /src/packages \
    && nuget restore Reinsurance.Services/packages.config -PackagesDirectory /src/packages \
    && nuget restore Reinsurance.Api/packages.config -PackagesDirectory /src/packages \
    && msbuild Reinsurance.Api/Reinsurance.Api.csproj /p:Configuration=Release /v:minimal

FROM mono:6.12

RUN sed -i 's|deb.debian.org|archive.debian.org|g' /etc/apt/sources.list \
    && apt-get -o Acquire::Check-Valid-Until=false update \
    && apt-get install -y --no-install-recommends mono-xsp4 curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /src/Reinsurance.Api/Global.asax /app/Global.asax
COPY --from=build /src/Reinsurance.Api/Web.config /app/Web.config
COPY --from=build /src/Reinsurance.Api/bin/ /app/bin/

EXPOSE 5055
HEALTHCHECK --interval=10s --timeout=10s --start-period=120s --retries=12 \
    CMD curl --fail --silent http://localhost:5055/api/health || exit 1
CMD ["xsp4", "--port", "5055", "--address", "0.0.0.0", "--nonstop", "--root", "/app", "--minThreads", "1"]
