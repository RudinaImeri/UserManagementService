#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["UserManagement.API/", "UserManagement.API/"]
COPY ["UserManagement.Core/", "UserManagement.Core/"]
COPY ["UserManagement.Domain/", "UserManagement.Domain/"]


RUN dotnet restore "UserManagement.API/UserManagement.API.csproj"
COPY . .
WORKDIR "/src/UserManagement.API"
RUN dotnet build "UserManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000
EXPOSE 443
ENV ASPNETCORE_URLS=http://*:5000

ENTRYPOINT ["dotnet", "UserManagement.API.dll"]