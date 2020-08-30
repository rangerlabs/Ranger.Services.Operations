FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS restore
WORKDIR /app

ARG BUILD_CONFIG="Release"

RUN mkdir -p /app/vsdbg && touch /app/vsdbg/touched
ENV DEBIAN_FRONTEND noninteractive
RUN if [ "${BUILD_CONFIG}" = "Debug" ]; then \
    apt-get update && \
    apt-get install apt-utils -y --no-install-recommends && \
    apt-get install curl unzip -y && \
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /app/vsdbg; \
    fi
ENV DEBIAN_FRONTEND teletype

ARG MYGET_API_KEY

COPY *.sln ./
COPY ./src/Ranger.Services.Operations/Ranger.Services.Operations.csproj ./src/Ranger.Services.Operations/Ranger.Services.Operations.csproj
COPY ./src/Ranger.Services.Operations.Data/Ranger.Services.Operations.Data.csproj ./src/Ranger.Services.Operations.Data/Ranger.Services.Operations.Data.csproj
COPY ./test/Ranger.Services.Operations.Tests/Ranger.Services.Operations.Tests.csproj ./test/Ranger.Services.Operations.Tests/Ranger.Services.Operations.Tests.csproj
COPY ./scripts ./scripts

RUN ./scripts/create-nuget-config.sh ${MYGET_API_KEY}
RUN dotnet restore

COPY ./src ./src
COPY ./test ./test

RUN dotnet publish -c ${BUILD_CONFIG} -o /app/published --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=restore /app/published .
COPY --from=restore /app/vsdbg ./vsdbg

ARG BUILD_CONFIG="Release"
ARG ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
ARG DOCKER_IMAGE_TAG="NO_TAG"
ENV DOCKER_IMAGE_TAG=${DOCKER_IMAGE_TAG}
ENV DEBIAN_FRONTEND noninteractive
RUN if [ "${BUILD_CONFIG}" = "Debug" ]; then \
    apt-get update && \
    apt-get install procps -y; \
    fi
ENV DEBIAN_FRONTEND teletype

EXPOSE 8083
ENTRYPOINT ["dotnet", "Ranger.Services.Operations.dll"]