# syntax=docker/dockerfile:1

FROM node:16-bullseye-slim AS node

# ASP.NET Core 3.1 project (netcoreapp3.1)
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

COPY --from=node /usr/local/ /usr/local/

COPY coursework-itransition.csproj ./
RUN dotnet restore ./coursework-itransition.csproj

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . ./
RUN dotnet publish ./coursework-itransition.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "coursework-itransition.dll"]
