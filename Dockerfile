FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
RUN mkdir -p /app/keys && chmod 777 /app/keys

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_DATA_PROTECTION__KEYRING_PATH=/app/keys
EXPOSE 8080
ENTRYPOINT ["dotnet", "ServiceApp.dll"]
