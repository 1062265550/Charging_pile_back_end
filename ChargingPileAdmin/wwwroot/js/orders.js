/**
 * 订单管理模块
 * 实现订单数据的加载、分页、搜索、详情查看和状态更新等功能
 */

// 在最顶部添加调试日志，确认脚本被加载
console.log("订单管理JS加载成功");

// 使用config.js中已定义的API，不要重新声明
console.log("使用全局API配置:", API.orders);

// 全局变量
let currentPage = 1;
let pageSize = 10;
let totalPages = 0;
let orderListData = [];
let currentOrderId = null;

// 模态框实例
let orderDetailModal;
let updateStatusModal;
let successModal;
let errorModal;

// DOM元素加载完成后执行初始化
document.addEventListener('DOMContentLoaded', function() {
    // 初始化模态框实例
    try {
        orderDetailModal = new bootstrap.Modal(document.getElementById('orderDetailModal'));
        updateStatusModal = new bootstrap.Modal(document.getElementById('updateStatusModal'));
        successModal = new bootstrap.Modal(document.getElementById('successModal'));
        errorModal = new bootstrap.Modal(document.getElementById('errorModal'));
        console.log("模态框初始化成功");
    } catch (error) {
        console.error("模态框初始化失败:", error);
    }
    
    // 初始化页面数据
    initPage();
    
    // 注册按钮点击事件，添加元素存在性检查
    const refreshDataBtn = document.getElementById('refreshData');
    if (refreshDataBtn) {
        refreshDataBtn.addEventListener('click', refreshData);
    }
    
    const exportDataBtn = document.getElementById('exportData');
    if (exportDataBtn) {
        exportDataBtn.addEventListener('click', exportOrdersToExcel);
    }
    
    const searchBtn = document.getElementById('searchBtn');
    if (searchBtn) {
        searchBtn.addEventListener('click', searchOrders);
    }
    
    const resetSearchBtn = document.getElementById('resetSearch');
    if (resetSearchBtn) {
        resetSearchBtn.addEventListener('click', resetSearchForm);
    }
    
    // 确保这些元素存在再添加事件监听
    const confirmUpdateStatusBtn = document.getElementById('confirmUpdateStatus');
    if (confirmUpdateStatusBtn) {
        confirmUpdateStatusBtn.addEventListener('click', updateOrderStatus);
    }
    
    const cancelOrderBtn = document.getElementById('cancelOrderBtn');
    if (cancelOrderBtn) {
        cancelOrderBtn.addEventListener('click', () => showUpdateStatusModal(3)); // 取消订单，状态值3
    }
    
    const completeOrderBtn = document.getElementById('completeOrderBtn');
    if (completeOrderBtn) {
        completeOrderBtn.addEventListener('click', () => showUpdateStatusModal(2)); // 完成订单，状态值2
    }
});

/**
 * 初始化页面数据
 */
function initPage() {
    // 设置日期选择器的初始值（默认为过去30天）
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);
    
    document.getElementById('endDate').valueAsDate = today;
    document.getElementById('startDate').valueAsDate = thirtyDaysAgo;
    
    // 修正：使用正确的ID 'searchBtn' 而非 'searchButton'
    document.getElementById('searchBtn').setAttribute('data-searched', 'false');
    
    // 加载订单统计数据
    loadOrderStats();
    
    // 加载订单列表（第一页）
    loadOrders(1);
}

/**
 * 加载订单统计信息
 */
function loadOrderStats() {
    // 确保API.orders.statistics存在
    if (!API.orders.statistics) {
        console.error("错误: API.orders.statistics 未定义，请检查config.js配置");
        document.getElementById('totalOrders').textContent = '错误';
        document.getElementById('completedOrders').textContent = '错误';
        document.getElementById('chargingOrders').textContent = '错误';
        document.getElementById('abnormalOrders').textContent = '错误';
        return;
    }
    
    console.log("正在请求订单统计数据...", API.orders.statistics);
    
    fetch(API.orders.statistics)
        .then(response => {
            console.log("收到统计数据响应:", response.status, response.statusText);
            if (!response.ok) {
                throw new Error('加载订单统计数据失败');
            }
            return response.json();
        })
        .then(data => {
            console.log("统计数据:", data);
            // 更新统计卡片
            document.getElementById('totalOrders').textContent = data.totalOrders || 0;
            document.getElementById('completedOrders').textContent = data.completedOrders || 0;
            document.getElementById('chargingOrders').textContent = data.chargingOrders || 0;
            document.getElementById('abnormalOrders').textContent = data.abnormalOrders || 0;
        })
        .catch(error => {
            console.error('获取订单统计信息失败:', error);
            showErrorModal('获取订单统计信息失败: ' + error.message);
        });
}

/**
 * 加载订单列表
 * @param {number} page 页码
 */
function loadOrders(page) {
    currentPage = page;
    
    // 获取筛选参数
    const keyword = document.getElementById('searchInput') ? document.getElementById('searchInput').value.trim() : '';
    const status = document.getElementById('statusFilter') ? document.getElementById('statusFilter').value : '';
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    
    // 构建URL查询参数
    let url = `${API.orders.getPaged}?pageNumber=${page}&pageSize=${pageSize}`;
    if (keyword) url += `&keyword=${encodeURIComponent(keyword)}`;
    if (status !== '') url += `&status=${status}`;
    
    // 只有当用户手动点击搜索按钮时才添加日期条件
    if (startDate && document.getElementById('searchBtn').getAttribute('data-searched') === 'true') {
        url += `&startDate=${startDate}`;
    }
    if (endDate && document.getElementById('searchBtn').getAttribute('data-searched') === 'true') {
        url += `&endDate=${endDate}`;
    }
    
    // 显示加载状态
    document.getElementById('orderTableBody').innerHTML = '<tr><td colspan="9" class="text-center">加载中...</td></tr>';
    
    // 添加更详细的日志，确认请求URL
    console.log("准备发送API请求，URL:", url);
    
    fetch(url)
        .then(response => {
            console.log("收到API响应，状态:", response.status, response.statusText);
            if (!response.ok) {
                // 尝试读取错误响应内容
                return response.text().then(text => {
                    console.error("API错误响应内容:", text);
                    throw new Error(`加载订单数据失败: ${response.status} ${response.statusText} - ${text}`);
                });
            }
            return response.json();
        })
        .then(data => {
            // 输出响应数据到控制台
            console.log("API响应数据:", data);
            
            // 保存数据到全局变量
            orderListData = data.items || [];
            totalPages = data.totalPages || 0;
            
            // 查看数据是否符合预期
            if (Array.isArray(orderListData)) {
                console.log(`成功获取到${orderListData.length}条订单数据`);
            } else {
                console.warn("警告: 返回的订单数据不是数组格式");
            }
            
            // 更新表格和分页
            renderOrderTable(orderListData);
            renderPagination(currentPage, totalPages);
        })
        .catch(error => {
            console.error('获取订单列表失败:', error);
            document.getElementById('orderTableBody').innerHTML = 
                `<tr><td colspan="9" class="text-center text-danger">加载失败: ${error.message}</td></tr>`;
            
            // 显示错误弹窗，方便调试
            showErrorModal(`获取订单列表失败: ${error.message}`);
        });
}

/**
 * 渲染订单表格
 * @param {Array} orders 订单数组
 */
function renderOrderTable(orders) {
    const tableBody = document.getElementById('orderTableBody');
    
    if (!orders || orders.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="9" class="text-center">暂无订单数据</td></tr>';
        return;
    }
    
    console.log("获取到订单数据:", orders); // 添加日志，查看订单数据内容
    
    let html = '';
    orders.forEach(order => {
        try {
            // 格式化日期和时间
            const startTime = order.startTime ? new Date(order.startTime).toLocaleString() : '未开始';
            
            // 构建状态标签
            const statusBadge = createStatusBadge(order.status, order.statusDescription);
            
            // 构建支付状态
            const paymentStatus = order.paymentStatus !== undefined ? 
                `<span class="payment-status-${order.paymentStatus}">${getPaymentStatusText(order.paymentStatus)}</span>` : 
                '未知';
            
            html += `
                <tr>
                    <td>${order.orderNo || '-'}</td>
                    <td>${order.userNickname || `用户ID: ${order.userId || '-'}`}</td>
                    <td>${order.pileNo || '-'}</td>
                    <td>${statusBadge}</td>
                    <td>${startTime}</td>
                    <td>${order.powerConsumption ? order.powerConsumption.toFixed(2) : '0.00'}</td>
                    <td>￥${order.totalAmount ? order.totalAmount.toFixed(2) : '0.00'}</td>
                    <td>${paymentStatus}</td>
                    <td>
                        <button class="btn btn-sm btn-outline-primary me-1" onclick="viewOrderDetail('${order.id}')">
                            <i class="bi bi-eye"></i> 详情
                        </button>
                        <button class="btn btn-sm btn-outline-secondary" onclick="showUpdateStatusModal(null, '${order.id}')">
                            <i class="bi bi-pencil"></i> 更新状态
                        </button>
                    </td>
                </tr>
            `;
        } catch (error) {
            console.error("渲染订单数据时出错:", error, "问题订单:", order);
            // 对错误数据进行容错处理
            html += `
                <tr>
                    <td>${order.orderNo || '-'}</td>
                    <td>${order.userNickname || `用户ID: ${order.userId || '-'}`}</td>
                    <td>${order.pileNo || '-'}</td>
                    <td><span class="status-badge status-4">数据错误</span></td>
                    <td>-</td>
                    <td>-</td>
                    <td>-</td>
                    <td>-</td>
                    <td>
                        <button class="btn btn-sm btn-outline-primary me-1" onclick="viewOrderDetail('${order.id}')">
                            <i class="bi bi-eye"></i> 详情
                        </button>
                    </td>
                </tr>
            `;
        }
    });
    
    tableBody.innerHTML = html;
}

/**
 * 创建订单状态徽章
 * @param {number} status 状态码
 * @param {string} description 状态描述
 * @returns {string} HTML标签
 */
function createStatusBadge(status, description) {
    // 确保状态是数字类型
    const statusNum = parseInt(status);
    let statusClass;
    let statusText = description || '';
    
    // 如果状态为null或undefined，显示为未知状态
    if (status === null || status === undefined) {
        statusClass = 'status-4'; // 使用异常样式
        if (!statusText) statusText = '未知状态';
        return `<span class="status-badge ${statusClass}">${statusText}</span>`;
    }
    
    // 根据状态值选择样式
    switch(statusNum) {
        case 0:
            statusClass = 'status-0'; // 待支付
            if (!statusText) statusText = '待支付';
            break;
        case 1:
            statusClass = 'status-wait'; // 已支付(等待充电)
            if (!statusText) statusText = '已支付(等待充电)';
            break;
        case 2:
            statusClass = 'status-1'; // 充电中
            if (!statusText) statusText = '充电中';
            break;
        case 3:
            statusClass = 'status-2'; // 已完成
            if (!statusText) statusText = '已完成';
            break;
        case 4:
            statusClass = 'status-3'; // 已取消
            if (!statusText) statusText = '已取消';
            break;
        default:
            // 对于不在预期范围内的状态，统一显示为状态{状态值}
            statusClass = 'status-4'; // 使用异常样式
            if (!statusText) statusText = `状态${statusNum}`;
    }
    
    return `<span class="status-badge ${statusClass}">${statusText}</span>`;
}

/**
 * 渲染分页控件
 * @param {number} currentPage 当前页码
 * @param {number} totalPages 总页数
 */
function renderPagination(currentPage, totalPages) {
    const pagination = document.getElementById('pagination');
    let html = '';
    
    // 上一页按钮
    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="javascript:void(0)" onclick="loadOrders(${currentPage - 1})" aria-label="上一页">
                <span aria-hidden="true">&laquo;</span>
            </a>
        </li>
    `;
    
    // 页码按钮
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, startPage + 4);
    
    for (let i = startPage; i <= endPage; i++) {
        html += `
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="javascript:void(0)" onclick="loadOrders(${i})">${i}</a>
            </li>
        `;
    }
    
    // 下一页按钮
    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="javascript:void(0)" onclick="loadOrders(${currentPage + 1})" aria-label="下一页">
                <span aria-hidden="true">&raquo;</span>
            </a>
        </li>
    `;
    
    pagination.innerHTML = html;
}

/**
 * 查看订单详情
 * @param {string} orderId 订单ID
 */
function viewOrderDetail(orderId) {
    fetch(API.orders.getById(orderId))
        .then(response => {
            if (!response.ok) {
                throw new Error('获取订单详情失败');
            }
            return response.json();
        })
        .then(order => {
            // 保存当前订单ID
            currentOrderId = order.id;
            
            // 填充基本信息
            document.getElementById('detailOrderNo').textContent = order.orderNo || '-';
            document.getElementById('detailUserId').textContent = order.userId || '-';
            document.getElementById('detailUserNickname').textContent = order.userNickname || '-';
            document.getElementById('detailPileNo').textContent = order.pileNo || '-';
            document.getElementById('detailPortNo').textContent = order.portNo || '-';
            
            // 订单状态
            const statusMap = {
                0: '待支付',
                1: '已支付(等待充电)',
                2: '充电中',
                3: '已完成',
                4: '已取消'
            };
            document.getElementById('detailStatus').textContent = statusMap[order.status] || '未知';
            
            // 计费模式
            const billingModeMap = {
                0: '按时间',
                1: '电量+电量服务费',
                2: '电量+时间服务费'
            };
            document.getElementById('detailBillingMode').textContent = 
                order.billingMode !== null && order.billingMode !== undefined ? 
                billingModeMap[order.billingMode] : '-';
            
            // 充电信息
            document.getElementById('detailStartTime').textContent = 
                order.startTime ? new Date(order.startTime).toLocaleString() : '-';
            document.getElementById('detailEndTime').textContent = 
                order.endTime ? new Date(order.endTime).toLocaleString() : '-';
            
            // 充电时长格式化为小时分钟秒
            let chargingTimeText = '-';
            if (order.chargingTime) {
                const hours = Math.floor(order.chargingTime / 3600);
                const minutes = Math.floor((order.chargingTime % 3600) / 60);
                const seconds = order.chargingTime % 60;
                chargingTimeText = `${hours}小时${minutes}分钟${seconds}秒`;
            }
            document.getElementById('detailChargingTime').textContent = chargingTimeText;
            
            document.getElementById('detailPowerConsumption').textContent = 
                order.powerConsumption ? `${order.powerConsumption.toFixed(2)} kWh` : '-';
            document.getElementById('detailPower').textContent = 
                order.power ? `${order.power.toFixed(2)} kW` : '-';
            
            // 充电模式
            const chargingModeMap = {
                1: '充满自停',
                2: '按金额',
                3: '按时间',
                4: '按电量',
                5: '其他'
            };
            document.getElementById('detailChargingMode').textContent = 
                order.chargingMode !== null && order.chargingMode !== undefined ? 
                chargingModeMap[order.chargingMode] : '-';
            
            // 停止原因
            const stopReasonMap = {
                0: '充满自停',
                1: '时间用完',
                2: '金额用完',
                3: '手动停止',
                4: '电量用完'
            };
            document.getElementById('detailStopReason').textContent = 
                order.stopReason !== null && order.stopReason !== undefined ? 
                stopReasonMap[order.stopReason] : '-';
            
            // 费用信息
            document.getElementById('detailAmount').textContent = 
                order.amount ? `￥${order.amount.toFixed(2)}` : '-';
            document.getElementById('detailServiceFee').textContent = 
                order.serviceFee ? `￥${order.serviceFee.toFixed(2)}` : '-';
            document.getElementById('detailTotalAmount').textContent = 
                order.totalAmount ? `￥${order.totalAmount.toFixed(2)}` : '-';
            
            // 支付状态
            const paymentStatusMap = {
                0: '未支付',
                1: '已支付',
                2: '已退款'
            };
            document.getElementById('detailPaymentStatus').textContent = 
                order.paymentStatus !== null && order.paymentStatus !== undefined ? 
                paymentStatusMap[order.paymentStatus] : '-';
            
            document.getElementById('detailPaymentTime').textContent = 
                order.paymentTime ? new Date(order.paymentTime).toLocaleString() : '-';
            
            // 支付方式
            const paymentMethodMap = {
                1: '微信',
                2: '支付宝',
                3: '余额'
            };
            document.getElementById('detailPaymentMethod').textContent = 
                order.paymentMethod !== null && order.paymentMethod !== undefined ? 
                paymentMethodMap[order.paymentMethod] : '-';
            
            document.getElementById('detailTransactionId').textContent = order.transactionId || '-';
            
            // 分时电费信息
            document.getElementById('detailSharpElectricity').textContent = 
                order.sharpElectricity ? `${order.sharpElectricity.toFixed(2)} kWh` : '-';
            document.getElementById('detailSharpAmount').textContent = 
                order.sharpAmount ? `￥${order.sharpAmount.toFixed(2)}` : '-';
            
            document.getElementById('detailPeakElectricity').textContent = 
                order.peakElectricity ? `${order.peakElectricity.toFixed(2)} kWh` : '-';
            document.getElementById('detailPeakAmount').textContent = 
                order.peakAmount ? `￥${order.peakAmount.toFixed(2)}` : '-';
            
            document.getElementById('detailFlatElectricity').textContent = 
                order.flatElectricity ? `${order.flatElectricity.toFixed(2)} kWh` : '-';
            document.getElementById('detailFlatAmount').textContent = 
                order.flatAmount ? `￥${order.flatAmount.toFixed(2)}` : '-';
            
            document.getElementById('detailValleyElectricity').textContent = 
                order.valleyElectricity ? `${order.valleyElectricity.toFixed(2)} kWh` : '-';
            document.getElementById('detailValleyAmount').textContent = 
                order.valleyAmount ? `￥${order.valleyAmount.toFixed(2)}` : '-';
            
            // 备注信息
            document.getElementById('detailRemark').textContent = order.remark || '无';
            
            // 控制操作按钮显示逻辑
            const cancelOrderBtn = document.getElementById('cancelOrderBtn');
            const completeOrderBtn = document.getElementById('completeOrderBtn');
            
            // 只有在待支付或充电中状态才能取消订单
            cancelOrderBtn.style.display = (order.status === 0 || order.status === 1) ? 'inline-block' : 'none';
            
            // 只有在充电中状态才能完成订单
            completeOrderBtn.style.display = order.status === 2 ? 'inline-block' : 'none';
            
            // 显示模态框
            orderDetailModal.show();
        })
        .catch(error => {
            console.error('获取订单详情失败:', error);
            showErrorModal('获取订单详情失败: ' + error.message);
        });
}

/**
 * 显示更新订单状态的模态框
 * @param {number|null} statusValue 状态值，如果提供，则预设状态
 * @param {string|null} orderId 订单ID，如果不提供，则使用当前查看的订单
 */
function showUpdateStatusModal(statusValue, orderId) {
    if (!orderId && currentOrderId) {
        orderId = currentOrderId;
    }
    
    if (!orderId) {
        showErrorModal('未选择订单');
        return;
    }
    
    // 更新模态框中的订单ID
    document.getElementById('updateOrderId').value = orderId;
    
    // 如果提供了状态值，则预设
    if (statusValue !== null && statusValue !== undefined) {
        const statusSelect = document.getElementById('updateOrderStatus');
        statusSelect.value = statusValue;
    }
    
    // 显示模态框
    updateStatusModal.show();
}

/**
 * 更新订单状态
 */
function updateOrderStatus() {
    const orderId = document.getElementById('updateOrderId').value;
    const status = parseInt(document.getElementById('updateOrderStatus').value);
    const remark = document.getElementById('updateRemark').value;
    
    if (!orderId) {
        showErrorModal('缺少订单ID');
        return;
    }
    
    // 禁用提交按钮，防止重复提交
    const confirmBtn = document.getElementById('confirmUpdateStatus');
    const originalText = confirmBtn.textContent;
    confirmBtn.disabled = true;
    confirmBtn.textContent = '提交中...';
    
    // 构建请求数据
    const data = {
        status: status
    };
    
    // 如果有备注，则添加到请求数据中
    if (remark.trim()) {
        data.remark = remark.trim();
    }
    
    fetch(API.orders.updateStatus(orderId), {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => {
                throw new Error(text || response.statusText || '更新订单状态失败');
            });
        }
        
        // 隐藏状态更新模态框
        updateStatusModal.hide();
        
        // 显示成功消息
        showSuccessModal('订单状态更新成功');
        
        // 刷新订单列表和统计
        refreshData();
    })
    .catch(error => {
        console.error('更新订单状态失败:', error);
        showErrorModal('更新订单状态失败: ' + error.message);
    })
    .finally(() => {
        // 恢复按钮状态
        confirmBtn.disabled = false;
        confirmBtn.textContent = originalText;
    });
}

/**
 * 搜索订单
 */
function searchOrders() {
    // 设置搜索标志为true，表示用户已手动点击搜索
    document.getElementById('searchBtn').setAttribute('data-searched', 'true');
    // 加载第一页数据
    loadOrders(1);
}

/**
 * 重置搜索表单
 */
function resetSearchForm() {
    document.getElementById('searchInput').value = '';
    document.getElementById('statusFilter').value = '';
    
    // 设置默认日期范围
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);
    
    document.getElementById('endDate').valueAsDate = today;
    document.getElementById('startDate').valueAsDate = thirtyDaysAgo;
    
    // 重置搜索标志
    document.getElementById('searchBtn').setAttribute('data-searched', 'false');
    
    // 重新加载订单
    loadOrders(1);
}

/**
 * 刷新数据
 */
function refreshData() {
    // 重置搜索标志，确保显示所有数据
    document.getElementById('searchBtn').setAttribute('data-searched', 'false');
    
    // 刷新统计数据
    loadOrderStats();
    
    // 刷新订单列表，保持在当前页
    loadOrders(currentPage);
}

/**
 * 导出订单数据为Excel
 */
function exportOrdersToExcel() {
    // 获取筛选参数
    const keyword = document.getElementById('searchInput').value.trim();
    const status = document.getElementById('statusFilter').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    
    // 将日期格式化为YYYY-MM-DD格式
    let startDateStr = '';
    let endDateStr = '';
    
    if (startDate) {
        const date = new Date(startDate);
        startDateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    }
    
    if (endDate) {
        const date = new Date(endDate);
        endDateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    }
    
    // 构建导出URL
    let exportUrl = `${API.orders.getAll}/export?`;
    const params = [];
    
    if (keyword) params.push(`keyword=${encodeURIComponent(keyword)}`);
    if (status) params.push(`status=${status}`);
    if (startDate) params.push(`startDate=${startDateStr}`);
    if (endDate) params.push(`endDate=${endDateStr}`);
    
    exportUrl += params.join('&');
    
    // 使用窗口打开导出链接
    window.open(exportUrl, '_blank');
}

/**
 * 显示成功提示模态框
 * @param {string} message 成功消息
 */
function showSuccessModal(message) {
    document.getElementById('successMessage').textContent = message;
    successModal.show();
}

/**
 * 显示错误提示模态框
 * @param {string} message 错误消息
 */
function showErrorModal(message) {
    document.getElementById('errorMessage').textContent = message;
    errorModal.show();
}

/**
 * 获取支付状态文本
 * @param {number} status 支付状态
 * @returns {string} 支付状态文本
 */
function getPaymentStatusText(status) {
    // 确保状态是数字
    if (status === null || status === undefined) {
        return '未知';
    }
    
    const paymentStatus = parseInt(status);
    switch (paymentStatus) {
        case 0:
            return '未支付';
        case 1:
            return '已支付';
        case 2:
            return '已退款';
        default:
            return `支付状态${paymentStatus}`;
    }
}

// 在全局作用域暴露一些函数，以便在HTML中调用
window.loadOrders = loadOrders;
window.viewOrderDetail = viewOrderDetail;
window.showUpdateStatusModal = showUpdateStatusModal;
