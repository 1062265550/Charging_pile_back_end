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
    update_time DATETIME,                       -- 最后更新时间
    description nvarchar(500) NULL              -- 充电桩描述
);
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
    description NVARCHAR(500) NULL,             -- 充电桩描述
    update_time DATETIME                        -- 最后更新时间
);
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
    current_order_id NVARCHAR(36),              -- 当前订单ID

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
    power_consumption DECIMAL(18, 3) NOT NULL,  -- 耗电量(kWh)  //
    charging_time INT NOT NULL,                 -- 充电时长(秒)
    power DECIMAL(18, 2) NOT NULL,              -- 充电功率(kW)  //
    status INT NOT NULL,                        -- 订单状态：0-创建，1-进行中，2-已完成，3-已取消，4-异常
    payment_status INT NOT NULL,                -- 支付状态：0-未支付，1-已支付，2-已退款
    payment_method INT,                         -- 支付方式：1-微信，2-支付宝，3-余额
    payment_time DATETIME,                      -- 支付时间
    service_fee DECIMAL(18, 2),                 -- 服务费(元)
    total_amount DECIMAL(18, 2),                -- 总金额(元)
    transaction_id NVARCHAR(100),               -- 交易流水号
    charging_mode INT NOT NULL,                 -- 充电模式：1-充满自停，2-按金额，3-按时间，4-按电量，5-其他
    stop_reason SMALLINT,                       -- 停止原因：0-充满自停，1-时间用完，2-金额用完，3-手动停止，4-电量用完
    billing_mode SMALLINT DEFAULT 0,            -- 计费模式：0-按时间，1-电量+电量服务费，2-电量+时间服务费

    -- 以下是从order_time_period_details合并的字段
    sharp_electricity DECIMAL(18, 3) DEFAULT 0, -- 尖峰时段电量(kWh)    //
    sharp_amount DECIMAL(18, 2) DEFAULT 0,      -- 尖峰时段金额(元)     //
    peak_electricity DECIMAL(18, 3) DEFAULT 0,  -- 峰时段电量(kWh)     //
    peak_amount DECIMAL(18, 2) DEFAULT 0,       -- 峰时段金额(元)       //
    flat_electricity DECIMAL(18, 3) DEFAULT 0,  -- 平时段电量(kWh)     //
    flat_amount DECIMAL(18, 2) DEFAULT 0,       -- 平时段金额(元)       //
    valley_electricity DECIMAL(18, 3) DEFAULT 0, -- 谷时段电量(kWh)     //
    valley_amount DECIMAL(18, 2) DEFAULT 0,     -- 谷时段金额(元)       //
    deep_valley_electricity DECIMAL(18, 3) DEFAULT 0, -- 深谷时段电量(kWh) //
    deep_valley_amount DECIMAL(18, 2) DEFAULT 0, -- 深谷时段金额(元)     //

    remark NVARCHAR(200),                       -- 订单备注
    update_time DATETIME                        -- 最后更新时间
);
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

-- 支付记录表
CREATE TABLE payment_records (
    id NVARCHAR(36) PRIMARY KEY,                -- 支付记录唯一标识符
    order_id NVARCHAR(36) NOT NULL,             -- 订单ID
    user_id INT NOT NULL,                       -- 用户ID
    payment_method INT NOT NULL,                -- 支付方式：1-微信，2-支付宝，3-余额
    amount DECIMAL(18, 2) NOT NULL,             -- 支付金额
    payment_status INT NOT NULL,                -- 支付状态：0-未支付，1-支付中，2-支付成功，3-支付失败，4-已退款
    payment_time DATETIME,                      -- 支付时间
    transaction_id NVARCHAR(100),               -- 交易流水号
    js_api_params NVARCHAR(1000),               -- JSAPI支付参数（JSON格式）
    create_time DATETIME NOT NULL,              -- 创建时间
    update_time DATETIME NOT NULL,              -- 更新时间
    CONSTRAINT fk_payment_records_user FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT fk_payment_records_order FOREIGN KEY (order_id) REFERENCES orders(id)
);

-- 创建索引
CREATE INDEX idx_payment_records_order_id ON payment_records(order_id);
CREATE INDEX idx_payment_records_user_id ON payment_records(user_id);
CREATE INDEX idx_payment_records_payment_status ON payment_records(payment_status);
CREATE INDEX idx_payment_records_create_time ON payment_records(create_time);

-- 充值记录表
CREATE TABLE recharge_records (
    id NVARCHAR(36) PRIMARY KEY,                -- 充值记录唯一标识符
    user_id INT NOT NULL,                       -- 用户ID
    amount DECIMAL(18, 2) NOT NULL,             -- 充值金额
    payment_method INT NOT NULL,                -- 支付方式：1-微信，2-支付宝
    status INT NOT NULL,                        -- 状态：0-创建，1-支付中，2-成功，3-失败
    transaction_id NVARCHAR(100),               -- 交易流水号
    js_api_params NVARCHAR(1000),               -- JSAPI支付参数（JSON格式）
    complete_time DATETIME,                     -- 完成时间
    create_time DATETIME NOT NULL,              -- 创建时间
    update_time DATETIME NOT NULL,              -- 更新时间
    CONSTRAINT fk_recharge_records_user FOREIGN KEY (user_id) REFERENCES users(id)
);

-- 创建索引
CREATE INDEX idx_recharge_records_user_id ON recharge_records(user_id);
CREATE INDEX idx_recharge_records_status ON recharge_records(status);
CREATE INDEX idx_recharge_records_create_time ON recharge_records(create_time);

-- 余额变动记录表
CREATE TABLE balance_records (
    id INT PRIMARY KEY IDENTITY(1,1),           -- 记录ID
    user_id INT NOT NULL,                       -- 用户ID
    amount DECIMAL(18, 2) NOT NULL,             -- 变动金额（正数表示收入，负数表示支出）
    balance DECIMAL(18, 2) NOT NULL,            -- 变动后余额
    type INT NOT NULL,                          -- 类型：1-充值，2-订单支付，3-退款，4-其他
    related_id NVARCHAR(36),                    -- 关联ID（订单ID或充值ID）
    description NVARCHAR(200),                  -- 描述
    create_time DATETIME NOT NULL,              -- 创建时间
    CONSTRAINT fk_balance_records_user FOREIGN KEY (user_id) REFERENCES users(id)
);

-- 创建索引
CREATE INDEX idx_balance_records_user_id ON balance_records(user_id);
CREATE INDEX idx_balance_records_type ON balance_records(type);
CREATE INDEX idx_balance_records_create_time ON balance_records(create_time);

PRINT '数据库 ChargingPileDB 创建成功！'
GO