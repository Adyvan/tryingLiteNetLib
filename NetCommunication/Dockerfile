# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY NetCommunication/*.csproj ./NetCommunication/

RUN dotnet restore --runtime linux-x64

# copy everything else and build app
COPY NetCommunication/. ./NetCommunication/
WORKDIR /source/NetCommunication
#for prod: 
RUN dotnet publish --configuration release --output /app --runtime linux-x64 --self-contained false
# RUN dotnet publish -p:PublishProfileFullPath=/Properties/PublishProfiles/DockerProfile.pubxml --configuration release --output /app --runtime linux-x64 --self-contained false

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal-amd64
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["./NetCommunication"]
EXPOSE 9050