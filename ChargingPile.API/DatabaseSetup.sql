-- 创建数据库
CREATE DATABASE IF NOT EXISTS charging_pile_db
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE charging_pile_db;

-- 创建充电站表
CREATE TABLE charging_stations (
    id VARCHAR(36) PRIMARY KEY, -- 充电站ID (GUID)
    name VARCHAR(100) NOT NULL, -- 充电站名称
    latitude DECIMAL(10,7) NOT NULL, -- 纬度
    longitude DECIMAL(10,7) NOT NULL, -- 经度
    location POINT SRID 4326, -- 地理位置点 (支持空间索引)
    address VARCHAR(200), -- 地址
    total_ports INT NOT NULL DEFAULT 0, -- 总插座数
    available_ports INT NOT NULL DEFAULT 0, -- 可用插座数
    status INT NOT NULL DEFAULT 0 COMMENT '0: 正常运营, 1: 维护中, 2: 故障', -- 状态
    create_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 创建时间
    update_time DATETIME NULL, -- 更新时间
    INDEX idx_location (latitude, longitude), -- 位置索引
    SPATIAL INDEX spatial_location (location) -- 空间索引
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建用户表
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY, -- 用户ID
    open_id VARCHAR(100) NOT NULL, -- 微信OpenID
    nickname VARCHAR(50), -- 昵称
    avatar VARCHAR(200), -- 头像
    balance DECIMAL(10,2) NOT NULL DEFAULT 0.00, -- 余额
    points INT NOT NULL DEFAULT 0, -- 积分
    create_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 创建时间
    last_login_time DATETIME NULL, -- 最后登录时间
    UNIQUE KEY uk_open_id (open_id) -- OpenID唯一索引
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建充电桩表
CREATE TABLE charging_piles (
    id VARCHAR(36) PRIMARY KEY, -- 充电桩ID (GUID)
    station_id VARCHAR(36) NOT NULL, -- 所属充电站ID
    pile_no VARCHAR(50) NOT NULL, -- 充电桩编号
    pile_type INT NOT NULL COMMENT '0: 快充, 1: 慢充', -- 充电桩类型
    status INT NOT NULL DEFAULT 0 COMMENT '0: 正常, 1: 故障, 2: 维护中', -- 状态
    power_rate DECIMAL(10,2) NOT NULL, -- 额定功率（kW，用于费率计算）
    voltage VARCHAR(20), -- 电压等级
    current VARCHAR(20), -- 电流规格
    manufacturer VARCHAR(100), -- 制造商
    model_number VARCHAR(50), -- 型号
    installation_date DATE, -- 安装日期
    last_maintenance_date DATE, -- 最后维护日期
    create_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 创建时间
    update_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 更新时间
    FOREIGN KEY (station_id) REFERENCES charging_stations(id), -- 充电站外键
    UNIQUE KEY uk_pile_no (pile_no) -- 充电桩编号唯一索引
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建充电口表
CREATE TABLE charging_ports (
    id VARCHAR(36) PRIMARY KEY, -- 充电口ID (GUID)
    pile_id VARCHAR(36) NOT NULL, -- 所属充电桩ID
    port_no VARCHAR(50) NOT NULL, -- 充电口编号
    port_type INT NOT NULL COMMENT '0: 国标, 1: 特斯拉, 2: 其他', -- 接口类型
    status INT NOT NULL DEFAULT 0 COMMENT '0: 空闲, 1: 使用中, 2: 故障, 3: 维护中', -- 状态
    current_order_id INT, -- 当前正在进行的订单ID
    last_order_id INT, -- 最后一个完成的订单ID
    total_charging_times INT NOT NULL DEFAULT 0, -- 总充电次数
    total_charging_duration INT NOT NULL DEFAULT 0, -- 总充电时长（分钟）
    total_power_consumption DECIMAL(10,2) NOT NULL DEFAULT 0, -- 总耗电量（度，用于统计）
    last_check_time DATETIME, -- 最后检查时间
    create_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 创建时间
    update_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 更新时间
    FOREIGN KEY (pile_id) REFERENCES charging_piles(id), -- 充电桩外键
    UNIQUE KEY uk_port_no (port_no) -- 充电口编号唯一索引
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建订单表
CREATE TABLE orders (
    id INT AUTO_INCREMENT PRIMARY KEY, -- 订单ID
    order_no VARCHAR(36) NOT NULL, -- 订单号 (UUID)
    user_id INT NOT NULL, -- 用户ID
    station_id VARCHAR(36) NOT NULL, -- 充电站ID
    port_id VARCHAR(36) NOT NULL, -- 充电口ID   
    start_time DATETIME NOT NULL, -- 开始时间
    end_time DATETIME NULL, -- 结束时间
    amount DECIMAL(10,2) NOT NULL DEFAULT 0.00, -- 订单金额（元，系统自动计算）
    power_consumption DECIMAL(10,2) NOT NULL DEFAULT 0.00, -- 充电量（度，系统自动计算）
    status INT NOT NULL DEFAULT 0 COMMENT '0: 进行中, 1: 已完成, 2: 已取消', -- 订单状态
    payment_status INT NOT NULL DEFAULT 0 COMMENT '0: 未支付, 1: 已支付, 2: 退款中, 3: 已退款', -- 支付状态
    create_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 创建时间
    update_time DATETIME NULL, -- 更新时间
    INDEX idx_user_status (user_id, status), -- 用户状态索引
    UNIQUE KEY uk_order_no (order_no), -- 订单号唯一索引
    FOREIGN KEY (user_id) REFERENCES users(id), -- 用户外键
    FOREIGN KEY (station_id) REFERENCES charging_stations(id), -- 充电站外键
    FOREIGN KEY (port_id) REFERENCES charging_ports(id) -- 充电口外键
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 添加充电口的订单关联
ALTER TABLE charging_ports
ADD FOREIGN KEY (current_order_id) REFERENCES orders(id),
ADD FOREIGN KEY (last_order_id) REFERENCES orders(id);

-- 创建费率配置表
CREATE TABLE rate_configs (
    id INT AUTO_INCREMENT PRIMARY KEY, -- 费率配置ID
    time_period VARCHAR(20) NOT NULL, -- 时间段（例如：'peak', 'off_peak'）
    start_time TIME NOT NULL, -- 开始时间
    end_time TIME NOT NULL, -- 结束时间
    rate DECIMAL(10,4) NOT NULL, -- 费率（元/度）
    description VARCHAR(100), -- 费率描述
    create_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 创建时间
    update_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 更新时间
    UNIQUE KEY uk_time_period (time_period) -- 时间段唯一索引
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 插入默认费率配置
INSERT INTO rate_configs (time_period, start_time, end_time, rate, description) VALUES
('peak', '08:00:00', '22:00:00', 1.0593, '峰时段费率'),
('off_peak', '22:00:00', '08:00:00', 0.5283, '谷时段费率');

-- 为现有的订单表添加订单号字段的SQL语句
-- 注意：此语句仅用于修改已存在的表，初始化数据库时不需要执行
ALTER TABLE orders 
ADD COLUMN order_no VARCHAR(36) NOT NULL AFTER id,
ADD UNIQUE KEY uk_order_no (order_no);

-- 为现有订单生成UUID作为订单号
-- 注意：此语句仅用于为已有数据生成订单号，初始化数据库时不需要执行
UPDATE orders SET order_no = UUID() WHERE order_no IS NULL OR order_no = '';

