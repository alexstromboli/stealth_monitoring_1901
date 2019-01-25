FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY ./GetchMarsRoverPhoto.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/bin/Release/netcoreapp2.1 .
ENTRYPOINT ["dotnet", "GetchMarsRoverPhoto.dll"]
