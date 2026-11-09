# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

# Copies only the .csproj files first to maximize the restore cache.
COPY src/LayeredCrm.Domain/LayeredCrm.Domain.csproj src/LayeredCrm.Domain/
COPY src/LayeredCrm.DataAccess/LayeredCrm.DataAccess.csproj src/LayeredCrm.DataAccess/
COPY src/LayeredCrm.Business/LayeredCrm.Business.csproj src/LayeredCrm.Business/
COPY src/LayeredCrm.Presentation/LayeredCrm.Presentation.csproj src/LayeredCrm.Presentation/
RUN dotnet restore src/LayeredCrm.Presentation/LayeredCrm.Presentation.csproj

# Copies the source code and publishes in Release.
COPY . .
RUN dotnet publish src/LayeredCrm.Presentation/LayeredCrm.Presentation.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Structurally separate web client, served by the API at "/".
COPY src/LayeredCrm.Web /app/LayeredCrm.Web

# ASP.NET Core listens on HTTP inside the container (the port mapping belongs to compose).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LayeredCrm.Presentation.dll"]
