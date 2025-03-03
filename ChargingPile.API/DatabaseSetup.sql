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

-- 创建订单表
CREATE TABLE orders (
    id INT AUTO_INCREMENT PRIMARY KEY, -- 订单ID
    user_id INT NOT NULL, -- 用户ID
    station_id VARCHAR(36) NOT NULL, -- 充电站ID (GUID)
    start_time DATETIME NOT NULL, -- 开始时间
    end_time DATETIME NULL, -- 结束时间
    amount DECIMAL(10,2) NOT NULL DEFAULT 0.00, -- 金额
    status INT NOT NULL DEFAULT 0 COMMENT '0: 进行中, 1: 已完成, 2: 已取消', -- 订单状态
    payment_status INT NOT NULL DEFAULT 0 COMMENT '0: 未支付, 1: 已支付, 2: 退款中, 3: 已退款', -- 支付状态
    create_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 创建时间
    update_time DATETIME NULL, -- 更新时间
    INDEX idx_user_status (user_id, status), -- 用户状态索引
    FOREIGN KEY (user_id) REFERENCES users(id), -- 用户外键
    FOREIGN KEY (station_id) REFERENCES charging_stations(id) -- 充电站外键
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci; 