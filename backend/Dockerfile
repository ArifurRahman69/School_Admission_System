# Base image for ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj and restore dependencies
COPY ["School Admission System.csproj", "./"]
RUN dotnet restore "School Admission System.csproj"

# Copy the rest of the code and build
COPY . .
RUN dotnet build "School Admission System.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "School Admission System.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image setup
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "School Admission System.dll"]
