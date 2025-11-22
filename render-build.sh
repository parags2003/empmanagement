#!/bin/bash
# Render build script - runs migrations automatically after build

echo "🔨 Building application..."
dotnet restore
dotnet publish -c Release -o out

echo "📦 Build completed successfully!"

echo "🗄️  Running database migrations..."
cd out
dotnet ef database update --project ../EmployeeLeaveManagement.csproj --startup-project ../EmployeeLeaveManagement.csproj

if [ $? -eq 0 ]; then
    echo "✅ Migrations completed successfully!"
else
    echo "⚠️  Migration failed, but continuing deployment..."
    echo "   You may need to run migrations manually via Render Shell"
fi

echo "🚀 Application ready to start!"

