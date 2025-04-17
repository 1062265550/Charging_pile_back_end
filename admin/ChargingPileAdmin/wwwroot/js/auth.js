/**
 * 充电桩管理系统 - 认证模块
 * 处理用户登录、登出和身份验证
 */

// 页面加载完成后执行
$(document).ready(function() {
    // 检查用户是否已登录
    checkAuth();
    
    // 登录表单提交
    $('#loginForm').on('submit', function(e) {
        e.preventDefault();
        login();
    });
});

/**
 * 处理用户登录
 */
function login() {
    // 获取表单数据
    const username = $('#username').val().trim();
    const password = $('#password').val().trim();
    const rememberMe = $('#rememberMe').is(':checked');
    
    // 验证表单
    if (!username || !password) {
        showLoginError('请输入用户名和密码');
        return;
    }
    
    // 禁用登录按钮，显示加载状态
    const loginBtn = $('#loginForm button[type="submit"]');
    loginBtn.prop('disabled', true);
    loginBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>登录中...');
    
    // 模拟API请求
    // 在实际应用中，这里应该调用后端API进行身份验证
    setTimeout(function() {
        // 默认管理员账号：admin/admin123
        if (username === 'admin' && password === 'admin123') {
            // 登录成功
            const authData = {
                username: username,
                role: 'admin',
                token: 'simulated-jwt-token-' + Date.now(),
                expires: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString() // 24小时后过期
            };
            
            // 保存认证信息
            if (rememberMe) {
                // 如果"记住我"被选中，使用localStorage（不会过期）
                localStorage.setItem('authData', JSON.stringify(authData));
            } else {
                // 否则使用sessionStorage（浏览器关闭后过期）
                sessionStorage.setItem('authData', JSON.stringify(authData));
            }
            
            // 重定向到首页
            window.location.href = 'index.html';
        } else {
            // 登录失败
            showLoginError('用户名或密码错误');
            
            // 恢复登录按钮状态
            loginBtn.prop('disabled', false);
            loginBtn.html('<i class="fas fa-sign-in-alt me-2"></i>登录');
        }
    }, 1000); // 模拟网络延迟
}

/**
 * 检查用户是否已登录
 * 在需要身份验证的页面上调用
 */
function checkAuth() {
    const currentPath = window.location.pathname;
    const isLoginPage = currentPath.endsWith('login.html');
    
    // 从存储中获取认证数据
    const authData = getAuthData();
    
    if (authData) {
        // 已登录
        if (isLoginPage) {
            // 如果已登录并尝试访问登录页面，重定向到首页
            window.location.href = 'index.html';
        }
    } else {
        // 未登录
        if (!isLoginPage) {
            // 如果未登录并尝试访问需要身份验证的页面，重定向到登录页面
            window.location.href = 'login.html';
        }
    }
}

/**
 * 获取当前用户认证数据
 * @returns {Object|null} 认证数据或null
 */
function getAuthData() {
    // 先尝试从sessionStorage获取
    let authData = sessionStorage.getItem('authData');
    
    // 如果sessionStorage中没有，再尝试从localStorage获取
    if (!authData) {
        authData = localStorage.getItem('authData');
    }
    
    if (authData) {
        try {
            const parsedData = JSON.parse(authData);
            
            // 检查令牌是否过期
            if (parsedData.expires && new Date(parsedData.expires) > new Date()) {
                return parsedData;
            } else {
                // 令牌已过期，清除存储
                logout();
                return null;
            }
        } catch (e) {
            console.error('解析认证数据失败:', e);
            return null;
        }
    }
    
    return null;
}

/**
 * 获取当前登录用户名
 * @returns {string} 用户名或空字符串
 */
function getCurrentUsername() {
    const authData = getAuthData();
    return authData ? authData.username : '';
}

/**
 * 用户登出
 */
function logout() {
    // 清除存储的认证数据
    localStorage.removeItem('authData');
    sessionStorage.removeItem('authData');
    
    // 重定向到登录页面
    window.location.href = 'login.html';
}

/**
 * 显示登录错误信息
 * @param {string} message 错误信息
 */
function showLoginError(message) {
    const alertEl = $('#loginAlert');
    $('#loginAlertMessage').text(message);
    alertEl.removeClass('d-none');
    
    // 5秒后自动隐藏错误提示
    setTimeout(function() {
        alertEl.addClass('d-none');
    }, 5000);
}
