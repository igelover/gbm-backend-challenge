#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Gbm.Challenge.API/Gbm.Challenge.API.csproj", "Gbm.Challenge.API/"]
COPY ["Gbm.Challenge.Application/Gbm.Challenge.Application.csproj", "Gbm.Challenge.Application/"]
COPY ["Gbm.Challenge.Domain/Gbm.Challenge.Domain.csproj", "Gbm.Challenge.Domain/"]
COPY ["Gbm.Challenge.Infrastructure/Gbm.Challenge.Infrastructure.csproj", "Gbm.Challenge.Infrastructure/"]
RUN dotnet restore "Gbm.Challenge.API/Gbm.Challenge.API.csproj"
COPY . .
WORKDIR "/src/Gbm.Challenge.API"
RUN dotnet build "Gbm.Challenge.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gbm.Challenge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gbm.Challenge.API.dll"]