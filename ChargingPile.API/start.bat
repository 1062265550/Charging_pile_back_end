@echo off
chcp 65001
echo 正在启动充电桩API项目...

echo 0. 结束可能存在的进程
taskkill /F /IM ChargingPile.API.exe 2>nul

echo 1. 清理项目
dotnet clean

echo 2. 安装 Entity Framework Core 工具
dotnet tool install --global dotnet-ef --version 9.0.0-preview.1.24081.2

echo 3. 还原项目依赖
dotnet restore

echo 4. 构建项目
dotnet build

echo 5. 更新数据库
dotnet ef database update

echo 6. 启动项目
dotnet run

pause 