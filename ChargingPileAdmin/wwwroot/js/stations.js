/**
 * 充电站管理页面脚本
 * 实现充电站的增删改查功能
 */

// 当前页码和页大小
let currentPage = 1;
const pageSize = 10;

// 当前筛选条件
let currentFilter = {
    keyword: '',
    status: ''
};

// 模态框实例
let stationModal;
let deleteModal;
let errorModal;
let successModal;

// 页面加载完成后执行
document.addEventListener('DOMContentLoaded', function() {
    // 初始化模态框
    stationModal = new bootstrap.Modal(document.getElementById('stationModal'));
    deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    errorModal = new bootstrap.Modal(document.getElementById('errorModal'));
    successModal = new bootstrap.Modal(document.getElementById('successModal'));
    
    // 初始化事件监听
    initEventListeners();
    
    // 加载充电站数据
    loadStations();
});

/**
 * 初始化事件监听
 */
function initEventListeners() {
    // 添加充电站按钮点击事件
    document.getElementById('addStationBtn').addEventListener('click', function() {
        // 重置表单
        document.getElementById('stationForm').reset();
        document.getElementById('stationId').value = '';
        document.getElementById('stationModalLabel').textContent = '添加充电站';
        
        // 显示模态框
        stationModal.show();
    });
    
    // 搜索按钮点击事件
    document.getElementById('searchBtn').addEventListener('click', function() {
        currentFilter.keyword = document.getElementById('searchInput').value.trim();
        currentPage = 1;
        loadStations();
    });
    
    // 搜索框回车事件
    document.getElementById('searchInput').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            currentFilter.keyword = this.value.trim();
            currentPage = 1;
            loadStations();
        }
    });
    
    // 状态筛选器变化事件
    document.getElementById('statusFilter').addEventListener('change', function() {
        currentFilter.status = this.value;
        currentPage = 1;
        loadStations();
    });
    
    // 刷新按钮点击事件
    document.getElementById('refreshBtn').addEventListener('click', loadStations);
    
    // 保存充电站按钮点击事件
    document.getElementById('saveStationBtn').addEventListener('click', saveStation);
    
    // 确认删除按钮点击事件
    document.getElementById('confirmDeleteBtn').addEventListener('click', deleteStation);
}

/**
 * 加载充电站数据
 */
function loadStations() {
    // 构建API URL
    let url = API.stations.getPaged + `?pageNumber=${currentPage}&pageSize=${pageSize}`;
    
    // 添加筛选条件
    if (currentFilter.keyword) {
        url += `&keyword=${encodeURIComponent(currentFilter.keyword)}`;
    }
    
    if (currentFilter.status) {
        url += `&status=${currentFilter.status}`;
    }
    
    // 显示加载中
    document.getElementById('stationsTable').querySelector('tbody').innerHTML = `
        <tr>
            <td colspan="7" class="text-center">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">加载中...</span>
                </div>
            </td>
        </tr>
    `;
    
    // 调用API
    fetch(url)
        .then(response => response.json())
        .then(data => {
            renderStationsTable(data.items);
            renderPagination(data);
        })
        .catch(error => {
            console.error('加载充电站数据失败:', error);
            document.getElementById('stationsTable').querySelector('tbody').innerHTML = `
                <tr>
                    <td colspan="7" class="text-center text-danger">
                        加载数据失败，请重试
                    </td>
                </tr>
            `;
        });
}

/**
 * 渲染充电站表格
 */
function renderStationsTable(stations) {
    const tableBody = document.getElementById('stationsTable').querySelector('tbody');
    
    if (!stations || stations.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center">
                    没有找到符合条件的充电站
                </td>
            </tr>
        `;
        return;
    }
    
    let html = '';
    stations.forEach(station => {
        html += `
            <tr>
                <td>${station.id || '-'}</td>
                <td>${station.name || '-'}</td>
                <td>${station.address || '-'}</td>
                <td>${UTILS.getStatusHtml(station.status, DICT.stationStatus)}</td>
                <td>${station.pileCount || 0}</td>
                <td>${UTILS.formatDateTime(station.updateTime)}</td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-primary" onclick="viewStationDetails('${station.id}')">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button type="button" class="btn btn-warning" onclick="editStation('${station.id}')">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-danger" onclick="showDeleteConfirm('${station.id}')">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    });
    
    tableBody.innerHTML = html;
}

/**
 * 渲染分页控件
 */
function renderPagination(data) {
    const pagination = document.getElementById('pagination');
    const totalPages = data.totalPages || 1;
    currentPage = data.pageNumber || 1;
    
    let html = '';
    
    // 上一页按钮
    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changePage(${currentPage - 1}); return false;">上一页</a>
        </li>
    `;
    
    // 页码按钮
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, startPage + 4);
    
    for (let i = startPage; i <= endPage; i++) {
        html += `
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="changePage(${i}); return false;">${i}</a>
            </li>
        `;
    }
    
    // 下一页按钮
    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changePage(${currentPage + 1}); return false;">下一页</a>
        </li>
    `;
    
    pagination.innerHTML = html;
}

/**
 * 切换页码
 */
function changePage(page) {
    currentPage = page;
    loadStations();
}

/**
 * 查看充电站详情
 */
function viewStationDetails(id) {
    window.location.href = `charging-station-detail.html?id=${id}`;
}

/**
 * 编辑充电站
 */
function editStation(id) {
    fetch(API.stations.getById(id))
        .then(response => response.json())
        .then(station => {
            document.getElementById('stationId').value = station.id;
            document.getElementById('stationName').value = station.name || '';
            document.getElementById('stationAddress').value = station.address || '';
            document.getElementById('stationLocation').value = station.location || '';
            document.getElementById('stationStatus').value = station.status || 1;
            
            // 更新模态框标题
            document.getElementById('stationModalLabel').textContent = '编辑充电站';
            
            // 显示模态框
            stationModal.show();
        })
        .catch(error => {
            console.error('获取充电站详情失败:', error);
            showErrorModal('获取充电站详情失败，请重试');
        });
}

/**
 * 保存充电站（新增或更新）
 */
function saveStation() {
    // 获取表单数据
    const id = document.getElementById('stationId').value;
    const name = document.getElementById('stationName').value;
    const address = document.getElementById('stationAddress').value;
    const location = document.getElementById('stationLocation').value;
    const status = parseInt(document.getElementById('stationStatus').value);
    
    // 验证表单
    if (!name) {
        showErrorModal('请输入充电站名称');
        return;
    }
    
    if (!address) {
        showErrorModal('请输入充电站地址');
        return;
    }
    
    // 构建请求数据
    const stationData = {
        name: name,
        address: address,
        location: location,
        status: status
    };
    
    // 判断是新增还是更新
    const isNew = !id;
    const url = isNew ? API.stations.create : API.stations.update(id);
    const method = isNew ? 'POST' : 'PUT';
    
    // 发送请求
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(stationData)
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('请求失败');
        }
        return isNew ? response.json() : { success: true };
    })
    .then(data => {
        // 隐藏模态框
        stationModal.hide();
        
        // 刷新数据
        loadStations();
        
        // 显示成功消息
        showSuccessModal(isNew ? '充电站添加成功' : '充电站修改成功');
    })
    .catch(error => {
        console.error('保存充电站失败:', error);
        showErrorModal('保存充电站失败，请重试');
    });
}

/**
 * 显示删除确认对话框
 */
function showDeleteConfirm(id) {
    document.getElementById('deleteStationId').value = id;
    deleteModal.show();
}

/**
 * 删除充电站
 */
function deleteStation() {
    const id = document.getElementById('deleteStationId').value;
    
    if (!id) {
        deleteModal.hide();
        showErrorModal('充电站ID无效');
        return;
    }
    
    // 显示加载提示
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
    const originalText = confirmDeleteBtn.innerHTML;
    confirmDeleteBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> 处理中...';
    confirmDeleteBtn.disabled = true;
    
    fetch(API.stations.delete(id), {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        // 处理不同的响应状态
        if (response.status === 204) {
            return { success: true }; // 删除成功
        } else {
            // 直接读取响应文本，而不是尝试解析JSON
            return response.text()
                .then(text => {
                    // 尝试解析为JSON，如果失败则直接使用文本
                    try {
                        const data = JSON.parse(text);
                        throw new Error(data.message || data || '删除失败');
                    } catch (jsonError) {
                        // 如果不是JSON，直接使用返回的文本
                        throw new Error(text || response.statusText || '删除失败');
                    }
                });
        }
    })
    .then(data => {
        // 恢复按钮状态
        confirmDeleteBtn.innerHTML = originalText;
        confirmDeleteBtn.disabled = false;
        
        if (data.success) {
            // 隐藏确认对话框
            deleteModal.hide();
            
            // 刷新数据
            loadStations();
            
            // 显示成功消息
            setTimeout(() => {
                showSuccessModal('充电站删除成功');
            }, 300); // 添加短暂延迟，确保前一个模态框完全关闭
        }
    })
    .catch(error => {
        // 恢复按钮状态
        confirmDeleteBtn.innerHTML = originalText;
        confirmDeleteBtn.disabled = false;
        
        console.error('删除充电站失败:', error);
        
        // 显示更友好的错误消息
        let errorMessage = '删除充电站失败';
        
        // 处理具体的错误情况
        if (error.message.includes('充电桩')) {
            errorMessage = '该充电站下存在充电桩，请先删除所有充电桩后再删除充电站';
        } else if (error.message.includes('权限')) {
            errorMessage = '您没有权限执行此操作';
        } else if (error.message) {
            errorMessage = error.message;
        }
        
        // 先关闭确认对话框，再显示错误信息
        deleteModal.hide();
        
        // 添加短暂延迟，确保前一个模态框完全关闭
        setTimeout(() => {
            showErrorModal(errorMessage);
        }, 300);
    });
}

/**
 * 显示错误提示模态框
 * @param {string} message 错误信息
 */
function showErrorModal(message) {
    const errorMessageEl = document.getElementById('errorMessage');
    errorMessageEl.textContent = message;
    
    errorModal.show();
}

/**
 * 显示成功提示模态框
 * @param {string} message 成功信息
 */
function showSuccessModal(message) {
    const successMessageEl = document.getElementById('successMessage');
    successMessageEl.textContent = message;
    
    successModal.show();
}
