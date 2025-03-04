# 充电桩管理系统 API

## 项目概述

充电桩管理系统API是一个为微信小程序提供后端服务的项目，用于管理电动车充电站和充电订单。该系统支持用户通过微信小程序查找附近充电站、创建充电订单、管理账户等功能。

## 技术栈

- **后端框架**：.NET 8.0
- **数据库**：MySQL 8.0+
- **ORM框架**：Entity Framework Core 8.0
- **日志框架**：Serilog
- **API文档**：Swagger/OpenAPI
- **前端**：微信小程序

## 项目特点

- **智能计费系统**：支持分时段计费，自动计算充电费用
- **实时监控**：支持实时查询充电状态、费用和电量
- **自动化处理**：订单完成时自动计算费用和更新充电口状态
- **完整的异常处理**：包含事务管理和重试机制
- **地理位置服务**：使用空间索引实现高效的附近充电站查询
- **微信登录集成**：通过OpenID实现无缝的微信用户登录
- **实时状态更新**：充电站可用端口实时更新
- **完整订单流程**：支持订单创建、支付、取消等全流程管理

## 项目结构

```
ChargingPile.API/
├── Controllers/                 
│   ├── ChargingStationController.cs
│   ├── ChargingPileController.cs    # 充电桩管理
│   ├── ChargingPortController.cs     # 充电口管理
│   ├── UserController.cs
│   └── OrderController.cs
│
├── Models/
│   ├── Entities/              
│   │   ├── ChargingStation.cs
│   │   ├── ChargingPile.cs         # 充电桩实体
│   │   ├── ChargingPort.cs         # 充电口实体
│   │   ├── User.cs
│   │   └── Order.cs
│   │
│   └── DTOs/                  
│       ├── ChargingStationDTO.cs
│       ├── ChargingPileDTO.cs      # 充电桩DTO
│       ├── ChargingPortDTO.cs      # 充电口DTO
│       ├── UserDTO.cs
│       ├── OrderDTO.cs
│       ├── CreateOrderDTO.cs       # 创建订单DTO
│       ├── UpdateOrderDTO.cs       # 更新订单DTO
│       └── OrderStatusDTO.cs       # 订单状态DTO
│
├── Services/                    
│   └── RateCalculationService.cs   # 费率计算服务
│
├── Data/
│   └── ApplicationDbContext.cs
│
└── Utils/                      
```

## 数据库设计

系统使用MySQL数据库，主要包含以下表：

1. **充电站表(charging_stations)**
   - 使用`VARCHAR(36)`作为主键(GUID格式)
   - 包含经纬度信息和POINT类型的位置字段
   - 使用空间索引优化地理位置查询

2. **充电桩表(charging_piles)**
   - 记录充电桩基本信息
   - 包含功率等计费相关参数
   - 关联充电站

3. **充电口表(charging_ports)**
   - 记录充电口状态和统计信息
   - 关联充电桩和当前订单

4. **用户表(users)**
   - 存储微信用户OpenID和基本信息
   - 管理用户余额和积分

5. **订单表(orders)**
   - 记录充电订单信息
   - 自动计算充电量和费用
   - 关联用户和充电口

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
  ```http
  POST /api/Order
  ```
  请求体：
  ```json
  {
    "userId": 1,
    "stationId": "550e8400-e29b-41d4-a716-446655440000",
    "portId": "550e8400-e29b-41d4-a716-446655440001"
  }
  ```

- **获取订单实时状态**
  ```http
  GET /api/Order/{id}/current-status
  ```
  响应：
  ```json
  {
    "id": 1,
    "orderNo": "uuid",
    "status": 0,
    "startTime": "2024-01-01T10:00:00",
    "endTime": null,
    "currentPowerConsumption": 2.28,
    "currentAmount": 2.41
  }
  ```

- **更新订单状态**
  ```http
  PUT /api/Order/{id}
  ```
  请求体：
  ```json
  {
    "status": 1,
    "paymentStatus": 1
  }
  ```

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

1. **订单费用计算问题**：
   - 检查充电桩功率配置是否正确
   - 确认计费时段设置
   - 验证订单开始和结束时间的准确性

2. **订单状态更新失败**：
   - 检查订单当前状态是否允许更新
   - 确认充电口状态是否正常
   - 验证数据库连接

## 贡献指南

欢迎提交问题和贡献代码！请确保在提交PR之前运行所有测试并通过。

---

感谢使用充电桩管理系统API！ 