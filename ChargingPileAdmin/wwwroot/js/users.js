/**
 * 充电桩管理平台 - 用户管理模块
 * 提供用户数据的增删改查、充值余额等功能
 * 与后端API交互，处理用户相关业务逻辑
 */

// 全局变量
let currentPage = 1;
const pageSize = 10;
let totalPages = 0;
let keyword = '';
let statusFilter = '';

// 页面加载完成后执行
$(document).ready(function() {
    // 初始化加载用户数据
    loadUsersData();
    
    // 加载用户统计信息
    loadUserStatistics();
    
    // 初始化事件监听
    initEventListeners();
    
    // 初始化无障碍支持
    initAccessibility();
});

/**
 * 初始化所有事件监听器
 */
function initEventListeners() {
    // 搜索按钮点击事件
    $('#searchButton').on('click', function() {
        keyword = $('#keyword').val();
        statusFilter = $('#status').val();
        currentPage = 1;
        loadUsersData();
    });
    
    // 重置搜索按钮点击事件
    $('#resetSearch').on('click', function() {
        $('#keyword').val('');
        $('#status').val('');
        keyword = '';
        statusFilter = '';
        currentPage = 1;
        loadUsersData();
    });
    
    // 刷新数据按钮点击事件
    $('#refreshData').on('click', function() {
        loadUsersData();
        loadUserStatistics();
    });
    
    // 新增用户按钮点击事件
    $('#createUserBtn').on('click', function() {
        $('#createUserForm')[0].reset();
        $('#createUserModal').modal('show');
    });
    
    // 确认创建用户点击事件
    $('#confirmCreateUser').on('click', function() {
        if (validateCreateUserForm()) {
            createUser();
        }
    });
    
    // 确认编辑用户点击事件
    $('#confirmUpdateUser').on('click', function() {
        if (validateUpdateUserForm()) {
            updateUser();
        }
    });
    
    // 确认充值点击事件
    $('#confirmRecharge').on('click', function() {
        if (validateRechargeForm()) {
            rechargeBalance();
        }
    });
    
    // 确认删除用户点击事件
    $('#confirmDeleteUser').on('click', function() {
        deleteUser();
    });
    
    // 导出数据按钮点击事件
    $('#exportData').on('click', function() {
        exportUserData();
    });
}

/**
 * 初始化无障碍支持
 * 使用现代Web标准inert属性代替aria-hidden
 */
function initAccessibility() {
    // 监听模态框显示事件，为主内容区域添加inert属性
    $('.modal').on('show.bs.modal', function () {
        // 将主内容区域设为inert
        $('.container-fluid').attr('inert', '');
    });
    
    // 监听模态框隐藏事件，移除主内容区域的inert属性
    $('.modal').on('hidden.bs.modal', function () {
        // 移除主内容区域的inert属性
        $('.container-fluid').removeAttr('inert');
    });
    
    // 使用MutationObserver监听动态加载的内容
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList') {
                // 为动态加载的内容添加适当的ARIA属性
                const addedNodes = mutation.addedNodes;
                for (let i = 0; i < addedNodes.length; i++) {
                    if (addedNodes[i].nodeType === 1) { // 元素节点
                        enhanceNodeAccessibility(addedNodes[i]);
                    }
                }
            }
        });
    });
    
    // 开始观察
    observer.observe(document.body, { childList: true, subtree: true });
}

/**
 * 增强节点的无障碍性
 * @param {Node} node - DOM节点
 */
function enhanceNodeAccessibility(node) {
    // 为表格添加适当的ARIA属性
    if (node.tagName === 'TABLE') {
        node.setAttribute('role', 'grid');
    }
    
    // 为按钮添加适当的ARIA标签
    if (node.tagName === 'BUTTON' && !node.getAttribute('aria-label')) {
        const text = node.textContent.trim();
        if (text) {
            node.setAttribute('aria-label', text);
        }
    }
}

/**
 * 加载用户列表数据
 */
function loadUsersData() {
    // 显示加载指示器
    $('#userTableBody').html('<tr><td colspan="10" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">加载中...</span></div></td></tr>');
    
    // 构建API请求URL
    let url = `/api/Users?pageNumber=${currentPage}&pageSize=${pageSize}`;
    if (keyword) {
        url += `&keyword=${encodeURIComponent(keyword)}`;
    }
    if (statusFilter) {
        url += `&status=${statusFilter}`;
    }
    
    // 发送AJAX请求
    $.ajax({
        url: url,
        type: 'GET',
        success: function(response) {
            if (response) {
                let data = {
                    items: Array.isArray(response) ? response : (response.items || []),
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    totalItems: Array.isArray(response) ? response.length : (response.totalCount || 0),
                    totalPages: Array.isArray(response) ? Math.ceil(response.length / pageSize) : (response.totalPages || 1)
                };
                renderUserTable(data);
                renderPagination(data);
                updateStatistics(data);
            } else {
                showError('获取用户数据失败：返回数据为空');
            }
        },
        error: function(xhr) {
            showError('获取用户数据失败：' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 加载用户统计数据
 */
function loadUserStatistics() {
    // 由于没有专门的统计接口，直接使用用户列表接口
    $.ajax({
        url: '/api/Users',
        type: 'GET',
        success: function(response) {
            if (response) {
                const users = Array.isArray(response) ? response : (response.items || []);
                const totalUsers = users.length;
                const activeUsers = users.filter(u => u.status !== 0).length;
                const inactiveUsers = users.filter(u => u.status === 0).length;
                
                // 计算今日新增用户
                const today = new Date();
                today.setHours(0, 0, 0, 0);
                const todayTs = today.getTime();
                const newUsersToday = users.filter(u => {
                    const registerTime = u.registerTime || u.last_login_time || u.update_time;
                    if (!registerTime) return false;
                    const registerDate = new Date(registerTime);
                    return registerDate.getTime() >= todayTs;
                }).length;
                
                $('#totalUsers').text(totalUsers || 0);
                $('#activeUsers').text(activeUsers || 0);
                $('#inactiveUsers').text(inactiveUsers || 0);
                $('#newUsersToday').text(newUsersToday || 0);
            }
        },
        error: function(xhr) {
            console.error('获取用户统计数据失败：', (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 根据API响应渲染用户表格
 * @param {Object} data - 分页响应数据
 */
function renderUserTable(data) {
    let html = '';
    
    if (data.items.length === 0) {
        html = '<tr><td colspan="10" class="text-center">暂无用户数据</td></tr>';
    } else {
        data.items.forEach(function(user) {
            html += `
                <tr>
                    <td>${user.id}</td>
                    <td><img src="${user.avatar || 'images/avatar-placeholder.png'}" alt="头像" class="rounded-circle" width="40" height="40"></td>
                    <td>${user.nickname || user.nickName || '未设置'}</td>
                    <td>${user.phoneNumber || user.phone || user.open_id || '未设置'}</td>
                    <td>${user.email || '未设置'}</td>
                    <td>¥${(user.balance || 0).toFixed(2)}</td>
                    <td>${formatDate(user.registerTime || user.last_login_time || user.update_time)}</td>
                    <td>${formatDate(user.lastLoginTime || user.last_login_time)}</td>
                    <td><span class="badge ${user.status !== 0 ? 'bg-success' : 'bg-danger'}">${user.status !== 0 ? '正常' : '禁用'}</span></td>
                    <td>
                        <div class="btn-group btn-group-sm" role="group" aria-label="用户操作">
                            <button type="button" class="btn btn-primary" onclick="viewUserDetail(${user.id})">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="btn btn-info" onclick="showUpdateUserModal(${user.id})">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-warning" onclick="showRechargeModal(${user.id})">
                                <i class="fas fa-wallet"></i>
                            </button>
                            <button type="button" class="btn btn-danger" onclick="showDeleteUserModal(${user.id}, '${user.nickname || user.nickName || '未命名用户'}')">
                                <i class="fas fa-trash-alt"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        });
    }
    
    $('#userTableBody').html(html);
}

/**
 * 渲染分页控件
 * @param {Object} data - 分页响应数据
 */
function renderPagination(data) {
    totalPages = data.totalPages;
    
    if (totalPages <= 1) {
        $('#pagination').empty();
        return;
    }
    
    let html = '';
    
    // 上一页按钮
    html += `
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" aria-label="上一页" ${currentPage > 1 ? 'onclick="goToPage(' + (currentPage - 1) + '); return false;"' : ''}>
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
                <a class="page-link" href="#" onclick="goToPage(${i}); return false;">${i}</a>
            </li>
        `;
    }
    
    // 下一页按钮
    html += `
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" aria-label="下一页" ${currentPage < totalPages ? 'onclick="goToPage(' + (currentPage + 1) + '); return false;"' : ''}>
                <span aria-hidden="true">&raquo;</span>
            </a>
        </li>
    `;
    
    $('#pagination').html(html);
}

/**
 * 跳转到指定页码
 * @param {number} page - 目标页码
 */
function goToPage(page) {
    if (page < 1 || page > totalPages) {
        return;
    }
    
    currentPage = page;
    loadUsersData();
}

/**
 * 查看用户详情
 * @param {number} userId - 用户ID
 */
function viewUserDetail(userId) {
    $.ajax({
        url: `/api/Users/${userId}`,
        type: 'GET',
        success: function(response) {
            if (response) {
                const user = response.data || response;
                
                $('#detailId').text(user.id);
                $('#detailNickname').text(user.nickname || user.nickName || '未设置');
                $('#detailAvatar').attr('src', user.avatar || 'images/avatar-placeholder.png');
                $('#detailPhone').text(user.phoneNumber || user.phone || user.open_id || '未设置');
                $('#detailEmail').text(user.email || '未设置');
                $('#detailBalance').text('¥' + (user.balance || 0).toFixed(2));
                $('#detailRegisterTime').text(formatDate(user.registerTime || user.last_login_time || user.update_time));
                $('#detailLastLoginTime').text(formatDate(user.lastLoginTime || user.last_login_time));
                $('#detailOpenId').text(user.openId || user.open_id || '未绑定');
                
                const statusHtml = user.status !== 0
                    ? '<span class="badge bg-success">正常</span>' 
                    : '<span class="badge bg-danger">禁用</span>';
                $('#detailStatus').html(statusHtml);
                
                // 设置充值和编辑按钮的数据
                $('#rechargeBalanceBtn').data('userId', user.id);
                $('#updateUserBtn').data('userId', user.id);
                
                $('#userDetailModal').modal('show');
            } else {
                showError('获取用户详情失败：返回数据为空');
            }
        },
        error: function(xhr) {
            showError('获取用户详情失败：' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 显示编辑用户模态框
 * @param {number} userId - 用户ID
 */
function showUpdateUserModal(userId) {
    $.ajax({
        url: `/api/Users/${userId}`,
        type: 'GET',
        success: function(response) {
            if (response) {
                const user = response.data || response;
                
                $('#updateUserId').val(user.id);
                $('#updateNickname').val(user.nickname || user.nickName);
                $('#updatePhone').val(user.phoneNumber || user.phone || user.open_id);
                $('#updateEmail').val(user.email);
                $('#updateAvatar').val(user.avatar);
                $('#updateStatus').val(user.status !== 0 ? '1' : '0');
                
                $('#updateUserModal').modal('show');
            } else {
                showError('获取用户数据失败：返回数据为空');
            }
        },
        error: function(xhr) {
            showError('获取用户数据失败：' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 显示充值余额模态框
 * @param {number} userId - 用户ID
 */
function showRechargeModal(userId) {
    $('#rechargeUserId').val(userId);
    $('#rechargeForm')[0].reset();
    $('#rechargeModal').modal('show');
}

/**
 * 显示删除用户确认模态框
 * @param {number} userId - 用户ID
 * @param {string} userName - 用户名称
 */
function showDeleteUserModal(userId, userName) {
    $('#deleteUserId').val(userId);
    $('#deleteUserName').text(userName);
    $('#deleteUserModal').modal('show');
}

/**
 * 创建新用户
 */
function createUser() {
    const userData = {
        nickname: $('#createNickname').val(),
        phone: $('#createPhone').val(),
        email: $('#createEmail').val(),
        avatar: $('#createAvatar').val(),
        password: $('#createPassword').val()
    };
    
    $.ajax({
        url: '/api/Users',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(userData),
        success: function(response) {
            if (response && (response.success !== false)) {
                $('#createUserModal').modal('hide');
                showSuccess('用户创建成功');
                loadUsersData();
                loadUserStatistics();
            } else {
                showError('创建用户失败：' + (response?.message || '未知错误'));
            }
        },
        error: function(xhr) {
            showError('创建用户失败：' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 更新用户信息
 */
function updateUser() {
    const userId = $('#updateUserId').val();
    const userData = {
        id: userId,
        nickname: $('#updateNickname').val(),
        phone: $('#updatePhone').val(),
        email: $('#updateEmail').val(),
        avatar: $('#updateAvatar').val(),
        status: $('#updateStatus').val() === '1' ? 1 : 0
    };
    
    $.ajax({
        url: `/api/Users/${userId}`,
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify(userData),
        success: function(response) {
            if (response && (response.success !== false)) {
                $('#updateUserModal').modal('hide');
                showSuccess('用户信息更新成功');
                loadUsersData();
            } else {
                showError('更新用户信息失败：' + (response?.message || '未知错误'));
            }
        },
        error: function(xhr) {
            showError('更新用户信息失败：' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 充值用户余额
 */
function rechargeBalance() {
    const userId = $('#rechargeUserId').val();
    const rechargeData = {
        userId: userId,
        amount: $('#rechargeAmount').val(),
        paymentMethod: $('#rechargeMethod').val(),
        transactionId: $('#rechargeTransactionId').val(),
        remark: $('#rechargeRemark').val()
    };
    
    $.ajax({
        url: `/api/Users/${userId}/recharge`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(rechargeData),
        success: function(response) {
            if (response && (response.success !== false)) {
                $('#rechargeModal').modal('hide');
                showSuccess('充值成功');
                loadUsersData();
            } else {
                showError('充值失败：' + (response?.message || '未知错误'));
            }
        },
        error: function(xhr) {
            showError('充值失败：' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 删除用户
 */
function deleteUser() {
    const userId = $('#deleteUserId').val();
    
    $.ajax({
        url: `/api/Users/${userId}`,
        type: 'DELETE',
        success: function(response) {
            if (response && (response.success !== false)) {
                $('#deleteUserModal').modal('hide');
                showSuccess('用户删除成功');
                loadUsersData();
                loadUserStatistics();
            } else {
                showError('删除用户失败：' + (response?.message || '未知错误'));
            }
        },
        error: function(xhr) {
            showError('删除用户失败：' + (xhr.responseJSON?.message || xhr.statusText));
        }
    });
}

/**
 * 导出用户数据
 */
function exportUserData() {
    let url = '/api/Users/export';
    if (keyword) {
        url += `?keyword=${encodeURIComponent(keyword)}`;
    }
    if (statusFilter) {
        url += `${keyword ? '&' : '?'}status=${statusFilter}`;
    }
    
    window.location.href = url;
}

/**
 * 验证创建用户表单
 * @returns {boolean} 验证结果
 */
function validateCreateUserForm() {
    // 表单验证
    const form = $('#createUserForm')[0];
    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }
    
    // 检查密码一致性
    const password = $('#createPassword').val();
    const confirmPassword = $('#createConfirmPassword').val();
    
    if (password !== confirmPassword) {
        showError('两次输入的密码不一致');
        return false;
    }
    
    return true;
}

/**
 * 验证更新用户表单
 * @returns {boolean} 验证结果
 */
function validateUpdateUserForm() {
    const form = $('#updateUserForm')[0];
    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }
    
    return true;
}

/**
 * 验证充值表单
 * @returns {boolean} 验证结果
 */
function validateRechargeForm() {
    const form = $('#rechargeForm')[0];
    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }
    
    return true;
}

/**
 * 更新统计数据
 * @param {Object} data - 分页响应数据
 */
function updateStatistics(data) {
    if (data && data.totalItems !== undefined) {
        $('#totalUsers').text(data.totalItems);
    }
}

/**
 * 显示成功提示
 * @param {string} message - 提示信息
 */
function showSuccess(message) {
    $('#successMessage').text(message);
    $('#successModal').modal('show');
}

/**
 * 显示错误提示
 * @param {string} message - 错误信息
 */
function showError(message) {
    $('#errorMessage').text(message);
    $('#errorModal').modal('show');
}

/**
 * 格式化日期时间
 * @param {string} dateTimeStr - 日期时间字符串
 * @returns {string} 格式化后的日期时间
 */
function formatDate(dateTimeStr) {
    if (!dateTimeStr) {
        return '暂无记录';
    }
    
    const date = new Date(dateTimeStr);
    if (isNaN(date.getTime())) {
        return '无效日期';
    }
    
    return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
}

// 用户详情模态框中的按钮事件
$('#rechargeBalanceBtn').on('click', function() {
    const userId = $(this).data('userId');
    $('#userDetailModal').modal('hide');
    showRechargeModal(userId);
});

$('#updateUserBtn').on('click', function() {
    const userId = $(this).data('userId');
    $('#userDetailModal').modal('hide');
    showUpdateUserModal(userId);
});
