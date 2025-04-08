-- ChargingPileDB 数据库结构 (SQL Server版)
-- 导出时间：2025-04-02
-- 数据库版本：SQL Server 

-- 创建数据库
USE master;
GO

-- 如果数据库已存在，则删除
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ChargingPileDB')
BEGIN
    ALTER DATABASE ChargingPileDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ChargingPileDB;
END
GO

-- 创建新数据库
CREATE DATABASE ChargingPileDB;
GO

-- 使用新创建的数据库
USE ChargingPileDB;
GO

--
-- 表结构
--

-- 充电站表
CREATE TABLE charging_stations (
    id NVARCHAR(36) PRIMARY KEY,                -- 充电站唯一标识符
    name NVARCHAR(100) NOT NULL,                -- 充电站名称
    status INT NOT NULL,                        -- 充电站状态：0-离线，1-在线，2-维护中，3-故障
    address NVARCHAR(200),                      -- 充电站地址
    location GEOGRAPHY,                         -- 充电站地理位置坐标
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '充电站信息表', 'SCHEMA', 'dbo', 'TABLE', 'charging_stations';
EXEC sp_addextendedproperty 'MS_Description', '充电站唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'charging_stations', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '充电站名称', 'SCHEMA', 'dbo', 'TABLE', 'charging_stations', 'COLUMN', 'name';
EXEC sp_addextendedproperty 'MS_Description', '充电站状态：0-离线，1-在线，2-维护中，3-故障', 'SCHEMA', 'dbo', 'TABLE', 'charging_stations', 'COLUMN', 'status';
EXEC sp_addextendedproperty 'MS_Description', '充电站地址', 'SCHEMA', 'dbo', 'TABLE', 'charging_stations', 'COLUMN', 'address';
EXEC sp_addextendedproperty 'MS_Description', '充电站地理位置坐标', 'SCHEMA', 'dbo', 'TABLE', 'charging_stations', 'COLUMN', 'location';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'charging_stations', 'COLUMN', 'update_time';
GO

-- 充电桩表（优化合并了充电桩配置和状态信息）
CREATE TABLE charging_piles (
    id NVARCHAR(36) PRIMARY KEY,                -- 充电桩唯一标识符
    station_id NVARCHAR(36) NOT NULL,           -- 所属充电站ID
    pile_no NVARCHAR(50) NOT NULL,              -- 充电桩编号
    pile_type INT NOT NULL,                     -- 充电桩类型：1-直流快充，2-交流慢充
    status INT NOT NULL,                        -- 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
    power_rate DECIMAL(18, 2) NOT NULL,         -- 额定功率(kW)
    manufacturer NVARCHAR(100),                 -- 制造商
    imei NVARCHAR(20),                          -- 设备IMEI号
    total_ports INT NOT NULL DEFAULT 1,         -- 总端口数
    floating_power DECIMAL(18, 2),              -- 浮充参考功率(W)
    auto_stop_enabled BIT DEFAULT 1,            -- 充满自停开关状态
    plugin_wait_time INT DEFAULT 300,           -- 充电器插入等待时间(秒)
    removal_wait_time INT DEFAULT 300,          -- 充电器移除等待时间(秒)
    supports_new_protocol BIT DEFAULT 0,        -- 是否支持新协议格式(带IMEI)
    
    -- 以下是从charging_pile_configs合并的字段
    protocol_version NVARCHAR(50),              -- 协议版本
    software_version NVARCHAR(50),              -- 软件版本
    hardware_version NVARCHAR(50),              -- 硬件版本
    ccid NVARCHAR(50),                          -- CCID号
    voltage NVARCHAR(20),                       -- 额定电压
    ampere_value NVARCHAR(20),                  -- 额定电流
    
    -- 以下是从charging_pile_status合并的字段
    online_status SMALLINT,                     -- 在线状态：0-离线，1-在线
    signal_strength INT,                        -- 信号强度
    temperature DECIMAL(18, 2),                 -- 设备温度
    last_heartbeat_time DATETIME,               -- 最后心跳时间
    
    installation_date DATE,                     -- 安装日期
    last_maintenance_date DATE,                 -- 最后维护日期
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '充电桩信息表（合并了配置和状态信息）', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles';
EXEC sp_addextendedproperty 'MS_Description', '充电桩唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '所属充电站ID', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'station_id';
EXEC sp_addextendedproperty 'MS_Description', '充电桩编号', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'pile_no';
EXEC sp_addextendedproperty 'MS_Description', '充电桩类型：1-直流快充，2-交流慢充', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'pile_type';
EXEC sp_addextendedproperty 'MS_Description', '充电桩状态：0-离线，1-空闲，2-使用中，3-故障', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'status';
EXEC sp_addextendedproperty 'MS_Description', '额定功率(kW)', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'power_rate';
EXEC sp_addextendedproperty 'MS_Description', '制造商', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'manufacturer';
EXEC sp_addextendedproperty 'MS_Description', '设备IMEI号', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'imei';
EXEC sp_addextendedproperty 'MS_Description', '总端口数', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'total_ports';
EXEC sp_addextendedproperty 'MS_Description', '浮充参考功率(W)', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'floating_power';
EXEC sp_addextendedproperty 'MS_Description', '充满自停开关状态', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'auto_stop_enabled';
EXEC sp_addextendedproperty 'MS_Description', '充电器插入等待时间(秒)', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'plugin_wait_time';
EXEC sp_addextendedproperty 'MS_Description', '充电器移除等待时间(秒)', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'removal_wait_time';
EXEC sp_addextendedproperty 'MS_Description', '是否支持新协议格式(带IMEI)', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'supports_new_protocol';
EXEC sp_addextendedproperty 'MS_Description', '协议版本', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'protocol_version';
EXEC sp_addextendedproperty 'MS_Description', '软件版本', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'software_version';
EXEC sp_addextendedproperty 'MS_Description', '硬件版本', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'hardware_version';
EXEC sp_addextendedproperty 'MS_Description', 'CCID号', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'ccid';
EXEC sp_addextendedproperty 'MS_Description', '额定电压', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'voltage';
EXEC sp_addextendedproperty 'MS_Description', '额定电流', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'ampere_value';
EXEC sp_addextendedproperty 'MS_Description', '在线状态：0-离线，1-在线', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'online_status';
EXEC sp_addextendedproperty 'MS_Description', '信号强度', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'signal_strength';
EXEC sp_addextendedproperty 'MS_Description', '设备温度', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'temperature';
EXEC sp_addextendedproperty 'MS_Description', '最后心跳时间', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'last_heartbeat_time';
EXEC sp_addextendedproperty 'MS_Description', '安装日期', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'installation_date';
EXEC sp_addextendedproperty 'MS_Description', '最后维护日期', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'last_maintenance_date';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'charging_piles', 'COLUMN', 'update_time';
GO

-- 充电端口表（合并了端口状态和统计信息）
CREATE TABLE charging_ports (
    id NVARCHAR(36) PRIMARY KEY,                -- 充电端口唯一标识符
    pile_id NVARCHAR(36) NOT NULL,              -- 所属充电桩ID
    port_no NVARCHAR(50) NOT NULL,              -- 端口编号
    port_type INT,                              -- 端口类型：1-国标，2-欧标，3-美标
    status INT,                                 -- 端口状态：0-离线，1-空闲，2-使用中，3-故障
    fault_type SMALLINT DEFAULT 0,              -- 故障类型：0-无故障，1-保险丝熔断，2-继电器粘连
    is_disabled BIT DEFAULT 0,                  -- 是否禁用
    current_order_id NVARCHAR(36),              -- 当前订单 ID
    
    -- 以下是从charging_port_status合并的字段
    voltage DECIMAL(18, 2),                     -- 当前电压(V)
    current_ampere DECIMAL(18, 2),              -- 当前电流(A)
    power DECIMAL(18, 2),                       -- 当前功率(kW)
    temperature DECIMAL(18, 2),                 -- 端口温度
    electricity DECIMAL(18, 2),                 -- 当前电量(kWh)
    
    -- 以下是从charging_port_statistics合并的字段
    total_charging_times INT DEFAULT 0,         -- 总充电次数
    total_charging_duration INT DEFAULT 0,      -- 总充电时长(秒)
    total_power_consumption DECIMAL(18, 2) DEFAULT 0, -- 总耗电量(kWh)
    last_check_time DATETIME,                   -- 最后检查时间
    last_order_id NVARCHAR(36),                 -- 最后一个订单ID
    
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '充电端口信息表（合并了状态和统计信息）', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports';
EXEC sp_addextendedproperty 'MS_Description', '充电端口唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '所属充电桩ID', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'pile_id';
EXEC sp_addextendedproperty 'MS_Description', '端口编号', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'port_no';
EXEC sp_addextendedproperty 'MS_Description', '端口类型：1-国标，2-欧标，3-美标', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'port_type';
EXEC sp_addextendedproperty 'MS_Description', '端口状态：0-离线，1-空闲，2-使用中，3-故障', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'status';
EXEC sp_addextendedproperty 'MS_Description', '故障类型：0-无故障，1-保险丝熔断，2-继电器粘连', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'fault_type';
EXEC sp_addextendedproperty 'MS_Description', '是否禁用', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'is_disabled';
EXEC sp_addextendedproperty 'MS_Description', '当前订单ID', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'current_order_id';
EXEC sp_addextendedproperty 'MS_Description', '当前电压(V)', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'voltage';
EXEC sp_addextendedproperty 'MS_Description', '当前电流(A)', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'current_ampere';
EXEC sp_addextendedproperty 'MS_Description', '当前功率(kW)', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'power';
EXEC sp_addextendedproperty 'MS_Description', '端口温度', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'temperature';
EXEC sp_addextendedproperty 'MS_Description', '当前电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'electricity';
EXEC sp_addextendedproperty 'MS_Description', '总充电次数', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'total_charging_times';
EXEC sp_addextendedproperty 'MS_Description', '总充电时长(秒)', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'total_charging_duration';
EXEC sp_addextendedproperty 'MS_Description', '总耗电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'total_power_consumption';
EXEC sp_addextendedproperty 'MS_Description', '最后检查时间', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'last_check_time';
EXEC sp_addextendedproperty 'MS_Description', '最后一个订单ID', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'last_order_id';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'charging_ports', 'COLUMN', 'update_time';
GO

-- 用户表（优化合并了用户配置）
CREATE TABLE users (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 用户唯一标识符
    open_id NVARCHAR(100) NOT NULL,             -- 用户开放平台ID
    nickname NVARCHAR(50),                      -- 用户昵称
    avatar NVARCHAR(200),                       -- 用户头像URL
    password NVARCHAR(100),                     -- 用户密码（加密存储）
    balance DECIMAL(18, 2) NOT NULL DEFAULT 0,  -- 账户余额
    points INT NOT NULL DEFAULT 0,              -- 用户积分
    
    -- 以下是从user_profiles合并的字段
    gender INT,                                 -- 性别：0-未知，1-男，2-女
    country NVARCHAR(50),                       -- 国家
    province NVARCHAR(50),                      -- 省份
    city NVARCHAR(50),                          -- 城市
    language NVARCHAR(20),                      -- 语言偏好
    
    last_login_time DATETIME,                   -- 最后登录时间
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 用户角色表
CREATE TABLE user_roles (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 角色唯一标识符
    name NVARCHAR(50) NOT NULL,                 -- 角色名称
    description NVARCHAR(200),                  -- 角色描述
    permission_level INT NOT NULL,              -- 权限级别：1-普通用户，2-VIP用户，3-管理员，4-超级管理员
    is_system BIT NOT NULL DEFAULT 0            -- 是否系统内置角色
);

-- 用户角色映射表
CREATE TABLE user_role_mappings (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 映射唯一标识符
    user_id INT NOT NULL,                       -- 用户ID
    role_id INT NOT NULL,                       -- 角色ID
    CONSTRAINT fk_user_role_mappings_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_role_mappings_role FOREIGN KEY (role_id) REFERENCES user_roles(id) ON DELETE CASCADE
);

-- 用户地址表
CREATE TABLE user_addresses (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 地址唯一标识符
    user_id INT NOT NULL,                       -- 用户ID
    recipient_name NVARCHAR(50) NOT NULL,       -- 收件人姓名
    recipient_phone NVARCHAR(20) NOT NULL,      -- 收件人电话
    province NVARCHAR(50) NOT NULL,             -- 省份
    city NVARCHAR(50) NOT NULL,                 -- 城市
    district NVARCHAR(50) NOT NULL,             -- 区县
    detail_address NVARCHAR(200) NOT NULL,      -- 详细地址
    postal_code NVARCHAR(20),                   -- 邮政编码
    is_default BIT NOT NULL DEFAULT 0,          -- 是否默认地址
    tag NVARCHAR(20),                           -- 标签：家、公司、学校等
    create_time DATETIME NOT NULL,              -- 创建时间
    update_time DATETIME,                       -- 更新时间
    CONSTRAINT fk_user_addresses_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- 用户消息通知表
CREATE TABLE user_notifications (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 通知唯一标识符
    user_id INT NOT NULL,                       -- 用户ID
    title NVARCHAR(100) NOT NULL,               -- 通知标题
    content NVARCHAR(1000) NOT NULL,            -- 通知内容
    type INT NOT NULL,                          -- 通知类型：1-系统通知，2-订单通知，3-活动通知，4-充值通知
    related_id NVARCHAR(50),                    -- 相关ID，如订单ID
    is_read BIT NOT NULL DEFAULT 0,             -- 是否已读
    read_time DATETIME,                         -- 阅读时间
    create_time DATETIME NOT NULL,              -- 创建时间
    CONSTRAINT fk_user_notifications_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- 创建索引
CREATE INDEX idx_user_role_mappings_user_id ON user_role_mappings(user_id);
CREATE INDEX idx_user_role_mappings_role_id ON user_role_mappings(role_id);
CREATE INDEX idx_user_addresses_user_id ON user_addresses(user_id);
CREATE INDEX idx_user_addresses_is_default ON user_addresses(is_default);
CREATE INDEX idx_user_notifications_user_id ON user_notifications(user_id);
CREATE INDEX idx_user_notifications_type ON user_notifications(type);
CREATE INDEX idx_user_notifications_is_read ON user_notifications(is_read);

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '用户信息表（合并了用户配置）', 'SCHEMA', 'dbo', 'TABLE', 'users';
EXEC sp_addextendedproperty 'MS_Description', '用户唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '用户开放平台ID', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'open_id';
EXEC sp_addextendedproperty 'MS_Description', '用户昵称', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'nickname';
EXEC sp_addextendedproperty 'MS_Description', '用户头像URL', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'avatar';
EXEC sp_addextendedproperty 'MS_Description', '用户密码（加密存储）', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'password';
EXEC sp_addextendedproperty 'MS_Description', '账户余额', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'balance';
EXEC sp_addextendedproperty 'MS_Description', '用户积分', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'points';
EXEC sp_addextendedproperty 'MS_Description', '性别：0-未知，1-男，2-女', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'gender';
EXEC sp_addextendedproperty 'MS_Description', '国家', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'country';
EXEC sp_addextendedproperty 'MS_Description', '省份', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'province';
EXEC sp_addextendedproperty 'MS_Description', '城市', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'city';
EXEC sp_addextendedproperty 'MS_Description', '语言偏好', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'language';
EXEC sp_addextendedproperty 'MS_Description', '最后登录时间', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'last_login_time';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'users', 'COLUMN', 'update_time';
GO

-- 用户卡表
CREATE TABLE user_cards (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 卡唯一标识符
    user_id INT NOT NULL,                       -- 所属用户ID
    card_no NVARCHAR(50) NOT NULL,              -- 卡号
    status SMALLINT NOT NULL,                   -- 卡状态：0-无效，1-有效，2-挂失
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '用户卡信息表', 'SCHEMA', 'dbo', 'TABLE', 'user_cards';
EXEC sp_addextendedproperty 'MS_Description', '卡唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'user_cards', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '所属用户ID', 'SCHEMA', 'dbo', 'TABLE', 'user_cards', 'COLUMN', 'user_id';
EXEC sp_addextendedproperty 'MS_Description', '卡号', 'SCHEMA', 'dbo', 'TABLE', 'user_cards', 'COLUMN', 'card_no';
EXEC sp_addextendedproperty 'MS_Description', '卡状态：0-无效，1-有效，2-挂失', 'SCHEMA', 'dbo', 'TABLE', 'user_cards', 'COLUMN', 'status';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'user_cards', 'COLUMN', 'update_time';
GO

-- 订单表（优化与合并了时段详情）
CREATE TABLE orders (
    id NVARCHAR(36) PRIMARY KEY,                -- 订单唯一标识符
    order_no NVARCHAR(36) NOT NULL,             -- 订单编号
    user_id INT NOT NULL,                       -- 用户ID
    pile_id NVARCHAR(36) NOT NULL,              -- 充电桩ID
    port_id NVARCHAR(36) NOT NULL,              -- 充电端口ID
    amount DECIMAL(18, 2) NOT NULL,             -- 订单总金额(元)
    start_time DATETIME NOT NULL,               -- 充电开始时间
    end_time DATETIME,                          -- 充电结束时间
    power_consumption DECIMAL(18, 3) NOT NULL,  -- 耗电量(kWh)
    charging_time INT NOT NULL,                 -- 充电时长(秒)
    power DECIMAL(18, 2) NOT NULL,              -- 充电功率(kW)
    status INT NOT NULL,                        -- 订单状态：0-创建，1-进行中，2-已完成，3-已取消，4-异常
    payment_status INT NOT NULL,                -- 支付状态：0-未支付，1-已支付，2-已退款
    charging_mode INT NOT NULL,                 -- 充电模式：1-充满自停，2-按金额，3-按时间，4-按电量，5-其他
    stop_reason SMALLINT,                       -- 停止原因：0-充满自停，1-时间用完，2-金额用完，3-手动停止，4-电量用完
    billing_mode SMALLINT DEFAULT 0,            -- 计费模式：0-按时间，1-电量+电量服务费，2-电量+时间服务费
    
    -- 以下是从order_time_period_details合并的字段
    sharp_electricity DECIMAL(18, 3) DEFAULT 0, -- 尖峰时段电量(kWh)
    sharp_amount DECIMAL(18, 2) DEFAULT 0,      -- 尖峰时段金额(元)
    peak_electricity DECIMAL(18, 3) DEFAULT 0,  -- 峰时段电量(kWh)
    peak_amount DECIMAL(18, 2) DEFAULT 0,       -- 峰时段金额(元)
    flat_electricity DECIMAL(18, 3) DEFAULT 0,  -- 平时段电量(kWh)
    flat_amount DECIMAL(18, 2) DEFAULT 0,       -- 平时段金额(元)
    valley_electricity DECIMAL(18, 3) DEFAULT 0, -- 谷时段电量(kWh)
    valley_amount DECIMAL(18, 2) DEFAULT 0,     -- 谷时段金额(元)
    deep_valley_electricity DECIMAL(18, 3) DEFAULT 0, -- 深谷时段电量(kWh)
    deep_valley_amount DECIMAL(18, 2) DEFAULT 0, -- 深谷时段金额(元)
    
    remark NVARCHAR(200),                       -- 订单备注
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '充电订单表（合并了时段详情）', 'SCHEMA', 'dbo', 'TABLE', 'orders';
EXEC sp_addextendedproperty 'MS_Description', '订单唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '订单编号', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'order_no';
EXEC sp_addextendedproperty 'MS_Description', '用户ID', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'user_id';
EXEC sp_addextendedproperty 'MS_Description', '充电桩ID', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'pile_id';
EXEC sp_addextendedproperty 'MS_Description', '充电端口ID', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'port_id';
EXEC sp_addextendedproperty 'MS_Description', '订单总金额(元)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'amount';
EXEC sp_addextendedproperty 'MS_Description', '充电开始时间', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'start_time';
EXEC sp_addextendedproperty 'MS_Description', '充电结束时间', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'end_time';
EXEC sp_addextendedproperty 'MS_Description', '耗电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'power_consumption';
EXEC sp_addextendedproperty 'MS_Description', '充电时长(秒)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'charging_time';
EXEC sp_addextendedproperty 'MS_Description', '充电功率(kW)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'power';
EXEC sp_addextendedproperty 'MS_Description', '订单状态：0-创建，1-进行中，2-已完成，3-已取消，4-异常', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'status';
EXEC sp_addextendedproperty 'MS_Description', '支付状态：0-未支付，1-已支付，2-已退款', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'payment_status';
EXEC sp_addextendedproperty 'MS_Description', '充电模式：1-充满自停，2-按金额，3-按时间，4-按电量，5-其他', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'charging_mode';
EXEC sp_addextendedproperty 'MS_Description', '停止原因：0-充满自停，1-时间用完，2-金额用完，3-手动停止，4-电量用完', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'stop_reason';
EXEC sp_addextendedproperty 'MS_Description', '峰时段电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'peak_electricity';
EXEC sp_addextendedproperty 'MS_Description', '峰时段金额(元)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'peak_amount';
EXEC sp_addextendedproperty 'MS_Description', '平时段电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'flat_electricity';
EXEC sp_addextendedproperty 'MS_Description', '平时段金额(元)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'flat_amount';
EXEC sp_addextendedproperty 'MS_Description', '谷时段电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'valley_electricity';
EXEC sp_addextendedproperty 'MS_Description', '谷时段金额(元)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'valley_amount';
EXEC sp_addextendedproperty 'MS_Description', '尖峰时段电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'sharp_electricity';
EXEC sp_addextendedproperty 'MS_Description', '尖峰时段金额(元)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'sharp_amount';
EXEC sp_addextendedproperty 'MS_Description', '深谷时段电量(kWh)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'deep_valley_electricity';
EXEC sp_addextendedproperty 'MS_Description', '深谷时段金额(元)', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'deep_valley_amount';
EXEC sp_addextendedproperty 'MS_Description', '订单备注', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'remark';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'update_time';
GO

-- 费率配置表
CREATE TABLE rate_configs (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 费率配置唯一标识符
    time_period NVARCHAR(20) NOT NULL,          -- 时段名称：尖峰、峰、平、谷
    start_time TIME NOT NULL,                   -- 开始时间
    end_time TIME NOT NULL,                     -- 结束时间
    rate DECIMAL(18, 4) NOT NULL,               -- 费率(元/kWh)
    description NVARCHAR(100),                  -- 费率描述
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '费率配置表', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs';
EXEC sp_addextendedproperty 'MS_Description', '费率配置唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '时段名称：尖峰、峰、平、谷', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs', 'COLUMN', 'time_period';
EXEC sp_addextendedproperty 'MS_Description', '开始时间', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs', 'COLUMN', 'start_time';
EXEC sp_addextendedproperty 'MS_Description', '结束时间', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs', 'COLUMN', 'end_time';
EXEC sp_addextendedproperty 'MS_Description', '费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs', 'COLUMN', 'rate';
EXEC sp_addextendedproperty 'MS_Description', '费率描述', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs', 'COLUMN', 'description';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'rate_configs', 'COLUMN', 'update_time';
GO

-- 命令历史记录表
CREATE TABLE command_histories (
    id NVARCHAR(36) PRIMARY KEY,                -- 命令记录唯一标识符
    pile_id NVARCHAR(36) NOT NULL,              -- 充电桩ID
    pile_no NVARCHAR(50) NOT NULL,              -- 充电桩编号
    user_id INT NOT NULL,                       -- 操作用户ID
    command_type SMALLINT,                      -- 命令类型：1-启动，2-停止，3-查询，4-配置
    command_name NVARCHAR(100) NOT NULL,        -- 命令名称
    command_content NVARCHAR(MAX) NOT NULL,     -- 命令内容
    result SMALLINT NOT NULL,                   -- 执行结果：0-失败，1-成功
    result_description NVARCHAR(200) NOT NULL,  -- 结果描述
    send_time DATETIME NOT NULL,                -- 发送时间
    response_time DATETIME,                     -- 响应时间
    update_time DATETIME                        -- 最后更新时间
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '命令历史记录表', 'SCHEMA', 'dbo', 'TABLE', 'command_histories';
EXEC sp_addextendedproperty 'MS_Description', '命令记录唯一标识符', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '充电桩ID', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'pile_id';
EXEC sp_addextendedproperty 'MS_Description', '充电桩编号', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'pile_no';
EXEC sp_addextendedproperty 'MS_Description', '操作用户ID', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'user_id';
EXEC sp_addextendedproperty 'MS_Description', '命令类型：1-启动，2-停止，3-查询，4-配置', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'command_type';
EXEC sp_addextendedproperty 'MS_Description', '命令名称', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'command_name';
EXEC sp_addextendedproperty 'MS_Description', '命令内容', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'command_content';
EXEC sp_addextendedproperty 'MS_Description', '执行结果：0-失败，1-成功', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'result';
EXEC sp_addextendedproperty 'MS_Description', '结果描述', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'result_description';
EXEC sp_addextendedproperty 'MS_Description', '发送时间', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'send_time';
EXEC sp_addextendedproperty 'MS_Description', '响应时间', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'response_time';
EXEC sp_addextendedproperty 'MS_Description', '最后更新时间', 'SCHEMA', 'dbo', 'TABLE', 'command_histories', 'COLUMN', 'update_time';
GO

--
-- 外键约束
--

-- 充电桩外键约束
ALTER TABLE charging_piles
    ADD CONSTRAINT fk_charging_piles_station FOREIGN KEY (station_id) REFERENCES charging_stations(id) ON DELETE CASCADE;

-- 充电端口外键约束
ALTER TABLE charging_ports
    ADD CONSTRAINT fk_charging_ports_pile FOREIGN KEY (pile_id) REFERENCES charging_piles(id) ON DELETE CASCADE;

-- 订单外键约束
ALTER TABLE orders
    ADD CONSTRAINT fk_orders_user FOREIGN KEY (user_id) REFERENCES users(id);

ALTER TABLE orders
    ADD CONSTRAINT fk_orders_pile FOREIGN KEY (pile_id) REFERENCES charging_piles(id);

ALTER TABLE orders
    ADD CONSTRAINT fk_orders_port FOREIGN KEY (port_id) REFERENCES charging_ports(id);

-- 用户卡外键约束
ALTER TABLE user_cards
    ADD CONSTRAINT fk_user_cards_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

-- 命令历史记录外键约束
ALTER TABLE command_histories
    ADD CONSTRAINT fk_command_histories_pile FOREIGN KEY (pile_id) REFERENCES charging_piles(id);

ALTER TABLE command_histories
    ADD CONSTRAINT fk_command_histories_user FOREIGN KEY (user_id) REFERENCES users(id);

--
-- 索引
--

-- 充电桩索引
CREATE INDEX idx_charging_piles_pile_no ON charging_piles(pile_no);
CREATE INDEX idx_charging_piles_imei ON charging_piles(imei);
CREATE INDEX idx_charging_piles_station_id ON charging_piles(station_id);

-- 充电端口索引
CREATE INDEX idx_charging_ports_pile_id ON charging_ports(pile_id);
CREATE INDEX idx_charging_ports_port_no ON charging_ports(port_no);
CREATE INDEX idx_charging_ports_status ON charging_ports(status);

-- 用户索引
CREATE UNIQUE INDEX idx_users_open_id ON users(open_id);

-- 用户卡索引
CREATE INDEX idx_user_cards_user_id ON user_cards(user_id);
CREATE INDEX idx_user_cards_card_no ON user_cards(card_no);

-- 订单索引
CREATE INDEX idx_orders_user_id ON orders(user_id);
CREATE INDEX idx_orders_pile_id ON orders(pile_id);
CREATE INDEX idx_orders_port_id ON orders(port_id);
CREATE INDEX idx_orders_order_no ON orders(order_no);
CREATE INDEX idx_orders_start_time ON orders(start_time);
CREATE INDEX idx_orders_status ON orders(status);

--
-- 初始数据（示例费率配置）
--
INSERT INTO rate_configs (time_period, start_time, end_time, rate, description, update_time)
VALUES 
('peak', '08:00:00', '22:00:00', 1.0593, '峰时段费率', GETDATE()),
('off_peak', '22:00:00', '08:00:00', 0.5283, '谷时段费率', GETDATE());
GO

-- 添加计费模式字段描述
EXEC sp_addextendedproperty 'MS_Description', '计费模式：0-按时间，1-电量+电量服务费，2-电量+时间服务费', 'SCHEMA', 'dbo', 'TABLE', 'orders', 'COLUMN', 'billing_mode';
GO

-- 电量计费配置表
CREATE TABLE electricity_rate_configs (
    id INT PRIMARY KEY IDENTITY(1,1),               -- 配置ID
    pile_id NVARCHAR(36) NOT NULL,                  -- 充电桩ID
    enabled SMALLINT DEFAULT 0,                     -- 启用状态：0-未启用，1-电量+电量服务费，2-电量+时间服务费
    sharp_rate DECIMAL(18, 4) DEFAULT 0,            -- 尖时段电费率(元/kWh)
    sharp_service_rate DECIMAL(18, 4) DEFAULT 0,    -- 尖时段服务费率(元/kWh)
    peak_rate DECIMAL(18, 4) DEFAULT 0,             -- 峰时段电费率(元/kWh)
    peak_service_rate DECIMAL(18, 4) DEFAULT 0,     -- 峰时段服务费率(元/kWh)
    flat_rate DECIMAL(18, 4) DEFAULT 0,             -- 平时段电费率(元/kWh)
    flat_service_rate DECIMAL(18, 4) DEFAULT 0,     -- 平时段服务费率(元/kWh)
    valley_rate DECIMAL(18, 4) DEFAULT 0,           -- 谷时段电费率(元/kWh)
    valley_service_rate DECIMAL(18, 4) DEFAULT 0,   -- 谷时段服务费率(元/kWh)
    deep_valley_rate DECIMAL(18, 4) DEFAULT 0,      -- 深谷时段电费率(元/kWh)
    deep_valley_service_rate DECIMAL(18, 4) DEFAULT 0, -- 深谷时段服务费率(元/kWh)
    update_time DATETIME,                           -- 更新时间
    CONSTRAINT fk_electricity_rate_pile FOREIGN KEY (pile_id) REFERENCES charging_piles(id)
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '电量计费配置表', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs';
EXEC sp_addextendedproperty 'MS_Description', '配置ID', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '充电桩ID', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'pile_id';
EXEC sp_addextendedproperty 'MS_Description', '启用状态：0-未启用，1-电量+电量服务费，2-电量+时间服务费', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'enabled';
EXEC sp_addextendedproperty 'MS_Description', '尖时段电费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'sharp_rate';
EXEC sp_addextendedproperty 'MS_Description', '尖时段服务费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'sharp_service_rate';
EXEC sp_addextendedproperty 'MS_Description', '峰时段电费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'peak_rate';
EXEC sp_addextendedproperty 'MS_Description', '峰时段服务费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'peak_service_rate';
EXEC sp_addextendedproperty 'MS_Description', '平时段电费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'flat_rate';
EXEC sp_addextendedproperty 'MS_Description', '平时段服务费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'flat_service_rate';
EXEC sp_addextendedproperty 'MS_Description', '谷时段电费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'valley_rate';
EXEC sp_addextendedproperty 'MS_Description', '谷时段服务费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'valley_service_rate';
EXEC sp_addextendedproperty 'MS_Description', '深谷时段电费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'deep_valley_rate';
EXEC sp_addextendedproperty 'MS_Description', '深谷时段服务费率(元/kWh)', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'deep_valley_service_rate';
EXEC sp_addextendedproperty 'MS_Description', '更新时间', 'SCHEMA', 'dbo', 'TABLE', 'electricity_rate_configs', 'COLUMN', 'update_time';
GO

-- 时段电价类型配置表
CREATE TABLE time_period_configs (
    id INT PRIMARY KEY IDENTITY(1,1),               -- 配置ID
    pile_id NVARCHAR(36) NOT NULL,                  -- 充电桩ID
    period_index SMALLINT NOT NULL,                 -- 时段索引(0-47，对应48个半小时段)
    rate_type SMALLINT NOT NULL,                    -- 电价类型：0-尖，1-峰，2-平，3-谷，4-深谷
    update_time DATETIME,                           -- 更新时间
    CONSTRAINT fk_time_period_pile FOREIGN KEY (pile_id) REFERENCES charging_piles(id)
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '时段电价类型配置表', 'SCHEMA', 'dbo', 'TABLE', 'time_period_configs';
EXEC sp_addextendedproperty 'MS_Description', '配置ID', 'SCHEMA', 'dbo', 'TABLE', 'time_period_configs', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '充电桩ID', 'SCHEMA', 'dbo', 'TABLE', 'time_period_configs', 'COLUMN', 'pile_id';
EXEC sp_addextendedproperty 'MS_Description', '时段索引(0-47，对应48个半小时段)', 'SCHEMA', 'dbo', 'TABLE', 'time_period_configs', 'COLUMN', 'period_index';
EXEC sp_addextendedproperty 'MS_Description', '电价类型：0-尖，1-峰，2-平，3-谷，4-深谷', 'SCHEMA', 'dbo', 'TABLE', 'time_period_configs', 'COLUMN', 'rate_type';
EXEC sp_addextendedproperty 'MS_Description', '更新时间', 'SCHEMA', 'dbo', 'TABLE', 'time_period_configs', 'COLUMN', 'update_time';
GO

-- 功率档位表
CREATE TABLE power_level_configs (
    id INT PRIMARY KEY IDENTITY(1,1),               -- 配置ID
    pile_id NVARCHAR(36) NOT NULL,                  -- 充电桩ID
    level_no SMALLINT NOT NULL,                     -- 档位序号(1-5)
    min_power DECIMAL(18, 2) NOT NULL,              -- 最小功率(W)
    max_power DECIMAL(18, 2) NOT NULL,              -- 最大功率(W)
    rate DECIMAL(18, 4) NOT NULL,                   -- 单价(元/小时)
    update_time DATETIME,                           -- 更新时间
    CONSTRAINT fk_power_level_pile FOREIGN KEY (pile_id) REFERENCES charging_piles(id)
);
GO

-- 添加表和列说明
EXEC sp_addextendedproperty 'MS_Description', '功率档位表', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs';
EXEC sp_addextendedproperty 'MS_Description', '配置ID', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs', 'COLUMN', 'id';
EXEC sp_addextendedproperty 'MS_Description', '充电桩ID', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs', 'COLUMN', 'pile_id';
EXEC sp_addextendedproperty 'MS_Description', '档位序号(1-5)', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs', 'COLUMN', 'level_no';
EXEC sp_addextendedproperty 'MS_Description', '最小功率(W)', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs', 'COLUMN', 'min_power';
EXEC sp_addextendedproperty 'MS_Description', '最大功率(W)', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs', 'COLUMN', 'max_power';
EXEC sp_addextendedproperty 'MS_Description', '单价(元/小时)', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs', 'COLUMN', 'rate';
EXEC sp_addextendedproperty 'MS_Description', '更新时间', 'SCHEMA', 'dbo', 'TABLE', 'power_level_configs', 'COLUMN', 'update_time';
GO

PRINT '数据库 ChargingPileDB 创建成功！'
GO
