# Etapa 1: compilar e publicar a aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY PensaAntes.API.csproj ./
RUN dotnet restore PensaAntes.API.csproj

COPY . .
RUN dotnet publish PensaAntes.API.csproj -c Release -o /app/publish --no-restore

# Etapa 2: executar a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "PensaAntes.API.dll"]