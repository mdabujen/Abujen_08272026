# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FileProcessingService.csproj", "./"]
RUN dotnet restore "FileProcessingService.csproj"

COPY . .
RUN dotnet publish "FileProcessingService.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FileProcessingService.dll"]