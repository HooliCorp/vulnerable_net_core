FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG SHIFTLEFT_ORG_ID
ARG SHIFTLEFT_ACCESS_TOKEN
ARG BRANCH
WORKDIR /src
COPY . .
RUN dotnet restore vulnerable_asp_net_core.sln
# Download ShiftLeft
RUN curl https://cdn.shiftleft.io/download/sl > sl && chmod a+rx sl
# Perform sl analysis
RUN ./sl analyze --verbose --wait --app vulnerable_net_core_docker --tag branch=$BRANCH --csharp --dotnet-core --cpg vulnerable_asp_net_core.sln -- /p:configuration="Release" /p:BuildProjectReferences="true" /p:SkipCompilerExecution="false" /p:CopyBuildOutputToOutputDirectory="true" /p:CopyOutputSymbolsToOutputDirectory="true" /p:SkipCopyBuildProduct="false"
