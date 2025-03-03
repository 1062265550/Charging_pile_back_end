# 充电桩管理系统 API

## 项目概述

充电桩管理系统API是一个为微信小程序提供后端服务的项目，用于管理电动车充电站和充电订单。该系统支持用户通过微信小程序查找附近充电站、创建充电订单、管理账户等功能。

## 技术栈

- **后端框架**：.NET 9.0
- **数据库**：MySQL 8.0+（支持空间索引）
- **ORM框架**：Entity Framework Core
- **空间数据**：NetTopologySuite
- **前端**：微信小程序

## 项目特点

- **地理位置服务**：使用空间索引实现高效的附近充电站查询
- **微信登录集成**：通过OpenID实现无缝的微信用户登录
- **实时状态更新**：充电站可用端口实时更新
- **完整订单流程**：支持订单创建、支付、取消等全流程管理

## 项目结构

```
ChargingPile.API/
├── Controllers/                     # 控制器
│   ├── ChargingStationController.cs # 充电站相关接口
│   ├── UserController.cs            # 用户相关接口
│   └── OrderController.cs           # 订单相关接口
│
├── Models/
│   ├── Entities/                    # 数据库实体
│   │   ├── ChargingStation.cs       # 充电站实体
│   │   ├── User.cs                  # 用户实体
│   │   └── Order.cs                 # 订单实体
│   │
│   └── DTOs/                        # 数据传输对象
│       ├── ChargingStationDTO.cs    # 充电站DTO
│       ├── UserDTO.cs               # 用户DTO
│       └── OrderDTO.cs              # 订单DTO
│
├── Services/                        # 服务层
│
├── Data/
│   ├── ApplicationDbContext.cs      # 数据库上下文
│
├── Utils/                           # 工具类
│
└── DatabaseSetup.sql                # 数据库初始化脚本
```

## 数据库设计

系统使用MySQL数据库，主要包含以下表：

1. **充电站表(charging_stations)**
   - 使用`VARCHAR(36)`作为主键(GUID格式)
   - 包含经纬度信息和POINT类型的位置字段
   - 使用空间索引优化地理位置查询

2. **用户表(users)**
   - 存储微信用户OpenID和基本信息
   - 管理用户余额和积分

3. **订单表(orders)**
   - 记录充电订单信息
   - 关联用户和充电站

## 数据库初始化

1. 确保MySQL 8.0+已安装并支持空间索引
2. 使用MySQL客户端登录到你的MySQL服务器：
   ```bash
   mysql -u root -p
   ```

3. 在MySQL命令行中执行SQL文件：
   ```sql
   SOURCE /path/to/ChargingPile.API/DatabaseSetup.sql;
   ```
   请将 `/path/to/` 替换为 `DatabaseSetup.sql` 文件的实际路径。

## 配置项目

1. 在`appsettings.json`中配置数据库连接字符串：
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;port=3306;database=charging_pile_db;user=root;password=yourpassword"
     }
   }
   ```

2. 确保已安装所需NuGet包：
   - Microsoft.EntityFrameworkCore
   - Pomelo.EntityFrameworkCore.MySql
   - NetTopologySuite
   - Pomelo.EntityFrameworkCore.MySql.NetTopologySuite

## 运行项目

1. 确保已安装 .NET 9.0 SDK。
2. 在项目根目录下运行以下命令：
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
3. 项目将运行在 `https://localhost:5001`。
4. 可通过 `https://localhost:5001/swagger` 访问API文档。

## API接口详情

### 充电站相关

- **获取附近充电站**
  - 请求：`GET /api/ChargingStation/nearby?latitude=30.5&longitude=104.1&radius=1000`
  - 参数：
    - latitude: 纬度
    - longitude: 经度
    - radius: 搜索半径(米)
  - 响应：返回指定范围内的充电站列表

- **获取充电站详情**
  - 请求：`GET /api/ChargingStation/{id}`
  - 响应：返回指定ID的充电站详细信息

- **创建充电站**
  - 请求：`POST /api/ChargingStation`
  - 权限：仅管理员
  - 请求体：充电站信息

### 用户相关

- **用户登录/注册**
  - 请求：`POST /api/User/login`
  - 请求体：微信OpenID
  - 响应：用户信息（新用户自动注册）

- **获取用户信息**
  - 请求：`GET /api/User/profile?openId={openId}`
  - 响应：用户基本信息和当前充电状态

### 订单相关

- **创建充电订单**
  - 请求：`POST /api/Order`
  - 请求体：包含用户ID和充电站ID的订单信息

- **获取用户订单列表**
  - 请求：`GET /api/Order?userId={userId}&status={status}`
  - 参数：
    - userId: 用户ID
    - status: 订单状态(可选)
  - 响应：订单列表

- **结束充电**
  - 请求：`PUT /api/Order/{id}/end`
  - 响应：更新后的订单信息

## 性能优化

1. **空间索引**：使用MySQL空间索引和NetTopologySuite优化地理位置查询
2. **缓存机制**：对热点数据如附近充电站实现缓存
3. **分页查询**：大数据量查询支持分页

## 安全性考虑

1. 用户身份验证基于微信OpenID
2. 敏感操作需要验证用户身份
3. 防止SQL注入和XSS攻击

## 开发指南

1. **添加新API**：
   - 在相应Controller中添加新方法
   - 添加适当的路由和HTTP方法
   - 实现业务逻辑
   - 更新Swagger文档注释

2. **数据库迁移**：
   ```bash
   dotnet ef migrations add MigrationName
   dotnet ef database update
   ```

## 常见问题

1. **空间查询不返回结果**：
   - 检查坐标系统是否一致(SRID 4326)
   - 确认搜索半径是否合适
   - 验证数据库中是否有符合条件的数据

2. **微信登录失败**：
   - 确认OpenID格式正确
   - 检查数据库连接

## 贡献指南

欢迎提交问题和贡献代码！请确保在提交PR之前运行所有测试并通过。


---

感谢使用充电桩管理系统API！ 