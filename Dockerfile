FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Obligatorio-N3D-342742-360021-Client/Obligatorio-N3D-342742-360021-Client.csproj", "Obligatorio-N3D-342742-360021-Client/"]
RUN dotnet restore "Obligatorio-N3D-342742-360021-Client/Obligatorio-N3D-342742-360021-Client.csproj"

COPY Obligatorio-N3D-342742-360021-Client/ Obligatorio-N3D-342742-360021-Client/
RUN dotnet publish "Obligatorio-N3D-342742-360021-Client/Obligatorio-N3D-342742-360021-Client.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render assigns the listen port dynamically via $PORT; fall back to 8080 for local `docker run`.
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet Obligatorio-N3D-342742-360021-Client.dll --urls http://+:${PORT:-8080}"]
