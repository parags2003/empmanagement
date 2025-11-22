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
