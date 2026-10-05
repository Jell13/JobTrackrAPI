FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY JobTrackrAPI/JobTrackrAPI.csproj JobTrackrAPI/
RUN dotnet restore JobTrackrAPI/JobTrackrAPI.csproj
COPY . .
RUN dotnet publish JobTrackrAPI/JobTrackrAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "JobTrackrAPI.dll"]
