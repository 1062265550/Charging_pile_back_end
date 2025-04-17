/**
 * 充电桩管理系统 - 数据统计模块
 * 实现数据统计页面的各项功能
 */

// 图表对象
let pileStatusChart;
let portTypeChart;
let userGenderChart;
let orderStatusChart;

// 页面加载完成后执行
$(document).ready(function() {
    // 初始化图表
    initializeCharts();
    
    // 加载统计数据
    loadStatistics();
    
    // 注册刷新按钮事件
    $("#refreshBtn").click(function() {
        loadStatistics();
    });

    // 使用正确的函数，此函数在common.js中定义
    if (typeof loadComponents === 'function') {
        loadComponents();
    }
});

/**
 * 初始化所有图表
 */
function initializeCharts() {
    // 图表配置项
    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false,  // 隐藏Chart.js自带的图例
                position: 'bottom',
                labels: {
                    padding: 20,
                    usePointStyle: true,
                    pointStyle: 'circle'
                }
            }
        },
        cutout: '65%'
    };
    
    // 充电桩状态分布图表
    const pileStatusCtx = document.getElementById('pileStatusChart').getContext('2d');
    pileStatusChart = new Chart(pileStatusCtx, {
        type: 'doughnut',
        data: {
            labels: ['在线', '离线', '故障'],
            datasets: [{
                data: [0, 0, 0],
                backgroundColor: ['#28a745', '#6c757d', '#dc3545'],
                borderWidth: 0
            }]
        },
        options: chartOptions
    });
    
    // 充电口类型分布图表
    const portTypeCtx = document.getElementById('portTypeChart').getContext('2d');
    portTypeChart = new Chart(portTypeCtx, {
        type: 'doughnut',
        data: {
            labels: ['国标', '欧标', '美标'],
            datasets: [{
                data: [0, 0, 0],
                backgroundColor: ['#007bff', '#20c997', '#fd7e14'],
                borderWidth: 0
            }]
        },
        options: chartOptions
    });
    
    // 用户性别分布图表
    const userGenderCtx = document.getElementById('userGenderChart').getContext('2d');
    userGenderChart = new Chart(userGenderCtx, {
        type: 'doughnut',
        data: {
            labels: ['男性', '女性', '未知'],
            datasets: [{
                data: [0, 0, 0],
                backgroundColor: ['#007bff', '#e83e8c', '#6c757d'],
                borderWidth: 0
            }]
        },
        options: chartOptions
    });
    
    // 订单状态分布图表
    const orderStatusCtx = document.getElementById('orderStatusChart').getContext('2d');
    orderStatusChart = new Chart(orderStatusCtx, {
        type: 'doughnut',
        data: {
            labels: ['待支付', '充电中', '已完成', '已取消', '异常'],
            datasets: [{
                data: [0, 0, 0, 0, 0],
                backgroundColor: ['#ffc107', '#17a2b8', '#28a745', '#6c757d', '#dc3545'],
                borderWidth: 0
            }]
        },
        options: chartOptions
    });
}

/**
 * 加载所有统计数据
 */
function loadStatistics() {
    // 显示加载提示
    showLoading();
    
    // 添加调试信息
    console.log('开始加载统计数据...');
    
    // 调用统计数据API
    $.ajax({
        url: '/api/Statistics',
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            // 详细记录返回数据
            console.log('统计数据加载成功:', data);
            
            // 检查数据格式是否符合预期
            if (!data) {
                console.error('API返回数据为空');
                showError('API返回数据为空');
                hideLoading();
                return;
            }
            
            // 检查各部分数据
            if (!data.pileStatistics) console.warn('充电桩统计数据缺失');
            if (!data.portStatistics) console.warn('充电口统计数据缺失');
            if (!data.userStatistics) console.warn('用户统计数据缺失');
            if (!data.orderStatistics) console.warn('订单统计数据缺失');
            
            // 更新页面数据
            updateStatistics(data);
            
            // 隐藏加载提示
            hideLoading();
        },
        error: function(xhr, status, error) {
            console.error('加载统计数据失败:', error);
            console.error('状态码:', xhr.status);
            console.error('响应文本:', xhr.responseText);
            
            // 尝试使用模拟数据
            console.log('尝试使用模拟数据...');
            useMockData();
            
            // 显示错误提示
            showError('加载统计数据失败，已显示模拟数据');
            
            // 隐藏加载提示
            hideLoading();
        }
    });
}

/**
 * 使用模拟数据（当API调用失败时）
 */
function useMockData() {
    // 创建模拟数据
    const mockData = {
        pileStatistics: {
            totalCount: 5,
            onlineCount: 3,
            offlineCount: 1,
            faultCount: 1,
            dcFastCount: 3,
            acSlowCount: 2
        },
        portStatistics: {
            totalCount: 8,
            idleCount: 4,
            inUseCount: 3,
            faultCount: 1,
            gbCount: 4,
            euCount: 2,
            usCount: 2
        },
        userStatistics: {
            totalCount: 10,
            newUsersLast7Days: 3,
            newUsersLast30Days: 7,
            activeUsers: 6,
            maleCount: 6,
            femaleCount: 3,
            unknownGenderCount: 1
        },
        orderStatistics: {
            totalCount: 15,
            todayCount: 2,
            last7DaysCount: 8,
            last30DaysCount: 15,
            completedCount: 10,
            chargingCount: 2,
            pendingPaymentCount: 1,
            cancelledCount: 1,
            abnormalCount: 1,
            totalPowerConsumption: 120.5,
            totalChargingDuration: 360,
            totalAmount: 500.50
        }
    };
    
    // 更新页面数据
    updateStatistics(mockData);
    
    // 添加提示，表明数据为模拟数据
    $('h5:contains("数据统计")').append('<span class="badge bg-warning ms-2">模拟数据</span>');
}

/**
 * 更新页面上的统计数据
 * @param {Object} data 统计数据
 */
function updateStatistics(data) {
    // 更新总览卡片数据
    updateOverviewCards(data);
    
    // 更新充电桩统计
    updatePileStatistics(data.pileStatistics);
    
    // 更新充电口统计
    updatePortStatistics(data.portStatistics);
    
    // 更新用户统计
    updateUserStatistics(data.userStatistics);
    
    // 更新订单统计
    updateOrderStatistics(data.orderStatistics);
}

/**
 * 更新总览卡片数据
 * @param {Object} data 统计数据
 */
function updateOverviewCards(data) {
    // 更新四个主要指标
    $('#pileCount').text(data.pileStatistics.totalCount);
    $('#portCount').text(data.portStatistics.totalCount);
    $('#userCount').text(data.userStatistics.totalCount);
    $('#todayOrderCount').text(data.orderStatistics.todayCount);
}

/**
 * 更新充电桩统计数据
 * @param {Object} pileStats 充电桩统计数据
 */
function updatePileStatistics(pileStats) {
    // 更新数字统计
    $('#onlinePileCount').text(pileStats.onlineCount);
    $('#offlinePileCount').text(pileStats.offlineCount);
    $('#faultPileCount').text(pileStats.faultCount);
    
    // 计算百分比并更新进度条
    const totalPiles = pileStats.totalCount > 0 ? pileStats.totalCount : 1; // 避免除以0
    const onlinePercent = (pileStats.onlineCount / totalPiles * 100).toFixed(0);
    const offlinePercent = (pileStats.offlineCount / totalPiles * 100).toFixed(0);
    const faultPercent = (pileStats.faultCount / totalPiles * 100).toFixed(0);
    
    $('#onlinePileBar').css('width', onlinePercent + '%');
    $('#offlinePileBar').css('width', offlinePercent + '%');
    $('#faultPileBar').css('width', faultPercent + '%');
    
    // 更新图表数据
    updateChart(pileStatusChart, [pileStats.onlineCount, pileStats.offlineCount, pileStats.faultCount]);
}

/**
 * 更新充电口统计数据
 * @param {Object} portStats 充电口统计数据
 */
function updatePortStatistics(portStats) {
    // 更新数字统计
    $('#idlePortCount').text(portStats.idleCount);
    $('#inUsePortCount').text(portStats.inUseCount);
    $('#faultPortCount').text(portStats.faultCount);
    
    // 计算百分比并更新进度条
    const totalPorts = portStats.totalCount > 0 ? portStats.totalCount : 1; // 避免除以0
    const idlePercent = (portStats.idleCount / totalPorts * 100).toFixed(0);
    const inUsePercent = (portStats.inUseCount / totalPorts * 100).toFixed(0);
    const faultPercent = (portStats.faultCount / totalPorts * 100).toFixed(0);
    
    $('#idlePortBar').css('width', idlePercent + '%');
    $('#inUsePortBar').css('width', inUsePercent + '%');
    $('#faultPortBar').css('width', faultPercent + '%');
    
    // 更新图表数据
    updateChart(portTypeChart, [portStats.gbCount, portStats.euCount, portStats.usCount]);
}

/**
 * 更新用户统计数据
 * @param {Object} userStats 用户统计数据
 */
function updateUserStatistics(userStats) {
    // 更新数字统计
    $('#activeUserCount').text(userStats.activeUsers);
    $('#newUser7Count').text(userStats.newUsersLast7Days);
    $('#newUser30Count').text(userStats.newUsersLast30Days);
    
    // 计算百分比并更新进度条
    const totalUsers = userStats.totalCount > 0 ? userStats.totalCount : 1; // 避免除以0
    const activePercent = (userStats.activeUsers / totalUsers * 100).toFixed(0);
    const newUser7Percent = (userStats.newUsersLast7Days / totalUsers * 100).toFixed(0);
    const newUser30Percent = (userStats.newUsersLast30Days / totalUsers * 100).toFixed(0);
    
    $('#activeUserBar').css('width', activePercent + '%');
    $('#newUser7Bar').css('width', newUser7Percent + '%');
    $('#newUser30Bar').css('width', newUser30Percent + '%');
    
    // 更新图表数据
    updateChart(userGenderChart, [userStats.maleCount, userStats.femaleCount, userStats.unknownGenderCount]);
}

/**
 * 更新订单统计数据
 * @param {Object} orderStats 订单统计数据
 */
function updateOrderStatistics(orderStats) {
    // 更新数字统计
    $('#totalPowerConsumption').text(orderStats.totalPowerConsumption.toFixed(2) + ' kWh');
    $('#totalChargingDuration').text(formatDuration(orderStats.totalChargingDuration));
    $('#totalAmount').text('¥' + orderStats.totalAmount.toFixed(2));
    
    // 更新图表数据
    updateChart(orderStatusChart, [
        orderStats.pendingPaymentCount, 
        orderStats.chargingCount, 
        orderStats.completedCount, 
        orderStats.cancelledCount, 
        orderStats.abnormalCount
    ]);
}

/**
 * 更新图表数据
 * @param {Chart} chart 图表对象
 * @param {Array} newData 新的数据数组
 */
function updateChart(chart, newData) {
    chart.data.datasets[0].data = newData;
    chart.update();
}

/**
 * 格式化时长（分钟转为小时和分钟）
 * @param {number} minutes 分钟数
 * @returns {string} 格式化后的时长
 */
function formatDuration(minutes) {
    if (minutes < 60) {
        return minutes + ' 分钟';
    }
    
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    
    if (remainingMinutes === 0) {
        return hours + ' 小时';
    }
    
    return hours + ' 小时 ' + remainingMinutes + ' 分钟';
}

/**
 * 显示加载中提示
 */
function showLoading() {
    // 如果需要，可以添加加载动画
    $("#refreshBtn").html('<i class="fas fa-spinner fa-spin me-1"></i>数据加载中...');
    $("#refreshBtn").prop('disabled', true);
}

/**
 * 隐藏加载中提示
 */
function hideLoading() {
    $("#refreshBtn").html('<i class="fas fa-sync-alt me-1"></i>刷新数据');
    $("#refreshBtn").prop('disabled', false);
}

/**
 * 显示错误提示
 * @param {string} message 错误信息
 */
function showError(message) {
    // 如果页面上没有提示区域，可以使用alert或创建一个提示元素
    alert(message);
}
