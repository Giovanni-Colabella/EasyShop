FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Installa Node.js 
RUN apt-get update && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y nodejs \
    && npm --version

WORKDIR /app
COPY . .
RUN npm install
RUN npm run css:build
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Frontend.dll"]
