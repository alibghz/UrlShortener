FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["UrlShortener.Web/UrlShortener.Web.csproj", "UrlShortener.Web/"]
RUN dotnet restore "UrlShortener.Web/UrlShortener.Web.csproj"
COPY . .
WORKDIR "/src/UrlShortener.Web"
RUN dotnet build "UrlShortener.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UrlShortener.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlShortener.Web.dll"]