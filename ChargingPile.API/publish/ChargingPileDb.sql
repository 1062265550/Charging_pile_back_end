-- ChargingPileDB 数据库导出
-- 导出时间：2025-03-25
-- 数据库版本：PostgreSQL 17

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;

--
-- 创建扩展
--
CREATE EXTENSION IF NOT EXISTS postgis;

--
-- 表结构
--

-- charging_stations 表
CREATE TABLE public.charging_stations (
    id character varying(36) NOT NULL,
    name character varying(100) NOT NULL,
    status integer NOT NULL,
    address character varying(200),
    location geometry NOT NULL,
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone
);

-- charging_piles 表
CREATE TABLE public.charging_piles (
    id character varying(36) NOT NULL,
    station_id character varying(36) NOT NULL,
    pile_no character varying(50) NOT NULL,
    pile_type integer NOT NULL,
    status integer NOT NULL,
    power_rate numeric NOT NULL,
    manufacturer character varying(100),
    model_number character varying(50),
    imei character varying(20),
    installation_date date,
    last_maintenance_date date,
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone
);

-- charging_pile_configs 表
CREATE TABLE public.charging_pile_configs (
    pile_id character varying(36) NOT NULL,
    protocol_version character varying(50),
    software_version character varying(50),
    hardware_version character varying(50),
    ccid character varying(50),
    voltage character varying(20),
    current character varying(20),
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone
);

-- charging_pile_status 表
CREATE TABLE public.charging_pile_status (
    pile_id character varying(36) NOT NULL,
    online_status smallint NOT NULL,
    signal_strength integer,
    temperature numeric,
    last_heartbeat_time timestamp without time zone,
    update_time timestamp without time zone NOT NULL
);

-- charging_ports 表
CREATE TABLE public.charging_ports (
    id character varying(36) NOT NULL,
    pile_id character varying(36) NOT NULL,
    port_no character varying(50) NOT NULL,
    port_type integer,
    status integer,
    current_order_id character varying(36),
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone
);

-- charging_port_status 表
CREATE TABLE public.charging_port_status (
    port_id character varying(36) NOT NULL,
    voltage numeric,
    current numeric,
    power numeric,
    temperature numeric,
    electricity numeric,
    update_time timestamp without time zone NOT NULL
);

-- charging_port_statistics 表
CREATE TABLE public.charging_port_statistics (
    port_id character varying(36) NOT NULL,
    total_charging_times integer,
    total_charging_duration integer,
    total_power_consumption numeric,
    last_check_time timestamp without time zone,
    last_order_id character varying(36),
    update_time timestamp without time zone NOT NULL
);

-- users 表
CREATE TABLE public.users (
    id integer NOT NULL,
    open_id character varying(100) NOT NULL,
    nickname character varying(50),
    avatar character varying(200),
    password character varying(100),
    balance numeric NOT NULL,
    points integer NOT NULL,
    create_time timestamp without time zone NOT NULL,
    last_login_time timestamp without time zone,
    update_time timestamp without time zone
);

-- user_profiles 表
CREATE TABLE public.user_profiles (
    user_id integer NOT NULL,
    gender integer NOT NULL,
    country character varying(50),
    province character varying(50),
    city character varying(50),
    language character varying(20),
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone
);

-- user_cards 表
CREATE TABLE public.user_cards (
    id integer NOT NULL,
    user_id integer NOT NULL,
    card_no character varying(50) NOT NULL,
    status smallint NOT NULL,
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone
);

-- orders 表
CREATE TABLE public.orders (
    id character varying(36) NOT NULL,
    order_no character varying(36) NOT NULL,
    user_id integer NOT NULL,
    station_id character varying(36) NOT NULL,
    pile_id character varying(36) NOT NULL,
    port_id character varying(36) NOT NULL,
    amount numeric NOT NULL,
    start_time timestamp without time zone NOT NULL,
    end_time timestamp without time zone,
    power_consumption numeric NOT NULL,
    charging_time integer NOT NULL,
    power numeric NOT NULL,
    status integer NOT NULL,
    payment_status integer NOT NULL,
    charging_mode integer NOT NULL,
    stop_reason smallint,
    remark character varying(200),
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone
);

-- order_time_period_details 表
CREATE TABLE public.order_time_period_details (
    id character varying(36) NOT NULL,
    order_id character varying(36) NOT NULL,
    time_period character varying(20) NOT NULL,
    electricity numeric NOT NULL,
    amount numeric NOT NULL,
    create_time timestamp without time zone NOT NULL
);

-- rate_configs 表
CREATE TABLE public.rate_configs (
    id integer NOT NULL,
    time_period character varying(20) NOT NULL,
    start_time time without time zone NOT NULL,
    end_time time without time zone NOT NULL,
    rate numeric NOT NULL,
    description character varying(100),
    create_time timestamp without time zone NOT NULL,
    update_time timestamp without time zone NOT NULL
);

-- command_histories 表
CREATE TABLE public.command_histories (
    id character varying(36) NOT NULL,
    pile_id character varying(36) NOT NULL,
    pile_no character varying(50) NOT NULL,
    user_id integer NOT NULL,
    command_type smallint,
    command_name character varying(100) NOT NULL,
    command_content text NOT NULL,
    result smallint NOT NULL,
    result_description character varying(200) NOT NULL,
    send_time timestamp without time zone NOT NULL,
    response_time timestamp without time zone,
    create_time timestamp without time zone NOT NULL
);

-- spatial_ref_sys 表 (PostGIS必需)
CREATE TABLE IF NOT EXISTS public.spatial_ref_sys (
    srid integer NOT NULL,
    auth_name character varying(256),
    auth_srid integer,
    srtext character varying(2048),
    proj4text character varying(2048)
);

--
-- 主键约束
--
ALTER TABLE ONLY public.charging_stations ADD CONSTRAINT charging_stations_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.charging_piles ADD CONSTRAINT charging_piles_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.charging_pile_configs ADD CONSTRAINT charging_pile_configs_pkey PRIMARY KEY (pile_id);
ALTER TABLE ONLY public.charging_pile_status ADD CONSTRAINT charging_pile_status_pkey PRIMARY KEY (pile_id);
ALTER TABLE ONLY public.charging_ports ADD CONSTRAINT charging_ports_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.charging_port_status ADD CONSTRAINT charging_port_status_pkey PRIMARY KEY (port_id);
ALTER TABLE ONLY public.charging_port_statistics ADD CONSTRAINT charging_port_statistics_pkey PRIMARY KEY (port_id);
ALTER TABLE ONLY public.users ADD CONSTRAINT users_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.user_profiles ADD CONSTRAINT user_profiles_pkey PRIMARY KEY (user_id);
ALTER TABLE ONLY public.user_cards ADD CONSTRAINT user_cards_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.orders ADD CONSTRAINT orders_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.order_time_period_details ADD CONSTRAINT order_time_period_details_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.rate_configs ADD CONSTRAINT rate_configs_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.command_histories ADD CONSTRAINT command_histories_pkey PRIMARY KEY (id);
ALTER TABLE ONLY public.spatial_ref_sys ADD CONSTRAINT spatial_ref_sys_pkey PRIMARY KEY (srid);

--
-- 唯一约束
--
ALTER TABLE ONLY public.charging_piles ADD CONSTRAINT charging_piles_pile_no_key UNIQUE (pile_no);
ALTER TABLE ONLY public.charging_ports ADD CONSTRAINT charging_ports_port_no_key UNIQUE (port_no);
ALTER TABLE ONLY public.users ADD CONSTRAINT users_open_id_key UNIQUE (open_id);
ALTER TABLE ONLY public.user_cards ADD CONSTRAINT user_cards_card_no_key UNIQUE (card_no);
ALTER TABLE ONLY public.orders ADD CONSTRAINT orders_order_no_key UNIQUE (order_no);
ALTER TABLE ONLY public.rate_configs ADD CONSTRAINT rate_configs_time_period_key UNIQUE (time_period);

--
-- 外键约束
--
ALTER TABLE ONLY public.charging_piles
    ADD CONSTRAINT charging_piles_ibfk_1 FOREIGN KEY (station_id) REFERENCES public.charging_stations(id);

ALTER TABLE ONLY public.charging_pile_configs
    ADD CONSTRAINT fk_pile_configs_pile FOREIGN KEY (pile_id) REFERENCES public.charging_piles(id);

ALTER TABLE ONLY public.charging_pile_status
    ADD CONSTRAINT fk_pile_status_pile FOREIGN KEY (pile_id) REFERENCES public.charging_piles(id);

ALTER TABLE ONLY public.charging_ports
    ADD CONSTRAINT charging_ports_ibfk_1 FOREIGN KEY (pile_id) REFERENCES public.charging_piles(id);

ALTER TABLE ONLY public.charging_ports
    ADD CONSTRAINT charging_ports_ibfk_2 FOREIGN KEY (current_order_id) REFERENCES public.orders(id);

ALTER TABLE ONLY public.charging_port_status
    ADD CONSTRAINT fk_port_status_port FOREIGN KEY (port_id) REFERENCES public.charging_ports(id);

ALTER TABLE ONLY public.charging_port_statistics
    ADD CONSTRAINT fk_port_statistics_port FOREIGN KEY (port_id) REFERENCES public.charging_ports(id);

ALTER TABLE ONLY public.charging_port_statistics
    ADD CONSTRAINT fk_port_statistics_order FOREIGN KEY (last_order_id) REFERENCES public.orders(id);

ALTER TABLE ONLY public.user_profiles
    ADD CONSTRAINT fk_user_profiles_user FOREIGN KEY (user_id) REFERENCES public.users(id);

ALTER TABLE ONLY public.user_cards
    ADD CONSTRAINT fk_user_cards_user FOREIGN KEY (user_id) REFERENCES public.users(id);

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_ibfk_1 FOREIGN KEY (user_id) REFERENCES public.users(id);

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_ibfk_2 FOREIGN KEY (station_id) REFERENCES public.charging_stations(id);

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_ibfk_3 FOREIGN KEY (pile_id) REFERENCES public.charging_piles(id);

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_ibfk_4 FOREIGN KEY (port_id) REFERENCES public.charging_ports(id);

ALTER TABLE ONLY public.order_time_period_details
    ADD CONSTRAINT fk_time_period_details_order FOREIGN KEY (order_id) REFERENCES public.orders(id);

ALTER TABLE ONLY public.command_histories
    ADD CONSTRAINT command_histories_ibfk_1 FOREIGN KEY (pile_id) REFERENCES public.charging_piles(id);

ALTER TABLE ONLY public.command_histories
    ADD CONSTRAINT command_histories_ibfk_2 FOREIGN KEY (user_id) REFERENCES public.users(id);

--
-- 索引
--
CREATE INDEX spatial_location ON public.charging_stations USING gist (location);

--
-- 序列
--
-- 用于用户ID自增
CREATE SEQUENCE IF NOT EXISTS public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;
ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);

-- 用于用户卡ID自增
CREATE SEQUENCE IF NOT EXISTS public.user_cards_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.user_cards_id_seq OWNED BY public.user_cards.id;
ALTER TABLE ONLY public.user_cards ALTER COLUMN id SET DEFAULT nextval('public.user_cards_id_seq'::regclass);

-- 用于费率配置ID自增
CREATE SEQUENCE IF NOT EXISTS public.rate_configs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
ALTER SEQUENCE public.rate_configs_id_seq OWNED BY public.rate_configs.id;
ALTER TABLE ONLY public.rate_configs ALTER COLUMN id SET DEFAULT nextval('public.rate_configs_id_seq'::regclass);

--
-- 表数据
--

-- rate_configs 表数据
INSERT INTO public.rate_configs (id, time_period, start_time, end_time, rate, description, create_time, update_time) VALUES
(1, 'peak', '08:00:00', '22:00:00', 1.0593, '峰时段费率', '2025-03-24 18:40:44.919', '2025-03-24 18:40:44.919'),
(2, 'off_peak', '22:00:00', '08:00:00', 0.5283, '谷时段费率', '2025-03-24 18:40:44.919', '2025-03-24 18:40:44.919');

-- 注: spatial_ref_sys 表包含约8500条PostGIS内置数据，由于数据量较大，此处未包含完整数据 