/**
 * 配置文件
 * 包含API接口地址和全局设置
 */

// API基础URL
const API_BASE_URL = 'http://localhost:5045';

// API端点
const API = {
    // 充电站相关接口
    stations: {
        getAll: `${API_BASE_URL}/api/ChargingStations`,
        getById: (id) => `${API_BASE_URL}/api/ChargingStations/${id}`,
        getPaged: `${API_BASE_URL}/api/ChargingStations/paged`,
        create: `${API_BASE_URL}/api/ChargingStations`,
        update: (id) => `${API_BASE_URL}/api/ChargingStations/${id}`,
        delete: (id) => `${API_BASE_URL}/api/ChargingStations/${id}`
    },
    
    // 充电桩相关接口
    piles: {
        getAll: `${API_BASE_URL}/api/ChargingPiles`,
        getById: (id) => `${API_BASE_URL}/api/ChargingPiles/${id}`,
        getByStation: (stationId) => `${API_BASE_URL}/api/ChargingPiles/byStation/${stationId}`,
        getPaged: `${API_BASE_URL}/api/ChargingPiles/paged`,
        create: `${API_BASE_URL}/api/ChargingPiles`,
        update: (id) => `${API_BASE_URL}/api/ChargingPiles/${id}`,
        delete: (id) => `${API_BASE_URL}/api/ChargingPiles/${id}`
    },
    
    // 充电端口相关接口
    ports: {
        getAll: `${API_BASE_URL}/api/ChargingPorts`,
        getById: (id) => `${API_BASE_URL}/api/ChargingPorts/${id}`,
        getByPile: (pileId) => `${API_BASE_URL}/api/ChargingPorts/by-pile/${pileId}`,
        getPaged: `${API_BASE_URL}/api/ChargingPorts/paged`,
        create: `${API_BASE_URL}/api/ChargingPorts`,
        update: (id) => `${API_BASE_URL}/api/ChargingPorts/${id}`,
        delete: (id) => `${API_BASE_URL}/api/ChargingPorts/${id}`
    },
    
    // 订单相关接口
    orders: {
        getAll: `${API_BASE_URL}/api/Orders`,
        getById: (id) => `${API_BASE_URL}/api/Orders/${id}`,
        getPaged: `${API_BASE_URL}/api/Orders/paged`,
        getByUser: (userId) => `${API_BASE_URL}/api/Orders/byUser/${userId}`,
        getByPile: (pileId) => `${API_BASE_URL}/api/Orders/byPile/${pileId}`,
        create: `${API_BASE_URL}/api/Orders`,
        update: (id) => `${API_BASE_URL}/api/Orders/${id}`,
        updateStatus: (id) => `${API_BASE_URL}/api/Orders/${id}/status`,
        pay: (id) => `${API_BASE_URL}/api/Orders/${id}/pay`,
        cancel: (id) => `${API_BASE_URL}/api/Orders/${id}/cancel`,
        complete: (id) => `${API_BASE_URL}/api/Orders/${id}/complete`,
        statistics: `${API_BASE_URL}/api/Orders/statistics`
    },
    
    // 用户相关接口
    users: {
        getAll: `${API_BASE_URL}/api/Users`,
        getById: (id) => `${API_BASE_URL}/api/Users/${id}`,
        getPaged: `${API_BASE_URL}/api/Users/paged`,
        create: `${API_BASE_URL}/api/Users`,
        update: (id) => `${API_BASE_URL}/api/Users/${id}`,
        delete: (id) => `${API_BASE_URL}/api/Users/${id}`,
        login: `${API_BASE_URL}/api/Users/login`,
        changePassword: (id) => `${API_BASE_URL}/api/Users/${id}/password`,
        rechargeBalance: (id) => `${API_BASE_URL}/api/Users/${id}/recharge`
    }
};

// 数据字典
const DICT = {
    // 充电站状态
    stationStatus: {
        0: { text: '离线', class: 'status-offline' },
        1: { text: '在线', class: 'status-online' },
        2: { text: '维护中', class: 'status-maintenance' },
        3: { text: '故障', class: 'status-error' }
    },
    
    // 充电桩类型
    pileType: {
        1: { text: '直流快充', class: 'badge bg-primary' },
        2: { text: '交流慢充', class: 'badge bg-success' }
    },
    
    // 充电桩状态
    pileStatus: {
        0: { text: '离线', class: 'status-offline' },
        1: { text: '空闲', class: 'status-online' },
        2: { text: '使用中', class: 'badge bg-info' },
        3: { text: '故障', class: 'status-error' }
    },
    
    // 充电端口类型
    portType: {
        1: { text: 'Type-1', class: 'badge bg-info' },
        2: { text: 'Type-2', class: 'badge bg-info' },
        3: { text: 'GB/T', class: 'badge bg-info' },
        4: { text: 'CHAdeMO', class: 'badge bg-primary' },
        5: { text: 'CCS', class: 'badge bg-primary' }
    },
    
    // 充电端口状态
    portStatus: {
        0: { text: '离线', class: 'badge bg-secondary' },
        1: { text: '空闲', class: 'badge bg-success' },
        2: { text: '使用中', class: 'badge bg-info' },
        3: { text: '故障', class: 'badge bg-danger' }
    },
    
    // 订单状态
    orderStatus: {
        0: { text: '待支付', class: 'badge bg-warning' },
        1: { text: '充电中', class: 'badge bg-info' },
        2: { text: '已完成', class: 'badge bg-success' },
        3: { text: '已取消', class: 'badge bg-secondary' },
        4: { text: '异常', class: 'badge bg-danger' }
    }
};

// 全局设置
const SETTINGS = {
    pageSize: 10,
    dateTimeFormat: 'YYYY-MM-DD HH:mm:ss',
    dateFormat: 'YYYY-MM-DD',
    colors: {
        primary: '#007aff',
        success: '#34c759',
        info: '#5ac8fa',
        warning: '#ff9500',
        danger: '#ff3b30'
    }
};

// 辅助函数
const UTILS = {
    // 格式化日期时间
    formatDateTime: function(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleString('zh-CN');
    },
    
    // 格式化日期
    formatDate: function(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleDateString('zh-CN');
    },
    
    // 格式化金额
    formatCurrency: function(amount) {
        if (amount === null || amount === undefined) return '-';
        return '¥' + parseFloat(amount).toFixed(2);
    },
    
    // 获取状态显示
    getStatusHtml: function(statusCode, statusDict) {
        const status = statusDict[statusCode];
        if (!status) return `<span class="badge bg-secondary">未知</span>`;
        return `<span class="${status.class}">${status.text}</span>`;
    },
    
    // 显示错误提示
    showError: function(message) {
        alert('错误: ' + message);
        console.error(message);
    }
};
