/**
 * 充电桩管理页面脚本
 * 实现充电桩的增删改查功能
 */

// 当前页码和页大小
let currentPage = 1;
const pageSize = 10;

// 当前筛选条件
let currentFilter = {
    stationId: '',
    status: '',
    keyword: ''
};

// 模态框实例
let pileModal;
let pileDetailModal;
let deleteModal;
let errorModal;
let successModal;
let portModal; 
let deletePortModal; 

// 当前选中的充电桩ID
let currentPileId = '';

// 页面加载完成后执行
document.addEventListener('DOMContentLoaded', function() {
    // 初始化模态框
    pileModal = new bootstrap.Modal(document.getElementById('pileModal'));
    pileDetailModal = new bootstrap.Modal(document.getElementById('pileDetailModal'));
    deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    errorModal = new bootstrap.Modal(document.getElementById('errorModal'));
    successModal = new bootstrap.Modal(document.getElementById('successModal'));
    portModal = new bootstrap.Modal(document.getElementById('portModal')); 
    deletePortModal = new bootstrap.Modal(document.getElementById('deletePortModal')); 
    
    // 初始化事件监听
    initEventListeners();
    
    // 加载充电站数据（用于筛选和下拉列表）
    loadStations();
    
    // 加载充电桩数据
    loadPiles();
});

/**
 * 初始化事件监听
 */
function initEventListeners() {
    // 添加充电桩按钮点击事件
    document.getElementById('addPileBtn').addEventListener('click', function() {
        // 重置表单
        document.getElementById('pileForm').reset();
        document.getElementById('pileId').value = '';
        document.getElementById('stationId').value = '';
        // 设置默认状态为"离线"
        document.getElementById('status').value = '0';
        document.getElementById('pileModalLabel').textContent = '添加充电桩';
        
        // 显示模态框
        pileModal.show();
    });
    
    // 搜索按钮点击事件
    document.getElementById('searchBtn').addEventListener('click', function() {
        currentFilter.keyword = document.getElementById('searchInput').value.trim();
        currentPage = 1;
        loadPiles();
    });
    
    // 搜索框回车事件
    document.getElementById('searchInput').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            currentFilter.keyword = this.value.trim();
            currentPage = 1;
            loadPiles();
        }
    });
    
    // 充电站筛选器变化事件
    document.getElementById('stationFilter').addEventListener('change', function() {
        currentFilter.stationId = this.value;
        currentPage = 1;
        loadPiles();
    });
    
    // 状态筛选器变化事件
    document.getElementById('statusFilter').addEventListener('change', function() {
        currentFilter.status = this.value;
        currentPage = 1;
        loadPiles();
    });
    
    // 刷新按钮点击事件
    document.getElementById('refreshBtn').addEventListener('click', loadPiles);
    
    // 保存充电桩按钮点击事件
    document.getElementById('savePileBtn').addEventListener('click', savePile);
    
    // 确认删除按钮点击事件
    document.getElementById('confirmDeleteBtn').addEventListener('click', deletePile);
    
    // 添加成功模态框关闭后的事件处理
    document.getElementById('successModal').addEventListener('hidden.bs.modal', function() {
        // 刷新充电桩列表以确保显示最新数据
        loadPiles();
    });
    
    // 添加充电端口按钮点击事件
    document.getElementById('addPortBtn').addEventListener('click', function() {
        // 重置表单
        document.getElementById('portForm').reset();
        document.getElementById('portId').value = '';
        document.getElementById('portPileId').value = currentPileId;
        document.getElementById('portStatus').value = '1'; // 默认为空闲状态
        document.getElementById('portModalLabel').textContent = '添加充电端口';
        
        // 显示模态框
        portModal.show();
    });
    
    // 保存充电端口按钮点击事件
    document.getElementById('savePortBtn').addEventListener('click', savePort);
    
    // 确认删除充电端口按钮点击事件
    document.getElementById('confirmDeletePortBtn').addEventListener('click', deletePort);
}

/**
 * 加载充电站数据
 */
function loadStations() {
    fetch(API.stations.getAll)
        .then(response => response.json())
        .then(stations => {
            // 充电站筛选下拉列表
            const stationFilterEl = document.getElementById('stationFilter');
            // 添加充电桩表单中的充电站下拉列表
            const stationIdEl = document.getElementById('stationId');
            
            // 清空之前的选项（保留第一个默认选项）
            stationFilterEl.innerHTML = '<option value="">所有充电站</option>';
            stationIdEl.innerHTML = '<option value="">请选择充电站</option>';
            
            // 添加充电站选项
            stations.forEach(station => {
                const option1 = document.createElement('option');
                option1.value = station.id;
                option1.textContent = `${station.name} (${station.address})`;
                stationFilterEl.appendChild(option1);
                
                const option2 = document.createElement('option');
                option2.value = station.id;
                option2.textContent = `${station.name} (${station.address})`;
                stationIdEl.appendChild(option2);
            });
        })
        .catch(error => {
            console.error('加载充电站数据失败:', error);
            showErrorModal('加载充电站数据失败，请重试');
        });
}

/**
 * 加载充电桩数据
 */
function loadPiles() {
    // 构建API URL
    let url = API.piles.getPaged + `?pageNumber=${currentPage}&pageSize=${pageSize}`;
    
    // 添加筛选条件
    if (currentFilter.stationId) {
        url += `&stationId=${encodeURIComponent(currentFilter.stationId)}`;
    }
    
    if (currentFilter.status) {
        url += `&status=${currentFilter.status}`;
    }
    
    if (currentFilter.keyword) {
        url += `&keyword=${encodeURIComponent(currentFilter.keyword)}`;
    }
    
    // 显示加载中
    document.getElementById('pilesTable').querySelector('tbody').innerHTML = `
        <tr>
            <td colspan="9" class="text-center">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">加载中...</span>
                </div>
            </td>
        </tr>
    `;
    
    // 添加缓存破坏参数，确保每次都获取最新数据
    const cacheBuster = new Date().getTime();
    url += `&_=${cacheBuster}`;
    
    // 调用API
    fetch(url)
        .then(response => response.json())
        .then(data => {
            renderPilesTable(data.items);
            renderPagination(data);
        })
        .catch(error => {
            console.error('加载充电桩数据失败:', error);
            document.getElementById('pilesTable').querySelector('tbody').innerHTML = `
                <tr>
                    <td colspan="9" class="text-center text-danger">
                        加载数据失败，请重试
                    </td>
                </tr>
            `;
        });
}

/**
 * 渲染充电桩表格
 */
function renderPilesTable(piles) {
    const tableBody = document.getElementById('pilesTable').querySelector('tbody');
    
    if (piles.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="9" class="text-center">
                    未找到充电桩数据
                </td>
            </tr>
        `;
        return;
    }
    
    let tableContent = '';
    
    piles.forEach(pile => {
        // 格式化状态显示的样式
        let statusClass = 'badge bg-secondary';
        if (pile.status === 0) statusClass = 'badge bg-secondary'; // 离线
        else if (pile.status === 1) statusClass = 'badge bg-success'; // 空闲
        else if (pile.status === 2) statusClass = 'badge bg-primary'; // 使用中
        else if (pile.status === 3) statusClass = 'badge bg-danger'; // 故障

        // 格式化在线状态显示的样式
        let onlineStatusClass = pile.onlineStatus === 1 ? 'badge bg-success' : 'badge bg-secondary';
        
        // 格式化最后心跳时间
        const lastHeartbeatTime = pile.lastHeartbeatTime ? new Date(pile.lastHeartbeatTime).toLocaleString() : '未知';
        
        tableContent += `
            <tr>
                <td>${pile.pileNo}</td>
                <td>${pile.stationName}</td>
                <td>${pile.pileTypeDescription}</td>
                <td>${pile.powerRate} kW</td>
                <td><span class="${statusClass}">${pile.statusDescription}</span></td>
                <td>${pile.totalPorts}</td>
                <td><span class="${onlineStatusClass}">${pile.onlineStatusDescription}</span></td>
                <td>${lastHeartbeatTime}</td>
                <td>
                    <div class="btn-group btn-group-sm" role="group">
                        <button type="button" class="btn btn-info" onclick="viewPileDetails('${pile.id}')">
                            <i class="fas fa-info-circle"></i>
                        </button>
                        <button type="button" class="btn btn-primary" onclick="editPile('${pile.id}')">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-danger" onclick="showDeleteConfirm('${pile.id}')">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    });
    
    tableBody.innerHTML = tableContent;
}

/**
 * 渲染分页控件
 */
function renderPagination(data) {
    const pagination = document.getElementById('pagination');
    
    // 计算显示的页码范围
    const totalPages = data.totalPages;
    const currentPageNum = data.pageNumber;
    
    let startPage = Math.max(currentPageNum - 2, 1);
    let endPage = Math.min(startPage + 4, totalPages);
    
    if (endPage - startPage < 4 && totalPages > 4) {
        startPage = Math.max(endPage - 4, 1);
    }
    
    let paginationHTML = `
        <li class="page-item ${currentPageNum === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="${currentPageNum > 1 ? `changePage(1); return false;` : 'return false;'}">首页</a>
        </li>
        <li class="page-item ${currentPageNum === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="${currentPageNum > 1 ? `changePage(${currentPageNum - 1}); return false;` : 'return false;'}">上一页</a>
        </li>
    `;
    
    for (let i = startPage; i <= endPage; i++) {
        paginationHTML += `
            <li class="page-item ${i === currentPageNum ? 'active' : ''}">
                <a class="page-link" href="#" onclick="changePage(${i}); return false;">${i}</a>
            </li>
        `;
    }
    
    paginationHTML += `
        <li class="page-item ${currentPageNum === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="${currentPageNum < totalPages ? `changePage(${currentPageNum + 1}); return false;` : 'return false;'}">下一页</a>
        </li>
        <li class="page-item ${currentPageNum === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="${currentPageNum < totalPages ? `changePage(${totalPages}); return false;` : 'return false;'}">末页</a>
        </li>
    `;
    
    pagination.innerHTML = paginationHTML;
}

/**
 * 切换页码
 */
function changePage(page) {
    if (page < 1) return;
    
    currentPage = page;
    loadPiles();
}

/**
 * 查看充电桩详情
 */
function viewPileDetails(id) {
    // 设置当前充电桩ID
    currentPileId = id;
    
    fetch(API.piles.getById(id))
        .then(response => response.json())
        .then(pile => {
            // 填充充电桩详情
            document.getElementById('detail-pileNo').textContent = pile.pileNo || '-';
            document.getElementById('detail-stationName').textContent = pile.stationName || '-';
            document.getElementById('detail-pileTypeDescription').textContent = pile.pileTypeDescription || '-';
            document.getElementById('detail-powerRate').textContent = pile.powerRate ? `${pile.powerRate} kW` : '-';
            document.getElementById('detail-manufacturer').textContent = pile.manufacturer || '-';
            document.getElementById('detail-imei').textContent = pile.imei || '-';
            document.getElementById('detail-totalPorts').textContent = pile.totalPorts || '0';
            
            document.getElementById('detail-statusDescription').innerHTML = UTILS.getStatusHtml(pile.status, DICT.pileStatus);
            document.getElementById('detail-onlineStatusDescription').textContent = pile.isOnline ? '在线' : '离线';
            document.getElementById('detail-signalStrength').textContent = pile.signalStrength ? `${pile.signalStrength}%` : '-';
            document.getElementById('detail-temperature').textContent = pile.temperature ? `${pile.temperature}℃` : '-';
            document.getElementById('detail-lastHeartbeatTime').textContent = UTILS.formatDateTime(pile.lastHeartbeatTime);
            document.getElementById('detail-installationDate').textContent = UTILS.formatDate(pile.installationDate);
            document.getElementById('detail-lastMaintenanceDate').textContent = UTILS.formatDate(pile.lastMaintenanceDate);
            
            // 加载充电端口信息
            loadPortsForPile(id);
            
            // 显示详情模态框
            pileDetailModal.show();
        })
        .catch(error => {
            console.error('加载充电桩详情失败:', error);
            showErrorModal('加载充电桩详情失败，请重试');
        });
}

/**
 * 加载充电桩的端口信息
 */
function loadPortsForPile(pileId) {
    fetch(API.ports.getByPile(pileId))
        .then(response => response.json())
        .then(ports => {
            const portsTable = document.getElementById('portsTable').querySelector('tbody');
            
            if (ports.length === 0) {
                portsTable.innerHTML = `
                    <tr>
                        <td colspan="10" class="text-center">
                            该充电桩暂无端口数据
                        </td>
                    </tr>
                `;
                return;
            }
            
            let tableContent = '';
            
            ports.forEach(port => {
                // 格式化状态显示
                const statusHtml = UTILS.getStatusHtml(port.status, DICT.portStatus);
                
                tableContent += `
                    <tr>
                        <td>${port.portNo}</td>
                        <td>${port.portTypeDescription || '未知'}</td>
                        <td>${statusHtml}</td>
                        <td>${port.voltage || '-'}</td>
                        <td>${port.currentAmpere || '-'}</td>
                        <td>${port.power || '-'}</td>
                        <td>${port.temperature || '-'}</td>
                        <td>${port.totalChargingTimes || '0'}</td>
                        <td>${port.totalPowerConsumption || '0'}</td>
                        <td>
                            <div class="btn-group btn-group-sm">
                                <button type="button" class="btn btn-outline-primary" onclick="editPort('${port.id}')">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" class="btn btn-outline-danger" onclick="showDeletePortConfirm('${port.id}')">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;
            });
            
            portsTable.innerHTML = tableContent;
        })
        .catch(error => {
            console.error('加载充电端口数据失败:', error);
            document.getElementById('portsTable').querySelector('tbody').innerHTML = `
                <tr>
                    <td colspan="10" class="text-center text-danger">
                        加载端口数据失败，请重试
                    </td>
                </tr>
            `;
        });
}

/**
 * 编辑充电桩
 */
function editPile(id) {
    fetch(API.piles.getById(id))
        .then(response => response.json())
        .then(pile => {
            // 填充表单
            document.getElementById('pileId').value = pile.id;
            document.getElementById('stationId').value = pile.stationId;
            document.getElementById('pileNo').value = pile.pileNo;
            document.getElementById('pileType').value = pile.pileType;
            document.getElementById('powerRate').value = pile.powerRate;
            document.getElementById('totalPorts').value = pile.totalPorts;
            document.getElementById('manufacturer').value = pile.manufacturer || '';
            document.getElementById('imei').value = pile.imei || '';
            document.getElementById('protocolVersion').value = pile.protocolVersion || '';
            document.getElementById('softwareVersion').value = pile.softwareVersion || '';
            document.getElementById('hardwareVersion').value = pile.hardwareVersion || '';
            document.getElementById('status').value = pile.status;
            
            // 处理日期
            if (pile.installationDate) {
                const date = new Date(pile.installationDate);
                document.getElementById('installationDate').value = date.toISOString().split('T')[0];
            } else {
                document.getElementById('installationDate').value = '';
            }
            
            // 更新模态框标题
            document.getElementById('pileModalLabel').textContent = '编辑充电桩';
            
            // 显示模态框
            pileModal.show();
        })
        .catch(error => {
            console.error('获取充电桩信息失败:', error);
            showErrorModal('获取充电桩信息失败，请重试');
        });
}

/**
 * 保存充电桩（新增或更新）
 */
function savePile() {
    // 获取表单数据
    const id = document.getElementById('pileId').value;
    const stationId = document.getElementById('stationId').value;
    const pileNo = document.getElementById('pileNo').value;
    const pileType = parseInt(document.getElementById('pileType').value);
    const powerRate = parseFloat(document.getElementById('powerRate').value);
    const totalPorts = parseInt(document.getElementById('totalPorts').value);
    const manufacturer = document.getElementById('manufacturer').value;
    const imei = document.getElementById('imei').value;
    const protocolVersion = document.getElementById('protocolVersion').value;
    const softwareVersion = document.getElementById('softwareVersion').value;
    const hardwareVersion = document.getElementById('hardwareVersion').value;
    const status = parseInt(document.getElementById('status').value);
    const installationDate = document.getElementById('installationDate').value;
    
    // 验证表单
    if (!stationId) {
        showErrorModal('请选择所属充电站');
        return;
    }
    
    if (!pileNo) {
        showErrorModal('请输入充电桩编号');
        return;
    }
    
    if (isNaN(powerRate) || powerRate <= 0 || powerRate > 500) {
        showErrorModal('请输入有效的额定功率（0.1-500kW之间）');
        return;
    }
    
    if (isNaN(totalPorts) || totalPorts < 1) {
        showErrorModal('请输入有效的端口数量');
        return;
    }
    
    // 构建请求数据
    const pileData = {
        stationId: stationId,
        pileNo: pileNo,
        pileType: pileType,
        powerRate: powerRate,
        totalPorts: totalPorts,
        manufacturer: manufacturer,
        imei: imei,
        protocolVersion: protocolVersion,
        softwareVersion: softwareVersion,
        hardwareVersion: hardwareVersion,
        status: status,
        installationDate: installationDate || null
    };
    
    // 判断是新增还是更新
    const isNew = !id;
    const url = isNew ? API.piles.create : API.piles.update(id);
    const method = isNew ? 'POST' : 'PUT';
    
    // 显示加载提示
    const savePileBtn = document.getElementById('savePileBtn');
    const originalText = savePileBtn.innerHTML;
    savePileBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> 处理中...';
    savePileBtn.disabled = true;
    
    // 发送请求
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(pileData)
    })
    .then(response => {
        // 先获取响应内容，然后再检查响应状态
        return response.text().then(text => {
            if (!response.ok) {
                try {
                    const errorData = JSON.parse(text);
                    // 处理验证错误
                    if (errorData.errors) {
                        // 将验证错误转换为用户友好的消息
                        const errorMessages = [];
                        
                        // 处理额定功率错误
                        if (errorData.errors.PowerRate) {
                            errorMessages.push('额定功率必须在0.1至500kW之间');
                        }
                        
                        // 处理充电桩编号错误
                        if (errorData.errors.PileNo) {
                            errorMessages.push('充电桩编号' + errorData.errors.PileNo[0].toLowerCase());
                        }
                        
                        // 处理充电站ID错误
                        if (errorData.errors.StationId) {
                            errorMessages.push('请选择有效的充电站');
                        }
                        
                        // 处理端口数量错误
                        if (errorData.errors.TotalPorts) {
                            errorMessages.push('端口数量必须大于0且为整数');
                        }
                        
                        // 其他通用错误处理
                        if (errorMessages.length === 0 && errorData.title) {
                            errorMessages.push(errorData.title);
                        }
                        
                        throw new Error(errorMessages.join('<br>') || '验证错误，请检查输入');
                    }
                    
                    // 处理其他错误
                    throw new Error(errorData.message || errorData || '请求失败');
                } catch (e) {
                    // 如果无法解析JSON或者没有明确的错误信息
                    if (text.includes('validation')) {
                        throw new Error('输入数据验证失败，请检查输入项');
                    } else {
                        throw new Error(text || response.statusText || '请求失败');
                    }
                }
            }
            
            try {
                // 如果响应内容是JSON，则解析它
                return text ? JSON.parse(text) : { success: true };
            } catch (e) {
                // 如果不是JSON，则返回成功标记
                return { success: true };
            }
        });
    })
    .then(data => {
        // 恢复按钮状态
        savePileBtn.innerHTML = originalText;
        savePileBtn.disabled = false;
        
        // 隐藏模态框
        pileModal.hide();
        
        // 立即刷新数据，确保更新后的数据显示出来
        loadPiles();
        
        // 显示成功消息
        setTimeout(() => {
            showSuccessModal(isNew ? '充电桩添加成功' : '充电桩修改成功');
        }, 300);
    })
    .catch(error => {
        // 恢复按钮状态
        savePileBtn.innerHTML = originalText;
        savePileBtn.disabled = false;
        
        console.error('保存充电桩失败:', error);
        showErrorModal('保存充电桩失败: ' + error.message);
    });
}

/**
 * 显示删除确认对话框
 */
function showDeleteConfirm(id) {
    document.getElementById('deletePileId').value = id;
    deleteModal.show();
}

/**
 * 删除充电桩
 */
function deletePile() {
    const id = document.getElementById('deletePileId').value;
    
    if (!id) {
        deleteModal.hide();
        showErrorModal('充电桩ID无效');
        return;
    }
    
    // 显示加载提示
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
    const originalText = confirmDeleteBtn.innerHTML;
    confirmDeleteBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> 处理中...';
    confirmDeleteBtn.disabled = true;
    
    fetch(API.piles.delete(id), {
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
                    try {
                        const data = JSON.parse(text);
                        throw new Error(data.message || data || '删除失败');
                    } catch (jsonError) {
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
            
            // 立即刷新数据，确保更新后的数据显示出来
            loadPiles();
            
            // 显示成功消息
            setTimeout(() => {
                showSuccessModal('充电桩删除成功');
            }, 300);
        }
    })
    .catch(error => {
        // 恢复按钮状态
        confirmDeleteBtn.innerHTML = originalText;
        confirmDeleteBtn.disabled = false;
        
        console.error('删除充电桩失败:', error);
        
        // 显示更友好的错误消息
        let errorMessage = '删除充电桩失败';
        
        // 处理具体的错误情况
        if (error.message.includes('端口')) {
            errorMessage = '该充电桩下存在充电端口，请先删除所有端口后再删除充电桩';
        } else if (error.message.includes('订单')) {
            errorMessage = '该充电桩存在关联订单，无法删除';
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
    errorMessageEl.innerHTML = message; // 使用innerHTML而不是textContent，以支持HTML标签
    
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

/**
 * 在充电端口表单内显示错误信息
 * @param {string} message 错误信息
 */
function showPortFormError(message) {
    const errorDiv = document.getElementById('portFormError');
    const errorMessage = document.getElementById('portFormErrorMessage');
    
    if (errorDiv && errorMessage) {
        errorMessage.textContent = message;
        errorDiv.style.display = 'block';
        
        // 滚动到错误信息位置
        errorDiv.scrollIntoView({ behavior: 'smooth', block: 'center' });
    } else {
        // 如果找不到表单内的错误显示元素，则使用模态框
        showErrorModal(message);
    }
}

/**
 * 隐藏充电端口表单内的错误信息
 */
function hidePortFormError() {
    const errorDiv = document.getElementById('portFormError');
    if (errorDiv) {
        errorDiv.style.display = 'none';
    }
}

/**
 * 编辑充电端口
 */
function editPort(id) {
    fetch(API.ports.getById(id))
        .then(response => response.json())
        .then(port => {
            // 填充表单数据
            document.getElementById('portId').value = port.id;
            document.getElementById('portPileId').value = port.pileId;
            document.getElementById('portNo').value = port.portNo;
            document.getElementById('portType').value = port.portType;
            document.getElementById('portStatus').value = port.status;
            document.getElementById('voltage').value = port.voltage || '';
            document.getElementById('currentAmpere').value = port.currentAmpere || '';
            document.getElementById('power').value = port.power || '';
            document.getElementById('temperature').value = port.temperature || '';
            
            // 设置模态框标题
            document.getElementById('portModalLabel').textContent = '编辑充电端口';
            
            // 显示模态框
            portModal.show();
        })
        .catch(error => {
            console.error('获取充电端口数据失败:', error);
            showErrorModal('获取充电端口数据失败，请重试');
        });
}

/**
 * 显示删除充电端口确认对话框
 */
function showDeletePortConfirm(id) {
    document.getElementById('deletePortId').value = id;
    deletePortModal.show();
}

/**
 * 删除充电端口
 */
function deletePort() {
    const id = document.getElementById('deletePortId').value;
    
    if (!id) {
        deletePortModal.hide();
        showErrorModal('充电端口ID无效');
        return;
    }
    
    // 显示加载提示
    const confirmDeleteBtn = document.getElementById('confirmDeletePortBtn');
    const originalText = confirmDeleteBtn.innerHTML;
    confirmDeleteBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> 处理中...';
    confirmDeleteBtn.disabled = true;
    
    fetch(API.ports.delete(id), {
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
            // 尝试读取响应文本
            return response.text()
                .then(text => {
                    try {
                        const data = JSON.parse(text);
                        throw new Error(data.message || data || '删除失败');
                    } catch (jsonError) {
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
            deletePortModal.hide();
            
            // 显示成功消息
            setTimeout(() => {
                showSuccessModal('充电端口删除成功');
                
                // 重新加载端口数据
                loadPortsForPile(currentPileId);
            }, 300);
        }
    })
    .catch(error => {
        // 恢复按钮状态
        confirmDeleteBtn.innerHTML = originalText;
        confirmDeleteBtn.disabled = false;
        
        console.error('删除充电端口失败:', error);
        
        // 显示更友好的错误消息
        let errorMessage = '删除充电端口失败';
        
        // 处理具体的错误情况
        if (error.message.includes('订单')) {
            errorMessage = '该充电端口存在关联订单，无法删除';
        } else if (error.message.includes('权限')) {
            errorMessage = '您没有权限执行此操作';
        } else if (error.message) {
            errorMessage = error.message;
        }
        
        // 先关闭确认对话框，再显示错误信息
        deletePortModal.hide();
        
        // 添加短暂延迟，确保前一个模态框完全关闭
        setTimeout(() => {
            showErrorModal(errorMessage);
        }, 300);
    });
}

/**
 * 保存充电端口（新增或更新）
 */
function savePort() {
    // 隐藏之前的错误信息
    hidePortFormError();
    
    // 获取表单数据
    const id = document.getElementById('portId').value;
    const formData = {
        pileId: document.getElementById('portPileId').value,
        portNo: document.getElementById('portNo').value.toString(), // 作为字符串传递
        portType: parseInt(document.getElementById('portType').value),
        status: parseInt(document.getElementById('portStatus').value)
    };
    
    // 收集可选数据
    const voltage = document.getElementById('voltage').value.trim();
    const currentAmpere = document.getElementById('currentAmpere').value.trim();
    const power = document.getElementById('power').value.trim();
    const temperature = document.getElementById('temperature').value.trim();
    
    // 验证温度在合理范围内
    if (temperature && !isNaN(parseFloat(temperature))) {
        const tempValue = parseFloat(temperature);
        if (tempValue < -50 || tempValue > 200) {
            showPortFormError('温度值超出合理范围，请输入-50°C到200°C之间的数值');
            return;
        }
    }
    
    // 添加非空的可选数据到表单
    if (voltage) formData.voltage = parseFloat(voltage);
    if (currentAmpere) formData.currentAmpere = parseFloat(currentAmpere);
    if (power) formData.power = parseFloat(power);
    if (temperature) formData.temperature = parseFloat(temperature);
    
    // 验证必填字段
    if (!formData.pileId) {
        showPortFormError('请关联充电桩');
        return;
    }
    if (!formData.portNo || formData.portNo < 1) {
        showPortFormError('请输入有效的端口编号');
        return;
    }
    
    // 显示加载提示
    const saveBtn = document.getElementById('savePortBtn');
    const originalText = saveBtn.innerHTML;
    saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> 保存中...';
    saveBtn.disabled = true;
    
    // 确定是新增还是更新
    const isUpdate = id && id.trim() !== '';
    const url = isUpdate ? API.ports.update(id) : API.ports.create;
    const method = isUpdate ? 'PUT' : 'POST';
    
    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => {
                try {
                    const error = JSON.parse(text);
                    // 处理常见错误，提供更友好的错误提示
                    if (text.includes('Parameter value') && text.includes('out of range')) {
                        throw new Error('输入的数值超出系统允许的范围，请检查并调整输入值');
                    }
                    if (error.message && error.message.includes('entity changes')) {
                        throw new Error('保存数据时出错，请检查输入值是否在合理范围内');
                    }
                    throw new Error(error.message || error.title || '保存失败');
                } catch (e) {
                    // 尝试识别错误类型并提供更友好的提示
                    if (text.includes('Parameter value') && text.includes('out of range')) {
                        throw new Error('输入的数值超出系统允许的范围，请检查并调整输入值');
                    }
                    throw new Error(text || response.statusText || '保存失败');
                }
            });
        }
        
        return response.json();
    })
    .then(data => {
        // 恢复按钮状态
        saveBtn.innerHTML = originalText;
        saveBtn.disabled = false;
        
        // 关闭模态框
        portModal.hide();
        
        // 短暂延迟后显示成功消息并刷新数据
        setTimeout(() => {
            showSuccessModal(`充电端口${isUpdate ? '更新' : '创建'}成功`);
            loadPortsForPile(currentPileId);
        }, 300);
    })
    .catch(error => {
        // 恢复按钮状态
        saveBtn.innerHTML = originalText;
        saveBtn.disabled = false;
        
        console.error('保存充电端口失败:', error);
        // 在表单内显示错误，而不是弹出错误模态框
        showPortFormError(error.message || '保存充电端口失败');
    });
}
