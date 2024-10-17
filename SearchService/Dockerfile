# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY CarsAppBackend.sln .
COPY AuctionService/AuctionService.csproj AuctionService/
COPY SearchService/SearchService.csproj SearchService/
COPY GatewayService/GatewayService.csproj GatewayService/
COPY Contracts/Contracts.csproj Contracts/
COPY IdentityService/IdentityService.csproj IdentityService/

# Restore all dependencies for the solution
RUN dotnet restore

# Copy all files and build the app
COPY ./SearchService ./SearchService
COPY ./Contracts ./Contracts
WORKDIR /src/SearchService
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SearchService.dll"]
