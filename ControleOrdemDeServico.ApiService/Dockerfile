FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ControleOrdemDeServico.ApiService/OsService.ApiService.csproj", "ControleOrdemDeServico.ApiService/"]
COPY ["ControleOrdemDeServico.ServiceDefaults/OsService.ServiceDefaults.csproj", "ControleOrdemDeServico.ServiceDefaults/"]
COPY ["ControleOrdemDeServico.Services/OsService.Services.csproj", "ControleOrdemDeServico.Services/"]
COPY ["ControleOrdemDeServico.Domain/OsService.Domain.csproj", "ControleOrdemDeServico.Domain/"]
COPY ["ControleOrdemDeServico.Infrastructure/OsService.Infrastructure.csproj", "ControleOrdemDeServico.Infrastructure/"]

RUN dotnet restore "./ControleOrdemDeServico.ApiService/OsService.ApiService.csproj"

COPY . .
WORKDIR "/src/ControleOrdemDeServico.ApiService"

RUN dotnet build "./OsService.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OsService.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OsService.ApiService.dll"]