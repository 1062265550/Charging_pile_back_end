# 充电桩后台管理平台 API 接口文档

## 基本信息

- 基础URL: `http://localhost:5045`
- API版本: v1
- 数据格式: JSON
- 认证方式: 暂无（后续可添加JWT认证）

## 接口列表

### 1. 充电站管理

#### 1.1 获取所有充电站

- **接口**: GET `/api/ChargingStations`
- **描述**: 获取所有充电站信息
- **参数**: 无
- **响应示例**:
```json
[
  {
    "id": "string",
    "name": "充电站名称",
    "status": 1,
    "statusDescription": "在线",
    "address": "充电站地址",
    "location": "经纬度坐标",
    "updateTime": "2025-04-02T12:00:00Z",
    "pileCount": 5
  }
]
```

#### 1.2 获取充电站详情

- **接口**: GET `/api/ChargingStations/{id}`
- **描述**: 获取指定ID的充电站详情
- **参数**: 
  - `id`: 充电站ID（路径参数）
- **响应示例**:
```json
{
  "id": "string",
  "name": "充电站名称",
  "status": 1,
  "statusDescription": "在线",
  "address": "充电站地址",
  "location": "经纬度坐标",
  "updateTime": "2025-04-02T12:00:00Z",
  "pileCount": 5
}
```

#### 1.3 获取充电站分页列表

- **接口**: GET `/api/ChargingStations/paged`
- **描述**: 获取充电站分页列表
- **参数**: 
  - `pageNumber`: 页码，默认为1（查询参数）
  - `pageSize`: 每页记录数，默认为10（查询参数）
  - `status`: 充电站状态，可选（查询参数）
  - `keyword`: 关键字，可选（查询参数）
- **响应示例**:
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true,
  "items": [
    {
      "id": "string",
      "name": "充电站名称",
      "status": 1,
      "statusDescription": "在线",
      "address": "充电站地址",
      "location": "经纬度坐标",
      "updateTime": "2025-04-02T12:00:00Z",
      "pileCount": 5
    }
  ]
}
```

#### 1.4 创建充电站

- **接口**: POST `/api/ChargingStations`
- **描述**: 创建新的充电站
- **请求体**:
```json
{
  "name": "充电站名称",
  "status": 1,
  "address": "充电站地址",
  "location": "经纬度坐标"
}
```
- **响应示例**:
```json
{
  "id": "新创建的充电站ID"
}
```

#### 1.5 更新充电站

- **接口**: PUT `/api/ChargingStations/{id}`
- **描述**: 更新充电站信息
- **参数**: 
  - `id`: 充电站ID（路径参数）
- **请求体**:
```json
{
  "name": "更新后的充电站名称",
  "status": 1,
  "address": "更新后的充电站地址",
  "location": "更新后的经纬度坐标"
}
```
- **响应**: 204 No Content（更新成功）

#### 1.6 删除充电站

- **接口**: DELETE `/api/ChargingStations/{id}`
- **描述**: 删除充电站
- **参数**: 
  - `id`: 充电站ID（路径参数）
- **响应**: 204 No Content（删除成功）

### 2. 充电桩管理

#### 2.1 获取所有充电桩

- **接口**: GET `/api/ChargingPiles`
- **描述**: 获取所有充电桩信息
- **参数**: 无
- **响应示例**:
```json
[
  {
    "id": "string",
    "stationId": "string",
    "stationName": "充电站名称",
    "pileNo": "充电桩编号",
    "pileType": 1,
    "pileTypeDescription": "直流快充",
    "status": 1,
    "statusDescription": "空闲",
    "powerRate": 60.0,
    "manufacturer": "制造商",
    "imei": "IMEI号",
    "totalPorts": 2,
    "protocolVersion": "协议版本",
    "softwareVersion": "软件版本",
    "hardwareVersion": "硬件版本",
    "onlineStatus": 1,
    "onlineStatusDescription": "在线",
    "signalStrength": 85,
    "temperature": 35.5,
    "lastHeartbeatTime": "2025-04-02T12:00:00Z",
    "installationDate": "2025-01-01",
    "lastMaintenanceDate": "2025-03-01",
    "updateTime": "2025-04-02T12:00:00Z"
  }
]
```

#### 2.2 获取充电桩详情

- **接口**: GET `/api/ChargingPiles/{id}`
- **描述**: 获取指定ID的充电桩详情
- **参数**: 
  - `id`: 充电桩ID（路径参数）
- **响应示例**:
```json
{
  "id": "string",
  "stationId": "string",
  "stationName": "充电站名称",
  "pileNo": "充电桩编号",
  "pileType": 1,
  "pileTypeDescription": "直流快充",
  "status": 1,
  "statusDescription": "空闲",
  "powerRate": 60.0,
  "manufacturer": "制造商",
  "imei": "IMEI号",
  "totalPorts": 2,
  "protocolVersion": "协议版本",
  "softwareVersion": "软件版本",
  "hardwareVersion": "硬件版本",
  "onlineStatus": 1,
  "onlineStatusDescription": "在线",
  "signalStrength": 85,
  "temperature": 35.5,
  "lastHeartbeatTime": "2025-04-02T12:00:00Z",
  "installationDate": "2025-01-01",
  "lastMaintenanceDate": "2025-03-01",
  "updateTime": "2025-04-02T12:00:00Z"
}
```

#### 2.3 根据充电站ID获取充电桩列表

- **接口**: GET `/api/ChargingPiles/byStation/{stationId}`
- **描述**: 获取指定充电站下的所有充电桩
- **参数**: 
  - `stationId`: 充电站ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": "string",
    "stationId": "string",
    "stationName": "充电站名称",
    "pileNo": "充电桩编号",
    "pileType": 1,
    "pileTypeDescription": "直流快充",
    "status": 1,
    "statusDescription": "空闲",
    "powerRate": 60.0,
    "manufacturer": "制造商",
    "imei": "IMEI号",
    "totalPorts": 2,
    "protocolVersion": "协议版本",
    "softwareVersion": "软件版本",
    "hardwareVersion": "硬件版本",
    "onlineStatus": 1,
    "onlineStatusDescription": "在线",
    "signalStrength": 85,
    "temperature": 35.5,
    "lastHeartbeatTime": "2025-04-02T12:00:00Z",
    "installationDate": "2025-01-01",
    "lastMaintenanceDate": "2025-03-01",
    "updateTime": "2025-04-02T12:00:00Z"
  }
]
```

#### 2.4 获取充电桩分页列表

- **接口**: GET `/api/ChargingPiles/paged`
- **描述**: 获取充电桩分页列表
- **参数**: 
  - `pageNumber`: 页码，默认为1（查询参数）
  - `pageSize`: 每页记录数，默认为10（查询参数）
  - `stationId`: 充电站ID，可选（查询参数）
  - `status`: 充电桩状态，可选（查询参数）
  - `keyword`: 关键字，可选（查询参数）
- **响应示例**:
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true,
  "items": [
    {
      "id": "string",
      "stationId": "string",
      "stationName": "充电站名称",
      "pileNo": "充电桩编号",
      "pileType": 1,
      "pileTypeDescription": "直流快充",
      "status": 1,
      "statusDescription": "空闲",
      "powerRate": 60.0,
      "manufacturer": "制造商",
      "imei": "IMEI号",
      "totalPorts": 2,
      "protocolVersion": "协议版本",
      "softwareVersion": "软件版本",
      "hardwareVersion": "硬件版本",
      "onlineStatus": 1,
      "onlineStatusDescription": "在线",
      "signalStrength": 85,
      "temperature": 35.5,
      "lastHeartbeatTime": "2025-04-02T12:00:00Z",
      "installationDate": "2025-01-01",
      "lastMaintenanceDate": "2025-03-01",
      "updateTime": "2025-04-02T12:00:00Z"
    }
  ]
}
```

#### 2.5 创建充电桩

- **接口**: POST `/api/ChargingPiles`
- **描述**: 创建新的充电桩
- **请求体**:
```json
{
  "stationId": "充电站ID",
  "pileNo": "充电桩编号",
  "pileType": 1,
  "powerRate": 60,
  "manufacturer": "制造商",
  "imei": "IMEI号",
  "totalPorts": 2,
  "protocolVersion": "协议版本",
  "softwareVersion": "软件版本",
  "hardwareVersion": "硬件版本",
  "installationDate": "2025-01-01"
}
```
- **字段说明**:
  - `stationId`: 所属充电站ID，必填
  - `pileNo`: 充电桩编号，必填，要求唯一
  - `pileType`: 充电桩类型，必填，1=直流快充，2=交流慢充
  - `powerRate`: 额定功率(kW)，必填，必须在0.1至500kW之间
  - `manufacturer`: 制造商，可选
  - `imei`: 设备IMEI号，可选
  - `totalPorts`: 总端口数，必填，必须在1至10之间
  - `protocolVersion`: 协议版本，可选
  - `softwareVersion`: 软件版本，可选
  - `hardwareVersion`: 硬件版本，可选
  - `installationDate`: 安装日期，可选，ISO 8601格式
- **响应示例**:
```json
{
  "id": "新创建的充电桩ID"
}
```

#### 2.6 更新充电桩

- **接口**: PUT `/api/ChargingPiles/{id}`
- **描述**: 更新充电桩信息
- **参数**: 
  - `id`: 充电桩ID（路径参数）
- **请求体**:
```json
{
  "pileNo": "更新后的充电桩编号",
  "pileType": 1,
  "powerRate": 60,
  "manufacturer": "更新后的制造商",
  "imei": "更新后的IMEI号",
  "totalPorts": 2,
  "protocolVersion": "更新后的协议版本",
  "softwareVersion": "更新后的软件版本",
  "hardwareVersion": "更新后的硬件版本",
  "installationDate": "2025-01-01",
  "lastMaintenanceDate": "2025-03-01"
}
```
- **响应**: 204 No Content（更新成功）

#### 2.7 删除充电桩

- **接口**: DELETE `/api/ChargingPiles/{id}`
- **描述**: 删除充电桩
- **参数**: 
  - `id`: 充电桩ID（路径参数）
- **响应**: 204 No Content（删除成功）

### 3. 用户管理

#### 3.1 获取所有用户

- **接口**: GET `/api/Users`
- **描述**: 获取所有用户信息
- **参数**: 无
- **响应示例**:
```json
[
  {
    "id": 1,
    "openId": "微信OpenID",
    "nickname": "用户昵称",
    "avatar": "头像URL",
    "phone": "手机号码",
    "email": "电子邮箱",
    "balance": 100.00,
    "registerTime": "2025-04-02T12:00:00Z",
    "lastLoginTime": "2025-04-02T12:00:00Z",
    "status": 1,
    "statusDescription": "正常"
  }
]
```

#### 3.2 获取用户详情

- **接口**: GET `/api/Users/{id}`
- **描述**: 获取指定ID的用户详情
- **参数**: 
  - `id`: 用户ID（路径参数）
- **响应示例**:
```json
{
  "id": 1,
  "openId": "微信OpenID",
  "nickname": "用户昵称",
  "avatar": "头像URL",
  "phone": "手机号码",
  "email": "电子邮箱",
  "balance": 100.00,
  "registerTime": "2025-04-02T12:00:00Z",
  "lastLoginTime": "2025-04-02T12:00:00Z",
  "status": 1,
  "statusDescription": "正常"
}
```

#### 3.3 获取用户分页列表

- **接口**: GET `/api/Users/paged`
- **描述**: 获取用户分页列表
- **参数**: 
  - `pageNumber`: 页码，默认为1（查询参数）
  - `pageSize`: 每页记录数，默认为10（查询参数）
  - `status`: 用户状态，可选（查询参数）
  - `keyword`: 关键字，可选（查询参数）
- **响应示例**:
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true,
  "items": [
    {
      "id": 1,
      "openId": "微信OpenID",
      "nickname": "用户昵称",
      "avatar": "头像URL",
      "phone": "手机号码",
      "email": "电子邮箱",
      "balance": 100.00,
      "registerTime": "2025-04-02T12:00:00Z",
      "lastLoginTime": "2025-04-02T12:00:00Z",
      "status": 1,
      "statusDescription": "正常"
    }
  ]
}
```

#### 3.4 创建用户

- **接口**: POST `/api/Users`
- **描述**: 创建新的用户
- **请求体**:
```json
{
  "openId": "微信OpenID",
  "nickname": "用户昵称",
  "avatar": "头像URL",
  "phone": "手机号码",
  "email": "电子邮箱",
  "password": "密码"
}
```
- **响应示例**:
```json
{
  "id": 1
}
```

#### 3.5 更新用户

- **接口**: PUT `/api/Users/{id}`
- **描述**: 更新用户信息
- **参数**: 
  - `id`: 用户ID（路径参数）
- **请求体**:
```json
{
  "nickname": "更新后的用户昵称",
  "avatar": "更新后的头像URL",
  "phone": "更新后的手机号码",
  "email": "更新后的电子邮箱",
  "status": 1
}
```
- **响应**: 204 No Content（更新成功）

#### 3.6 删除用户

- **接口**: DELETE `/api/Users/{id}`
- **描述**: 删除用户
- **参数**: 
  - `id`: 用户ID（路径参数）
- **响应**: 204 No Content（删除成功）

#### 3.7 用户登录

- **接口**: POST `/api/Users/login`
- **描述**: 用户登录
- **请求体**:
```json
{
  "openId": "微信OpenID",
  "password": "密码"
}
```
- **响应示例**:
```json
{
  "id": 1,
  "openId": "微信OpenID",
  "nickname": "用户昵称",
  "avatar": "头像URL",
  "token": "登录令牌"
}
```

#### 3.8 修改密码

- **接口**: PUT `/api/Users/{id}/password`
- **描述**: 修改用户密码
- **参数**: 
  - `id`: 用户ID（路径参数）
- **请求体**:
```json
{
  "oldPassword": "旧密码",
  "newPassword": "新密码"
}
```
- **响应**: 204 No Content（修改成功）

#### 3.9 充值余额

- **接口**: PUT `/api/Users/{id}/balance`
- **描述**: 充值用户余额
- **参数**: 
  - `id`: 用户ID（路径参数）
- **请求体**:
```json
{
  "amount": 100.00
}
```
- **响应**: 204 No Content（充值成功）

### 4. 订单管理

#### 4.1 获取所有订单

- **接口**: GET `/api/Orders`
- **描述**: 获取所有订单信息
- **参数**: 无
- **响应示例**:
```json
[
  {
    "id": "string",
    "orderNo": "订单编号",
    "userId": 1,
    "userNickname": "用户昵称",
    "pileId": "string",
    "pileNo": "充电桩编号",
    "portId": "string",
    "portNo": "充电端口编号",
    "status": 1,
    "statusDescription": "充电中",
    "billingMode": 1,
    "billingModeDescription": "电量+电量服务费",
    "startTime": "2025-04-02T12:00:00Z",
    "endTime": "2025-04-02T14:00:00Z",
    "duration": 7200,
    "durationFormatted": "2小时0分钟0秒",
    "electricity": 30.5,
    "amount": 30.5,
    "serviceFee": 15.25,
    "totalAmount": 45.75,
    "paymentStatus": 1,
    "paymentStatusDescription": "已支付",
    "paymentTime": "2025-04-02T12:00:00Z",
    "paymentMethod": 1,
    "paymentMethodDescription": "微信",
    "transactionId": "交易流水号",
    "updateTime": "2025-04-02T14:00:00Z"
  }
]
```

#### 4.2 获取订单详情

- **接口**: GET `/api/Orders/{id}`
- **描述**: 获取指定ID的订单详情
- **参数**: 
  - `id`: 订单ID（路径参数）
- **响应示例**:
```json
{
  "id": "string",
  "orderNo": "订单编号",
  "userId": 1,
  "userNickname": "用户昵称",
  "pileId": "string",
  "pileNo": "充电桩编号",
  "portId": "string",
  "portNo": "充电端口编号",
  "status": 1,
  "statusDescription": "充电中",
  "billingMode": 1,
  "billingModeDescription": "电量+电量服务费",
  "startTime": "2025-04-02T12:00:00Z",
  "endTime": "2025-04-02T14:00:00Z",
  "duration": 7200,
  "durationFormatted": "2小时0分钟0秒",
  "electricity": 30.5,
  "amount": 30.5,
  "serviceFee": 15.25,
  "totalAmount": 45.75,
  "paymentStatus": 1,
  "paymentStatusDescription": "已支付",
  "paymentTime": "2025-04-02T12:00:00Z",
  "paymentMethod": 1,
  "paymentMethodDescription": "微信",
  "transactionId": "交易流水号",
  "updateTime": "2025-04-02T14:00:00Z"
}
```

#### 4.3 获取订单分页列表

- **接口**: GET `/api/Orders/paged`
- **描述**: 获取订单分页列表
- **参数**: 
  - `pageNumber`: 页码，默认为1（查询参数）
  - `pageSize`: 每页记录数，默认为10（查询参数）
  - `userId`: 用户ID，可选（查询参数）
  - `pileId`: 充电桩ID，可选（查询参数）
  - `status`: 订单状态，可选（查询参数）
  - `startDate`: 开始日期，可选（查询参数）
  - `endDate`: 结束日期，可选（查询参数）
  - `keyword`: 关键字，可选（查询参数）
- **响应示例**:
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true,
  "items": [
    {
      "id": "string",
      "orderNo": "订单编号",
      "userId": 1,
      "userNickname": "用户昵称",
      "pileId": "string",
      "pileNo": "充电桩编号",
      "portId": "string",
      "portNo": "充电端口编号",
      "status": 1,
      "statusDescription": "充电中",
      "billingMode": 1,
      "billingModeDescription": "电量+电量服务费",
      "startTime": "2025-04-02T12:00:00Z",
      "endTime": "2025-04-02T14:00:00Z",
      "duration": 7200,
      "durationFormatted": "2小时0分钟0秒",
      "electricity": 30.5,
      "amount": 30.5,
      "serviceFee": 15.25,
      "totalAmount": 45.75,
      "paymentStatus": 1,
      "paymentStatusDescription": "已支付",
      "paymentTime": "2025-04-02T12:00:00Z",
      "paymentMethod": 1,
      "paymentMethodDescription": "微信",
      "transactionId": "交易流水号",
      "updateTime": "2025-04-02T14:00:00Z"
    }
  ]
}
```

#### 4.4 获取用户订单列表

- **接口**: GET `/api/Orders/byUser/{userId}`
- **描述**: 获取指定用户的所有订单
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": "string",
    "orderNo": "订单编号",
    "userId": 1,
    "userNickname": "用户昵称",
    "pileId": "string",
    "pileNo": "充电桩编号",
    "portId": "string",
    "portNo": "充电端口编号",
    "status": 1,
    "statusDescription": "充电中",
    "billingMode": 1,
    "billingModeDescription": "电量+电量服务费",
    "startTime": "2025-04-02T12:00:00Z",
    "endTime": "2025-04-02T14:00:00Z",
    "duration": 7200,
    "durationFormatted": "2小时0分钟0秒",
    "electricity": 30.5,
    "amount": 30.5,
    "serviceFee": 15.25,
    "totalAmount": 45.75,
    "paymentStatus": 1,
    "paymentStatusDescription": "已支付",
    "paymentTime": "2025-04-02T12:00:00Z",
    "paymentMethod": 1,
    "paymentMethodDescription": "微信",
    "transactionId": "交易流水号",
    "updateTime": "2025-04-02T14:00:00Z"
  }
]
```

#### 4.5 获取充电桩订单列表

- **接口**: GET `/api/Orders/byPile/{pileId}`
- **描述**: 获取指定充电桩的所有订单
- **参数**: 
  - `pileId`: 充电桩ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": "string",
    "orderNo": "订单编号",
    "userId": 1,
    "userNickname": "用户昵称",
    "pileId": "string",
    "pileNo": "充电桩编号",
    "portId": "string",
    "portNo": "充电端口编号",
    "status": 1,
    "statusDescription": "充电中",
    "billingMode": 1,
    "billingModeDescription": "电量+电量服务费",
    "startTime": "2025-04-02T12:00:00Z",
    "endTime": "2025-04-02T14:00:00Z",
    "duration": 7200,
    "durationFormatted": "2小时0分钟0秒",
    "electricity": 30.5,
    "amount": 30.5,
    "serviceFee": 15.25,
    "totalAmount": 45.75,
    "paymentStatus": 1,
    "paymentStatusDescription": "已支付",
    "paymentTime": "2025-04-02T12:00:00Z",
    "paymentMethod": 1,
    "paymentMethodDescription": "微信",
    "transactionId": "交易流水号",
    "updateTime": "2025-04-02T14:00:00Z"
  }
]
```

#### 4.6 创建订单

- **接口**: POST `/api/Orders`
- **描述**: 创建新的订单
- **请求体**:
```json
{
  "userId": 1,
  "pileId": "充电桩ID",
  "portId": "充电端口ID",
  "billingMode": 1
}
```
- **响应示例**:
```json
{
  "id": "新创建的订单ID"
}
```

#### 4.7 更新订单状态

- **接口**: PUT `/api/Orders/{id}/status`
- **描述**: 更新订单状态
- **参数**: 
  - `id`: 订单ID（路径参数）
- **请求体**:
```json
{
  "status": 1
}
```
- **响应**: 204 No Content（更新成功）

#### 4.8 订单支付

- **接口**: POST `/api/Orders/{id}/pay`
- **描述**: 支付订单
- **参数**: 
  - `id`: 订单ID（路径参数）
- **请求体**:
```json
{
  "paymentMethod": 1,
  "transactionId": "交易流水号"
}
```
- **响应**: 204 No Content（支付成功）

#### 4.9 取消订单

- **接口**: POST `/api/Orders/{id}/cancel`
- **描述**: 取消订单
- **参数**: 
  - `id`: 订单ID（路径参数）
- **响应**: 204 No Content（取消成功）

#### 4.10 完成订单

- **接口**: POST `/api/Orders/{id}/complete`
- **描述**: 完成订单
- **参数**: 
  - `id`: 订单ID（路径参数）
  - `electricity`: 充电电量（查询参数）
  - `duration`: 充电时长（查询参数）
- **响应**: 204 No Content（完成成功）

#### 4.11 获取订单统计数据

- **接口**: GET `/api/Orders/statistics`
- **描述**: 获取订单统计数据
- **参数**: 
  - `startDate`: 开始日期，可选（查询参数）
  - `endDate`: 结束日期，可选（查询参数）
- **响应示例**:
```json
{
  "totalOrders": 100,
  "totalElectricity": 3000.5,
  "totalDuration": 360000,
  "totalAmount": 5000.75,
  "pendingPaymentOrders": 10,
  "chargingOrders": 20,
  "completedOrders": 60,
  "cancelledOrders": 5,
  "abnormalOrders": 5
}
```

### 5. 用户角色管理

#### 5.1 获取所有角色

- **接口**: GET `/api/UserRoles`
- **描述**: 获取所有角色信息
- **参数**: 无
- **响应示例**:
```json
[
  {
    "id": 1,
    "name": "普通用户",
    "description": "普通用户角色",
    "permissionLevel": 1,
    "permissionLevelDescription": "普通用户",
    "isSystem": true
  }
]
```

#### 5.2 获取角色详情

- **接口**: GET `/api/UserRoles/{id}`
- **描述**: 获取指定ID的角色详情
- **参数**: 
  - `id`: 角色ID（路径参数）
- **响应示例**:
```json
{
  "id": 1,
  "name": "普通用户",
  "description": "普通用户角色",
  "permissionLevel": 1,
  "permissionLevelDescription": "普通用户",
  "isSystem": true
}
```

#### 5.3 创建角色

- **接口**: POST `/api/UserRoles`
- **描述**: 创建新的角色
- **请求体**:
```json
{
  "name": "VIP用户",
  "description": "VIP用户角色",
  "permissionLevel": 2
}
```
- **响应示例**:
```json
{
  "id": 2
}
```

#### 5.4 更新角色

- **接口**: PUT `/api/UserRoles/{id}`
- **描述**: 更新角色信息
- **参数**: 
  - `id`: 角色ID（路径参数）
- **请求体**:
```json
{
  "name": "高级VIP用户",
  "description": "高级VIP用户角色",
  "permissionLevel": 2
}
```
- **响应**: 204 No Content（更新成功）

#### 5.5 删除角色

- **接口**: DELETE `/api/UserRoles/{id}`
- **描述**: 删除角色
- **参数**: 
  - `id`: 角色ID（路径参数）
- **响应**: 204 No Content（删除成功）

#### 5.6 获取用户的角色列表

- **接口**: GET `/api/UserRoles/byUser/{userId}`
- **描述**: 获取指定用户的所有角色
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": 1,
    "name": "普通用户",
    "description": "普通用户角色",
    "permissionLevel": 1,
    "permissionLevelDescription": "普通用户",
    "isSystem": true
  }
]
```

#### 5.7 分配用户角色

- **接口**: POST `/api/UserRoles/assign`
- **描述**: 分配角色给用户
- **请求体**:
```json
{
  "userId": 1,
  "roleId": 2
}
```
- **响应**: 200 OK（分配成功）

#### 5.8 移除用户角色

- **接口**: DELETE `/api/UserRoles/remove/{userId}/{roleId}`
- **描述**: 移除用户的角色
- **参数**: 
  - `userId`: 用户ID（路径参数）
  - `roleId`: 角色ID（路径参数）
- **响应**: 204 No Content（移除成功）

#### 5.9 检查用户是否拥有指定角色

- **接口**: GET `/api/UserRoles/check/{userId}/{roleId}`
- **描述**: 检查用户是否拥有指定角色
- **参数**: 
  - `userId`: 用户ID（路径参数）
  - `roleId`: 角色ID（路径参数）
- **响应示例**:
```json
true
```

#### 5.10 检查用户是否拥有指定权限级别

- **接口**: GET `/api/UserRoles/checkPermission/{userId}/{permissionLevel}`
- **描述**: 检查用户是否拥有指定权限级别
- **参数**: 
  - `userId`: 用户ID（路径参数）
  - `permissionLevel`: 权限级别（路径参数）
- **响应示例**:
```json
true
```

### 6. 用户地址管理

#### 6.1 获取用户的所有地址

- **接口**: GET `/api/UserAddresses/byUser/{userId}`
- **描述**: 获取指定用户的所有地址
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": 1,
    "userId": 1,
    "recipientName": "张三",
    "recipientPhone": "13800138000",
    "province": "广东省",
    "city": "深圳市",
    "district": "南山区",
    "detailAddress": "科技园路1号",
    "fullAddress": "广东省深圳市南山区科技园路1号",
    "postalCode": "518000",
    "isDefault": true,
    "tag": "公司",
    "createTime": "2025-04-02T12:00:00Z",
    "updateTime": "2025-04-02T12:00:00Z"
  }
]
```

#### 6.2 获取地址详情

- **接口**: GET `/api/UserAddresses/{id}`
- **描述**: 获取指定ID的地址详情
- **参数**: 
  - `id`: 地址ID（路径参数）
- **响应示例**:
```json
{
  "id": 1,
  "userId": 1,
  "recipientName": "张三",
  "recipientPhone": "13800138000",
  "province": "广东省",
  "city": "深圳市",
  "district": "南山区",
  "detailAddress": "科技园路1号",
  "fullAddress": "广东省深圳市南山区科技园路1号",
  "postalCode": "518000",
  "isDefault": true,
  "tag": "公司",
  "createTime": "2025-04-02T12:00:00Z",
  "updateTime": "2025-04-02T12:00:00Z"
}
```

#### 6.3 获取用户的默认地址

- **接口**: GET `/api/UserAddresses/default/{userId}`
- **描述**: 获取指定用户的默认地址
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
{
  "id": 1,
  "userId": 1,
  "recipientName": "张三",
  "recipientPhone": "13800138000",
  "province": "广东省",
  "city": "深圳市",
  "district": "南山区",
  "detailAddress": "科技园路1号",
  "fullAddress": "广东省深圳市南山区科技园路1号",
  "postalCode": "518000",
  "isDefault": true,
  "tag": "公司",
  "createTime": "2025-04-02T12:00:00Z",
  "updateTime": "2025-04-02T12:00:00Z"
}
```

#### 6.4 创建用户地址

- **接口**: POST `/api/UserAddresses/{userId}`
- **描述**: 创建新的用户地址
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **请求体**:
```json
{
  "recipientName": "张三",
  "recipientPhone": "13800138000",
  "province": "广东省",
  "city": "深圳市",
  "district": "南山区",
  "detailAddress": "科技园路1号",
  "postalCode": "518000",
  "isDefault": true,
  "tag": "公司"
}
```
- **响应示例**:
```json
{
  "id": 1
}
```

#### 6.5 更新用户地址

- **接口**: PUT `/api/UserAddresses/{id}`
- **描述**: 更新用户地址信息
- **参数**: 
  - `id`: 地址ID（路径参数）
- **请求体**:
```json
{
  "recipientName": "李四",
  "recipientPhone": "13900139000",
  "province": "广东省",
  "city": "深圳市",
  "district": "福田区",
  "detailAddress": "福中路1号",
  "postalCode": "518000",
  "isDefault": true,
  "tag": "家"
}
```
- **响应**: 204 No Content（更新成功）

#### 6.6 删除用户地址

- **接口**: DELETE `/api/UserAddresses/{id}`
- **描述**: 删除用户地址
- **参数**: 
  - `id`: 地址ID（路径参数）
- **响应**: 204 No Content（删除成功）

#### 6.7 设置默认地址

- **接口**: PUT `/api/UserAddresses/{userId}/default/{addressId}`
- **描述**: 设置用户的默认地址
- **参数**: 
  - `userId`: 用户ID（路径参数）
  - `addressId`: 地址ID（路径参数）
- **响应**: 204 No Content（设置成功）

### 7. 用户消息通知管理

#### 7.1 获取用户的所有通知

- **接口**: GET `/api/UserNotifications/byUser/{userId}`
- **描述**: 获取指定用户的所有通知
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": 1,
    "userId": 1,
    "title": "订单已完成",
    "content": "您的订单已完成，感谢您的使用！",
    "type": 2,
    "typeDescription": "订单通知",
    "relatedId": "订单ID",
    "isRead": false,
    "readTime": null,
    "createTime": "2025-04-02T12:00:00Z"
  }
]
```

#### 7.2 获取用户的未读通知

- **接口**: GET `/api/UserNotifications/unread/{userId}`
- **描述**: 获取指定用户的未读通知
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": 1,
    "userId": 1,
    "title": "订单已完成",
    "content": "您的订单已完成，感谢您的使用！",
    "type": 2,
    "typeDescription": "订单通知",
    "relatedId": "订单ID",
    "isRead": false,
    "readTime": null,
    "createTime": "2025-04-02T12:00:00Z"
  }
]
```

#### 7.3 获取通知详情

- **接口**: GET `/api/UserNotifications/{id}`
- **描述**: 获取指定ID的通知详情
- **参数**: 
  - `id`: 通知ID（路径参数）
- **响应示例**:
```json
{
  "id": 1,
  "userId": 1,
  "title": "订单已完成",
  "content": "您的订单已完成，感谢您的使用！",
  "type": 2,
  "typeDescription": "订单通知",
  "relatedId": "订单ID",
  "isRead": false,
  "readTime": null,
  "createTime": "2025-04-02T12:00:00Z"
}
```

#### 7.4 创建用户通知

- **接口**: POST `/api/UserNotifications`
- **描述**: 创建新的用户通知
- **请求体**:
```json
{
  "userId": 1,
  "title": "订单已完成",
  "content": "您的订单已完成，感谢您的使用！",
  "type": 2,
  "relatedId": "订单ID"
}
```
- **响应示例**:
```json
{
  "id": 1
}
```

#### 7.5 批量创建用户通知

- **接口**: POST `/api/UserNotifications/batch`
- **描述**: 批量创建用户通知
- **请求体**:
```json
{
  "userIds": [1, 2, 3],
  "title": "系统维护通知",
  "content": "系统将于今晚22:00-23:00进行维护，请提前做好准备。",
  "type": 1,
  "relatedId": null
}
```
- **响应示例**:
```json
{
  "count": 3
}
```

#### 7.6 标记通知为已读

- **接口**: PUT `/api/UserNotifications/read/{id}`
- **描述**: 标记指定通知为已读
- **参数**: 
  - `id`: 通知ID（路径参数）
- **响应**: 204 No Content（标记成功）

#### 7.7 批量标记通知为已读

- **接口**: PUT `/api/UserNotifications/batchRead`
- **描述**: 批量标记通知为已读
- **请求体**:
```json
{
  "notificationIds": [1, 2, 3]
}
```
- **响应示例**:
```json
{
  "count": 3
}
```

#### 7.8 标记用户所有通知为已读

- **接口**: PUT `/api/UserNotifications/readAll/{userId}`
- **描述**: 标记用户所有通知为已读
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
{
  "count": 10
}
```

#### 7.9 删除通知

- **接口**: DELETE `/api/UserNotifications/{id}`
- **描述**: 删除指定通知
- **参数**: 
  - `id`: 通知ID（路径参数）
- **响应**: 204 No Content（删除成功）

#### 7.10 获取用户未读通知数量

- **接口**: GET `/api/UserNotifications/unreadCount/{userId}`
- **描述**: 获取用户未读通知数量
- **参数**: 
  - `userId`: 用户ID（路径参数）
- **响应示例**:
```json
{
  "count": 5
}
```

### 8. 充电端口管理

#### 8.1 获取所有充电端口

- **接口**: GET `/api/ChargingPorts`
- **描述**: 获取所有充电端口信息
- **参数**: 无
- **响应示例**:
```json
[
  {
    "id": "string",
    "pileId": "string",
    "pileNo": "充电桩编号",
    "portNo": "端口编号",
    "portType": 1,
    "portTypeDescription": "国标",
    "status": 1,
    "statusDescription": "空闲",
    "faultType": 0,
    "faultTypeDescription": "无故障",
    "isDisabled": false,
    "currentOrderId": "string",
    "voltage": 220.5,
    "currentAmpere": 16.8,
    "power": 3.5,
    "temperature": 35.2,
    "electricity": 10.5,
    "totalChargingTimes": 120,
    "totalChargingDuration": 36000,
    "totalPowerConsumption": 360.5,
    "lastCheckTime": "2025-04-02T12:00:00Z",
    "lastOrderId": "string",
    "updateTime": "2025-04-02T12:00:00Z"
  }
]
```

#### 8.2 获取充电端口详情

- **接口**: GET `/api/ChargingPorts/{id}`
- **描述**: 获取指定ID的充电端口详情
- **参数**: 
  - `id`: 充电端口ID（路径参数）
- **响应示例**:
```json
{
  "id": "string",
  "pileId": "string",
  "pileNo": "充电桩编号",
  "portNo": "端口编号",
  "portType": 1,
  "portTypeDescription": "国标",
  "status": 1,
  "statusDescription": "空闲",
  "faultType": 0,
  "faultTypeDescription": "无故障",
  "isDisabled": false,
  "currentOrderId": "string",
  "voltage": 220.5,
  "currentAmpere": 16.8,
  "power": 3.5,
  "temperature": 35.2,
  "electricity": 10.5,
  "totalChargingTimes": 120,
  "totalChargingDuration": 36000,
  "totalPowerConsumption": 360.5,
  "lastCheckTime": "2025-04-02T12:00:00Z",
  "lastOrderId": "string",
  "updateTime": "2025-04-02T12:00:00Z"
}
```

#### 8.3 获取充电桩下的所有充电端口

- **接口**: GET `/api/ChargingPorts/by-pile/{pileId}`
- **描述**: 获取指定充电桩下的所有充电端口
- **参数**: 
  - `pileId`: 充电桩ID（路径参数）
- **响应示例**:
```json
[
  {
    "id": "string",
    "pileId": "string",
    "pileNo": "充电桩编号",
    "portNo": "端口编号",
    "portType": 1,
    "portTypeDescription": "国标",
    "status": 1,
    "statusDescription": "空闲",
    "faultType": 0,
    "faultTypeDescription": "无故障",
    "isDisabled": false,
    "currentOrderId": "string",
    "voltage": 220.5,
    "currentAmpere": 16.8,
    "power": 3.5,
    "temperature": 35.2,
    "electricity": 10.5,
    "totalChargingTimes": 120,
    "totalChargingDuration": 36000,
    "totalPowerConsumption": 360.5,
    "lastCheckTime": "2025-04-02T12:00:00Z",
    "lastOrderId": "string",
    "updateTime": "2025-04-02T12:00:00Z"
  }
]
```

#### 8.4 获取充电端口分页列表

- **接口**: GET `/api/ChargingPorts/paged`
- **描述**: 获取充电端口分页列表
- **参数**: 
  - `pageNumber`: 页码，默认为1（查询参数）
  - `pageSize`: 每页记录数，默认为10（查询参数）
  - `pileId`: 充电桩ID，可选（查询参数）
  - `status`: 充电端口状态，可选（查询参数）
  - `keyword`: 关键字，可选（查询参数）
- **响应示例**:
```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true,
  "items": [
    {
      "id": "string",
      "pileId": "string",
      "pileNo": "充电桩编号",
      "portNo": "端口编号",
      "portType": 1,
      "portTypeDescription": "国标",
      "status": 1,
      "statusDescription": "空闲",
      "faultType": 0,
      "faultTypeDescription": "无故障",
      "isDisabled": false,
      "currentOrderId": "string",
      "voltage": 220.5,
      "currentAmpere": 16.8,
      "power": 3.5,
      "temperature": 35.2,
      "electricity": 10.5,
      "totalChargingTimes": 120,
      "totalChargingDuration": 36000,
      "totalPowerConsumption": 360.5,
      "lastCheckTime": "2025-04-02T12:00:00Z",
      "lastOrderId": "string",
      "updateTime": "2025-04-02T12:00:00Z"
    }
  ]
}
```

#### 8.5 创建充电端口

- **接口**: POST `/api/ChargingPorts`
- **描述**: 创建新的充电端口
- **请求体**:
```json
{
  "pileId": "充电桩ID",
  "portNo": "端口编号",
  "portType": 1
}
```
- **字段说明**:
  - `pileId`: 所属充电桩ID，必填
  - `portNo`: 端口编号，必填，在同一充电桩下要求唯一
  - `portType`: 端口类型，可选，1=国标，2=欧标，3=美标
- **响应示例**:
```json
{
  "id": "新创建的充电端口ID"
}
```

#### 8.6 更新充电端口

- **接口**: PUT `/api/ChargingPorts/{id}`
- **描述**: 更新充电端口信息
- **参数**: 
  - `id`: 充电端口ID（路径参数）
- **请求体**:
```json
{
  "portNo": "更新后的端口编号",
  "portType": 2,
  "isDisabled": false
}
```
- **响应**: 204 No Content（更新成功）

#### 8.7 删除充电端口

- **接口**: DELETE `/api/ChargingPorts/{id}`
- **描述**: 删除充电端口
- **参数**: 
  - `id`: 充电端口ID（路径参数）
- **响应**: 204 No Content（删除成功）

## 状态码说明

- `200 OK`: 请求成功
- `201 Created`: 资源创建成功
- `204 No Content`: 请求成功，无返回内容
- `400 Bad Request`: 请求参数错误
- `404 Not Found`: 资源不存在
- `500 Internal Server Error`: 服务器内部错误

## 数据字典

### 充电站状态

| 状态码 | 描述 |
| ----- | ---- |
| 0 | 离线 |
| 1 | 在线 |
| 2 | 维护中 |
| 3 | 故障 |

### 充电桩类型

| 类型码 | 描述 |
| ----- | ---- |
| 1 | 直流快充 |
| 2 | 交流慢充 |

### 充电桩状态

| 状态码 | 描述 |
| ----- | ---- |
| 0 | 离线 |
| 1 | 空闲 |
| 2 | 使用中 |
| 3 | 故障 |

### 充电桩在线状态

| 状态码 | 描述 |
| ----- | ---- |
| 0 | 离线 |
| 1 | 在线 |

### 充电端口类型

| 类型码 | 描述 |
| ----- | ---- |
| 1 | 国标 |
| 2 | 欧标 |
| 3 | 美标 |

### 用户状态

| 状态码 | 描述 |
| ----- | ---- |
| 0 | 禁用 |
| 1 | 正常 |

### 用户性别

| 状态码 | 描述 |
| ----- | ---- |
| 0 | 未知 |
| 1 | 男 |
| 2 | 女 |

### 用户权限级别

| 级别码 | 描述 |
| ----- | ---- |
| 1 | 普通用户 |
| 2 | VIP用户 |
| 3 | 管理员 |
| 4 | 超级管理员 |

### 订单状态

| 状态码 | 描述 |
| ----- | ---- |
| 0 | 待支付 |
| 1 | 充电中 |
| 2 | 已完成 |
| 3 | 已取消 |
| 4 | 异常 |

### 订单计费模式

| 模式码 | 描述 |
| ----- | ---- |
| 0 | 按时间 |
| 1 | 电量+电量服务费 |
| 2 | 电量+时间服务费 |

### 订单支付状态

| 状态码 | 描述 |
| ----- | ---- |
| 0 | 未支付 |
| 1 | 已支付 |
| 2 | 已退款 |

### 订单支付方式

| 方式码 | 描述 |
| ----- | ---- |
| 1 | 微信 |
| 2 | 支付宝 |
| 3 | 余额 |

### 通知类型

| 类型码 | 描述 |
| ----- | ---- |
| 1 | 系统通知 |
| 2 | 订单通知 |
| 3 | 活动通知 |
| 4 | 充值通知 |
