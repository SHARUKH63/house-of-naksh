FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY HouseOfNaksh.sln Directory.Build.props global.json ./
COPY src/HouseOfNaksh.Api/HouseOfNaksh.Api.csproj                       src/HouseOfNaksh.Api/
COPY src/HouseOfNaksh.Domain/HouseOfNaksh.Domain.csproj                 src/HouseOfNaksh.Domain/
COPY src/HouseOfNaksh.Infrastructure/HouseOfNaksh.Infrastructure.csproj src/HouseOfNaksh.Infrastructure/
COPY Tests/HouseOfNaksh.Tests/HouseOfNaksh.Tests.csproj                 Tests/HouseOfNaksh.Tests/
RUN dotnet restore

COPY . .

RUN dotnet publish src/HouseOfNaksh.Api/HouseOfNaksh.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "HouseOfNaksh.Api.dll"]