# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG BUILD_CONFIGURATION=Release

COPY net8-api-docker-sqlserver-tls-fix.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c ${BUILD_CONFIGURATION} -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Allow legacy TLS versions (TLS 1.0/1.1) so older SQL Server instances can negotiate SSL.
# OpenSSL on Debian Bookworm disables these by default, causing pre-login handshake failures
# against SQL Servers that don't support TLS 1.2+.
RUN sed -i 's/\[openssl_init\]/# [openssl_init]/' /etc/ssl/openssl.cnf
RUN printf "\n\n[openssl_init]\nssl_conf = ssl_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_sect]\nsystem_default = ssl_default_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_default_sect]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0\n" >> /etc/ssl/openssl.cnf

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "net8-api-docker-sqlserver-tls-fix.dll"]
