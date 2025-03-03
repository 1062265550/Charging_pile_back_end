#!/bin/bash
echo "正在启动充电桩API项目..."

echo "1. 还原项目依赖"
dotnet restore

echo "2. 构建项目"
dotnet build

echo "3. 更新数据库"
dotnet ef database update

echo "4. 启动项目"
dotnet run 