/**
 * 控制面板页面脚本
 * 负责加载和显示控制面板的数据和图表
 */

// 在DOM加载完成后执行
document.addEventListener('DOMContentLoaded', function() {
    // 检查Chart.js是否已加载
    if (typeof Chart === 'undefined') {
        console.error('Chart.js未加载，尝试动态加载...');
        loadChartJsLibrary()
            .then(() => {
                console.log('Chart.js加载成功');
                loadDashboardData();
            })
            .catch(error => {
                console.error('Chart.js加载失败:', error);
                document.getElementById('pileStatusChart').parentElement.innerHTML = 
                    '<div class="alert alert-danger">图表库加载失败，请刷新页面重试</div>';
                // 仍然加载其他非图表数据
                loadStationsCount();
                loadPilesCount();
                loadTodayOrdersCount();
                loadUsersCount();
                loadRecentOrders();
            });
    } else {
        // Chart.js已加载，正常初始化
        loadDashboardData();
    }
    
    // 绑定刷新按钮事件
    document.getElementById('refreshDashboard').addEventListener('click', function() {
        // 显示加载状态
        ['stationCount', 'pileCount', 'todayOrderCount', 'userCount'].forEach(id => {
            document.getElementById(id).innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        });
        
        // 清空表格并显示加载中
        document.getElementById('recentOrdersTable').querySelector('tbody').innerHTML = 
            '<tr><td colspan="5" class="text-center">加载中...</td></tr>';
            
        // 重新加载数据
        loadDashboardData();
    });
});

/**
 * 动态加载Chart.js库
 */
function loadChartJsLibrary() {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = '/js/lib/chart.min.js';
        script.onload = () => resolve();
        script.onerror = () => reject(new Error('无法加载Chart.js'));
        document.head.appendChild(script);
    });
}

/**
 * 加载控制面板所有数据
 */
function loadDashboardData() {
    // 并行加载所有数据
    loadStationsCount();
    loadPilesCount();
    loadTodayOrdersCount();
    loadUsersCount();
    loadRecentOrders();
    loadPileStatusChart();
}

/**
 * 加载充电站总数
 */
function loadStationsCount() {
    fetch(API.stations.getAll)
        .then(response => {
            // 检查HTTP状态
            if (!response.ok) {
                throw new Error('服务器返回错误: ' + response.status + ' ' + response.statusText);
            }
            // 尝试解析为JSON，如果失败则使用text()解析
            return response.json().catch(e => {
                return response.text().then(text => {
                    console.error('JSON解析失败，服务器返回的不是有效的JSON:', text);
                    return { error: '数据格式错误', message: text };
                });
            });
        })
        .then(data => {
            // 检查是否返回的是错误信息
            if (data.error) {
                console.error('API返回错误:', data.message);
                document.getElementById('stationCount').textContent = '获取失败';
                return;
            }
            // 正常处理返回的数据
            document.getElementById('stationCount').textContent = data.length || 0;
        })
        .catch(error => {
            console.error('获取充电站数据失败:', error);
            document.getElementById('stationCount').textContent = '获取失败';
        });
}

/**
 * 加载充电桩总数
 */
function loadPilesCount() {
    fetch(API.piles.getAll)
        .then(response => {
            // 检查HTTP状态
            if (!response.ok) {
                throw new Error('服务器返回错误: ' + response.status + ' ' + response.statusText);
            }
            // 尝试解析为JSON，如果失败则使用text()解析
            return response.json().catch(e => {
                return response.text().then(text => {
                    console.error('JSON解析失败，服务器返回的不是有效的JSON:', text);
                    return { error: '数据格式错误', message: text };
                });
            });
        })
        .then(data => {
            // 检查是否返回的是错误信息
            if (data.error) {
                console.error('API返回错误:', data.message);
                document.getElementById('pileCount').textContent = '获取失败';
                return;
            }
            // 正常处理返回的数据
            document.getElementById('pileCount').textContent = data.length || 0;
        })
        .catch(error => {
            console.error('获取充电桩数据失败:', error);
            document.getElementById('pileCount').textContent = '获取失败';
        });
}

/**
 * 加载今日订单数
 */
function loadTodayOrdersCount() {
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
 * 加载用户总数
 */
function loadUsersCount() {
    fetch(API.users.getAll)
        .then(response => response.json())
        .then(data => {
            document.getElementById('userCount').textContent = data.length || 0;
        })
        .catch(error => {
            console.error('获取用户数据失败:', error);
            document.getElementById('userCount').textContent = '获取失败';
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
    
    // 添加数据调试日志
    console.log('订单数据:', orders);
    
    if (!orders || orders.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center">暂无订单数据</td></tr>';
        return;
    }
    
    // 清空表格
    tableBody.innerHTML = '';
    
    // 添加订单数据
    orders.forEach(order => {
        const tr = document.createElement('tr');
        
        // 根据API文档，订单状态码：0=待支付，1=充电中，2=已完成，3=已取消，4=异常
        let statusClass = '';
        let statusText = '';
        
        switch(order.status) {
            case 0:
                statusClass = 'badge bg-warning';
                statusText = '待支付';
                break;
            case 1:
                statusClass = 'badge bg-info';
                statusText = '充电中';
                break;
            case 2:
                statusClass = 'badge bg-success';
                statusText = '已完成';
                break;
            case 3:
                statusClass = 'badge bg-secondary';
                statusText = '已取消';
                break;
            case 4:
                statusClass = 'badge bg-danger';
                statusText = '异常';
                break;
            default:
                statusClass = 'badge bg-secondary';
                statusText = '未知';
        }
        
        // 根据API文档，订单的字段名称应该是orderNo, userNickname, pileNo等
        const orderNo = order.orderNo || order.orderNumber || '-';
        const userName = order.userNickname || order.userName || '-';
        const pileCode = order.pileNo || order.pileCode || '-';
        
        // 格式化时间 - 支持createTime或updateTime
        const timeField = order.createTime || order.updateTime;
        const orderTime = timeField ? new Date(timeField).toLocaleString('zh-CN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        }) : '-';
        
        tr.innerHTML = `
            <td>${orderNo}</td>
            <td>${userName}</td>
            <td>${pileCode}</td>
            <td><span class="${statusClass}">${statusText}</span></td>
            <td>${orderTime}</td>
        `;
        
        tableBody.appendChild(tr);
    });
}

/**
 * 加载充电桩状态图表
 */
function loadPileStatusChart() {
    // 检查Chart.js是否可用
    if (typeof Chart === 'undefined') {
        console.error('Chart.js未加载，无法渲染图表');
        const chartCanvas = document.getElementById('pileStatusChart');
        const ctx = chartCanvas.getContext('2d');
        ctx.font = '14px Arial';
        ctx.fillStyle = '#dc3545';
        ctx.textAlign = 'center';
        ctx.fillText('图表库未加载，请刷新页面', chartCanvas.width / 2, chartCanvas.height / 2);
        
        // 尝试重新加载Chart.js
        loadChartJsLibrary()
            .then(() => {
                console.log('Chart.js动态加载成功，重试渲染图表');
                setTimeout(loadPileStatusChart, 500); // 稍后重试
            })
            .catch(error => {
                console.error('重试加载Chart.js失败:', error);
            });
        return;
    }

    // 清除之前的错误信息
    const chartCanvas = document.getElementById('pileStatusChart');
    const ctx = chartCanvas.getContext('2d');
    ctx.clearRect(0, 0, chartCanvas.width, chartCanvas.height);
    
    // 显示加载中信息
    ctx.font = '14px Arial';
    ctx.fillStyle = '#17a2b8';
    ctx.textAlign = 'center';
    ctx.fillText('正在加载数据...', chartCanvas.width / 2, chartCanvas.height / 2);
    
    // 先尝试ping服务器以检查API是否可用
    checkApiAvailability()
        .then(isAvailable => {
            if (!isAvailable) {
                throw new Error('后端服务不可用');
            }
            return fetch(API.piles.getAll);
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`服务器返回错误: ${response.status} ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            // 统计各状态的数量
            const statusCounts = {
                '0': 0, // 离线
                '1': 0, // 空闲
                '2': 0, // 使用中
                '3': 0  // 故障
            };
            
            // 解析API返回的数据
            data.forEach(pile => {
                const status = pile.status !== undefined ? pile.status.toString() : 'unknown';
                // 确保状态在范围内
                if (status in statusCounts) {
                    statusCounts[status]++;
                } else {
                    // 未知状态处理
                    statusCounts['unknown'] = (statusCounts['unknown'] || 0) + 1;
                }
            });
            
            // 准备图表数据
            const chartData = {
                labels: [],
                values: [],
                colors: []
            };
            
            // 根据API文档，设置状态标签和颜色，使用状态码
            if (statusCounts['1'] > 0) {
                chartData.labels.push('空闲');
                chartData.values.push(statusCounts['1']);
                chartData.colors.push('#28a745'); // 绿色
            }
            
            if (statusCounts['0'] > 0) {
                chartData.labels.push('离线');
                chartData.values.push(statusCounts['0']);
                chartData.colors.push('#17a2b8'); // 蓝色
            }
            
            if (statusCounts['2'] > 0) {
                chartData.labels.push('使用中');
                chartData.values.push(statusCounts['2']);
                chartData.colors.push('#ffc107'); // 黄色
            }
            
            if (statusCounts['3'] > 0) {
                chartData.labels.push('故障');
                chartData.values.push(statusCounts['3']);
                chartData.colors.push('#dc3545'); // 红色
            }
            
            // 如果有未知状态
            if (statusCounts['unknown'] > 0) {
                chartData.labels.push('未知');
                chartData.values.push(statusCounts['unknown']);
                chartData.colors.push('#6c757d'); // 灰色
            }
            
            // 没有数据时显示提示
            if (chartData.labels.length === 0) {
                chartData.labels = ['暂无数据'];
                chartData.values = [1];
                chartData.colors = ['#e9ecef'];
            }
            
            // 渲染图表
            renderPileStatusChart(chartData);
        })
        .catch(error => {
            console.error('获取充电桩状态数据失败:', error);
            
            // 显示错误信息并记录到控制台
            console.error('详细错误:', error.message);
            
            // 尝试使用备用静态数据 (以防API无法访问时仍能显示一些内容)
            const fallbackData = {
                labels: ['空闲', '离线', '使用中', '故障'],
                values: [3, 2, 1, 1],
                colors: ['#28a745', '#17a2b8', '#ffc107', '#dc3545']
            };
            
            // 显示错误信息并使用备用数据
            ctx.clearRect(0, 0, chartCanvas.width, chartCanvas.height);
            ctx.font = '14px Arial';
            ctx.fillStyle = '#dc3545';
            ctx.textAlign = 'center';
            ctx.fillText('获取数据失败 (显示备用数据)', chartCanvas.width / 2, 20);
            
            // 使用备用数据渲染图表
            renderPileStatusChart(fallbackData);
            
            // 添加重试按钮
            const parent = chartCanvas.parentElement;
            if (!document.getElementById('retryPileStatus')) {
                const retryBtn = document.createElement('button');
                retryBtn.id = 'retryPileStatus';
                retryBtn.className = 'btn btn-sm btn-primary mt-2';
                retryBtn.textContent = '重试加载';
                retryBtn.onclick = function() {
                    this.textContent = '正在加载...';
                    this.disabled = true;
                    loadPileStatusChart();
                    setTimeout(() => {
                        this.textContent = '重试加载';
                        this.disabled = false;
                    }, 3000);
                };
                parent.appendChild(retryBtn);
            }
        });
}

/**
 * 检查API服务器可用性
 * 返回Promise<boolean>
 */
function checkApiAvailability() {
    return new Promise((resolve) => {
        // 尝试简单请求来验证API是否可用
        fetch(`${API_BASE_URL}/api/health`, { 
            method: 'GET',
            headers: { 'Accept': 'application/json' },
            mode: 'cors',
            // 设置较短的超时时间
            signal: AbortSignal.timeout(3000)
        })
        .then(response => {
            resolve(response.ok);
        })
        .catch(() => {
            // 如果主健康检查端点不可用，尝试连接主API
            fetch(API.stations.getAll, { 
                method: 'HEAD',
                mode: 'cors',
                signal: AbortSignal.timeout(3000)
            })
            .then(response => {
                resolve(response.ok);
            })
            .catch(() => {
                resolve(false);
            });
        });
    });
}

/**
 * 渲染充电桩状态图表
 */
function renderPileStatusChart(data) {
    // 再次检查Chart.js是否可用
    if (typeof Chart === 'undefined') {
        console.error('Chart.js未加载，无法渲染图表');
        const chartCanvas = document.getElementById('pileStatusChart');
        const ctx = chartCanvas.getContext('2d');
        ctx.font = '14px Arial';
        ctx.fillStyle = '#dc3545';
        ctx.textAlign = 'center';
        ctx.fillText('图表库未加载，请刷新页面', chartCanvas.width / 2, chartCanvas.height / 2);
        return;
    }

    const ctx = document.getElementById('pileStatusChart').getContext('2d');
    
    try {
        // 安全销毁已有图表
        if (window.pileStatusChart && typeof window.pileStatusChart.destroy === 'function') {
            window.pileStatusChart.destroy();
        } else if (window.pileStatusChart) {
            // 如果存在但不能销毁，需要清除
            window.pileStatusChart = null;
            // 清除画布
            ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
        }
    } catch (error) {
        console.error('清除旧图表时出错:', error);
        // 确保变量被重置
        window.pileStatusChart = null;
        // 清除画布
        ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    }
    
    // 如果没有数据，显示提示并返回
    if (!data || !data.labels || data.labels.length === 0) {
        ctx.font = '14px Arial';
        ctx.fillStyle = '#6c757d';
        ctx.textAlign = 'center';
        ctx.fillText('暂无数据', ctx.canvas.width / 2, ctx.canvas.height / 2);
        return;
    }
    
    // 创建新图表
    try {
        window.pileStatusChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.labels,
                datasets: [{
                    data: data.values,
                    backgroundColor: data.colors,
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
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.raw || 0;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = Math.round((value / total) * 100);
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('创建图表时出错:', error);
        ctx.font = '14px Arial';
        ctx.fillStyle = '#dc3545';
        ctx.textAlign = 'center';
        ctx.fillText('图表渲染失败', ctx.canvas.width / 2, ctx.canvas.height / 2);
    }
}
