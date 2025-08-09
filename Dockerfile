# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /app

# COPY *.sln .
# COPY Application/*.csproj ./Application/
# COPY Domain/*.csproj ./Domain/
# COPY Infrastructure/*.csproj ./Infrastructure/
# COPY ExchangeTest/*.csproj ./ExchangeTest/
# COPY ExchangeCurrrency/*.csproj ./ExchangeCurrrency/
# RUN dotnet restore 

# COPY Application/. ./Application/
# COPY Domain/. ./Domain/
# COPY Infrastructure/. ./Infrastructure/
# COPY ExchangeTest/. ./ExchangeTest/
# COPY ExchangeCurrrency/. ./ExchangeCurrrency/
# RUN dotnet publish -c Release -o out
# RUN chmod -R 755 /app


# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# WORKDIR /app
# COPY --from=build /app/out/ ./

# ENV ASPNETCORE_URLS=http://+:5500
# EXPOSE 5500

# ENTRYPOINT ["dotnet", "ExchangeCurrrency.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and all project files
COPY *.sln ./
COPY Application/*.csproj ./Application/
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY ExchangeTest/*.csproj ./ExchangeTest/
COPY ExchangeCurrrency/*.csproj ./ExchangeCurrrency/

RUN dotnet restore

COPY Application/. ./Application/
COPY Domain/. ./Domain/
COPY Infrastructure/. ./Infrastructure/
COPY ExchangeTest/. ./ExchangeTest/
COPY ExchangeCurrrency/. ./ExchangeCurrrency/

# Publish specifically your main project
RUN dotnet publish ./ExchangeCurrrency/ExchangeCurrrency.csproj -c Release -o /app/out

RUN chmod -R 755 /app/out


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out/ ./

ENV ASPNETCORE_URLS=http://+:5500
EXPOSE 5500

ENTRYPOINT ["dotnet", "ExchangeCurrrency.dll"]
