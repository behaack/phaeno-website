FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY phaeno.api/phaeno.api.csproj phaeno.api/
RUN dotnet restore phaeno.api/phaeno.api.csproj

COPY . .
RUN dotnet publish phaeno.api/phaeno.api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "phaeno.api.dll"]
