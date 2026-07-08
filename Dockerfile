FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY API.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Install CA certificates and ICU for MongoDB Atlas TLS
RUN apk add --no-cache icu-libs ca-certificates && update-ca-certificates

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 10000

ENTRYPOINT ["dotnet", "API.dll"]
