#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NetCommunication/NetCommunication/ServerApp.csproj", "NetCommunication/NetCommunication/"]
COPY ["LiteNetLib/LiteNetLib/LiteNetLib.csproj", "LiteNetLib/LiteNetLib/"]
RUN dotnet restore "NetCommunication/NetCommunication/ServerApp.csproj"
COPY . .
WORKDIR "/src/NetCommunication/NetCommunication"
RUN dotnet build "ServerApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServerApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerApp.dll"]