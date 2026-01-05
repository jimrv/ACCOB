# Etapa de compilación (Uso del SDK de .NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Configuración del entorno
ENV ASPNETCORE_ENVIRONMENT Production 

# NOMBRE DEL APLICATIVO PARA TU PROYECTO ACCOB
ENV APP_NET_CORE ACCOB.dll 

CMD ASPNETCORE_URLS=http://*:$PORT dotnet $APP_NET_CORE