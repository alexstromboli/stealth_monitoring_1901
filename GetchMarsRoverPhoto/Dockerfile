FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY ./GetchMarsRoverPhoto.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /data
ENTRYPOINT ["dotnet", "/app/GetchMarsRoverPhoto.dll"]
COPY --from=build-env /app/bin/Release/netcoreapp2.1 /app
