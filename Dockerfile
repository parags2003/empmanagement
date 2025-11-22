<<<<<<< HEAD
# --------------------------
# 1. BUILD STAGE
# --------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore packages
RUN dotnet restore

# Build application in Release mode
RUN dotnet build -c Release -o /app/build

# Publish application
RUN dotnet publish -c Release -o /app/publish


# --------------------------
# 2. RUNTIME STAGE
# --------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port (Render uses this)
EXPOSE 8080

# ASP.NET Core needs this on Render
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Entry point
ENTRYPOINT ["dotnet", "EmployeeLeaveManagement.dll"]
=======
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EmployeeLeaveManagement.dll"]
>>>>>>> 180c8d6e3702f0f58af4dcbf9776c9620f9f8e60
