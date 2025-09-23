FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Direcional.Imobiliaria.sln", "/src/"]
COPY ["Direcional.Api/Direcional.Api.csproj", "Direcional.Api/"]
COPY ["Direcional.Application/Direcional.Application.csproj", "Direcional.Application/"]
COPY ["Direcional.Domain/Direcional.Domain.csproj", "Direcional.Domain/"]
COPY ["Direcional.Infrastructure/Direcional.Infrastructure.csproj", "Direcional.Infrastructure/"]
COPY ["Direcional.Tests/Direcional.Tests.csproj", "Direcional.Tests/"]
RUN dotnet restore "Direcional.Imobiliaria.sln"
COPY . .
RUN dotnet publish "Direcional.Api/Direcional.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Direcional.Api.dll"]


