/**
 * 控制面板页面脚本
 * 用于首页数据展示和统计
 */

document.addEventListener('DOMContentLoaded', function() {
    // 初始化统计数据和图表
    initDashboard();
});

/**
 * 初始化控制面板
 */
function initDashboard() {
    // 加载统计数据
    loadStatistics();
    
    // 加载最近订单
    loadRecentOrders();
    
    // 初始化充电桩状态图表
    initPileStatusChart();
}

/**
 * 加载统计数据
 */
function loadStatistics() {
    // 获取充电站总数
    fetch(API.stations.getAll)
        .then(response => response.json())
        .then(data => {
            document.getElementById('stationCount').textContent = data.length || 0;
        })
        .catch(error => {
            console.error('获取充电站数据失败:', error);
            document.getElementById('stationCount').textContent = '获取失败';
        });
    
    // 获取充电桩总数
    fetch(API.piles.getAll)
        .then(response => response.json())
        .then(data => {
            document.getElementById('pileCount').textContent = data.length || 0;
        })
        .catch(error => {
            console.error('获取充电桩数据失败:', error);
            document.getElementById('pileCount').textContent = '获取失败';
        });
    
    // 获取用户总数
    fetch(API.users.getAll)
        .then(response => response.json())
        .then(data => {
            document.getElementById('userCount').textContent = data.length || 0;
        })
        .catch(error => {
            console.error('获取用户数据失败:', error);
            document.getElementById('userCount').textContent = '获取失败';
        });
    
    // 获取今日订单数（这里简化处理，实际可能需要后端专门的接口）
    fetch(API.orders.getAll)
        .then(response => response.json())
        .then(data => {
            // 简单筛选当天的订单
            const today = new Date().toISOString().split('T')[0];
            const todayOrders = data.filter(order => {
                return order.createTime && order.createTime.startsWith(today);
            });
            document.getElementById('todayOrderCount').textContent = todayOrders.length || 0;
        })
        .catch(error => {
            console.error('获取订单数据失败:', error);
            document.getElementById('todayOrderCount').textContent = '获取失败';
        });
}

/**
 * 加载最近订单
 */
function loadRecentOrders() {
    // 获取最近的订单
    fetch(API.orders.getAll)
        .then(response => response.json())
        .then(data => {
            // 排序并获取最近的5个订单
            const recentOrders = data
                .sort((a, b) => new Date(b.createTime) - new Date(a.createTime))
                .slice(0, 5);
            
            renderRecentOrdersTable(recentOrders);
        })
        .catch(error => {
            console.error('获取最近订单失败:', error);
            document.getElementById('recentOrdersTable').querySelector('tbody').innerHTML = 
                '<tr><td colspan="5" class="text-center text-danger">获取数据失败</td></tr>';
        });
}

/**
 * 渲染最近订单表格
 */
function renderRecentOrdersTable(orders) {
    const tableBody = document.getElementById('recentOrdersTable').querySelector('tbody');
    
    if (!orders || orders.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center">暂无订单数据</td></tr>';
        return;
    }
    
    let html = '';
    orders.forEach(order => {
        html += `
            <tr>
                <td>${order.orderNo || '-'}</td>
                <td>${order.userNickname || '-'}</td>
                <td>${order.pileNo || '-'}</td>
                <td>${UTILS.getStatusHtml(order.status, DICT.orderStatus)}</td>
                <td>${UTILS.formatDateTime(order.createTime)}</td>
            </tr>
        `;
    });
    
    tableBody.innerHTML = html;
}

/**
 * 初始化充电桩状态图表
 */
function initPileStatusChart() {
    fetch(API.piles.getAll)
        .then(response => response.json())
        .then(data => {
            // 统计不同状态的充电桩数量
            const statusCounts = {
                0: 0, // 离线
                1: 0, // 空闲
                2: 0, // 使用中
                3: 0  // 故障
            };
            
            data.forEach(pile => {
                if (statusCounts[pile.status] !== undefined) {
                    statusCounts[pile.status]++;
                }
            });
            
            // 创建图表
            const ctx = document.getElementById('pileStatusChart').getContext('2d');
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: [
                        DICT.pileStatus[0].text,
                        DICT.pileStatus[1].text,
                        DICT.pileStatus[2].text,
                        DICT.pileStatus[3].text
                    ],
                    datasets: [{
                        data: [
                            statusCounts[0],
                            statusCounts[1],
                            statusCounts[2],
                            statusCounts[3]
                        ],
                        backgroundColor: [
                            '#8e8e93', // 离线
                            '#34c759', // 空闲
                            '#5ac8fa', // 使用中
                            '#ff3b30'  // 故障
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        },
                        title: {
                            display: true,
                            text: '充电桩状态分布'
                        }
                    }
                }
            });
        })
        .catch(error => {
            console.error('获取充电桩状态数据失败:', error);
            document.getElementById('pileStatusChart').parentNode.innerHTML = 
                '<div class="alert alert-danger">获取充电桩状态数据失败</div>';
        });
}
