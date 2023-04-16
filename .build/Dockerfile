#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG version

WORKDIR /src
COPY ["src/Serilog.Sinks.YandexCloud/Serilog.Sinks.YandexCloud.csproj", "Serilog.Sinks.YandexCloud/"]
RUN dotnet restore "Serilog.Sinks.YandexCloud/Serilog.Sinks.YandexCloud.csproj"
COPY "src/" .
WORKDIR "/src/Serilog.Sinks.YandexCloud"
RUN dotnet build "Serilog.Sinks.YandexCloud.csproj" -c Release -p:PackageVersion=$version -o /build


FROM scratch AS export-stage
COPY --from=build /build /
