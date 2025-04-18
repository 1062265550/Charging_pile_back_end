/**
 * 公共组件和导航管理脚本
 * 负责加载通用组件（导航栏、侧边栏、页脚）并处理当前页面的导航高亮
 */

// 在DOM加载完成后执行
document.addEventListener('DOMContentLoaded', function() {
    // 检查用户是否已登录
    checkUserAuthentication();

    // 加载共享组件
    loadComponents().then(() => {
        // 组件加载完成后，设置当前页面的菜单高亮
        highlightCurrentPage();

        // 确保侧边栏容器的背景色能够延伸到整个页面
        ensureSidebarBackground();
    });
});

/**
 * 确保侧边栏容器的背景色能够延伸到整个页面
 */
function ensureSidebarBackground() {
    const sidebarContainer = document.getElementById('sidebar-container');
    if (sidebarContainer) {
        // 设置侧边栏容器的最小高度为页面高度
        const updateSidebarHeight = () => {
            const pageHeight = Math.max(
                document.body.scrollHeight,
                document.body.offsetHeight,
                document.documentElement.clientHeight,
                document.documentElement.scrollHeight,
                document.documentElement.offsetHeight
            );
            sidebarContainer.style.minHeight = pageHeight + 'px';
        };

        // 初始调用
        updateSidebarHeight();

        // 在窗口调整大小时重新计算
        window.addEventListener('resize', updateSidebarHeight);

        // 在页面内容变化时重新计算
        const observer = new MutationObserver(updateSidebarHeight);
        observer.observe(document.body, { childList: true, subtree: true });
    }
}

/**
 * 检查用户是否已登录，如果未登录则重定向到登录页面
 */
function checkUserAuthentication() {
    const currentPath = window.location.pathname;
    const isLoginPage = currentPath.endsWith('login.html');

    // 如果当前页面不是登录页面，则检查用户是否已登录
    if (!isLoginPage) {
        // 尝试加载 auth.js
        if (typeof getAuthData !== 'function') {
            // 动态加载 auth.js
            const script = document.createElement('script');
            script.src = 'js/auth.js';
            script.onload = function() {
                // auth.js 加载完成后检查用户是否已登录
                if (typeof getAuthData === 'function') {
                    const authData = getAuthData();
                    if (!authData) {
                        // 如果用户未登录，重定向到登录页面
                        window.location.href = 'login.html';
                    }
                }
            };
            document.head.appendChild(script);
        } else {
            // 如果 auth.js 已经加载，直接检查用户是否已登录
            const authData = getAuthData();
            if (!authData) {
                // 如果用户未登录，重定向到登录页面
                window.location.href = 'login.html';
            }
        }
    }
}

/**
 * 加载所有共享组件（导航栏、侧边栏、页脚）
 * @returns {Promise} 所有组件加载完成的Promise
 */
async function loadComponents() {
    try {
        // 同时加载所有组件
        await Promise.all([
            loadComponent('#header-container', 'components/header.html'),
            loadComponent('#sidebar-container', 'components/sidebar.html'),
            loadComponent('#footer-container', 'components/footer.html')
        ]);
        console.log('所有组件加载完成');
    } catch (error) {
        console.error('组件加载失败:', error);
    }
}

/**
 * 加载单个组件到指定容器
 * @param {string} selector - 组件容器选择器
 * @param {string} url - 组件HTML文件路径
 * @returns {Promise} 组件加载完成的Promise
 */
function loadComponent(selector, url) {
    return new Promise((resolve, reject) => {
        const container = document.querySelector(selector);
        if (!container) {
            console.warn(`找不到组件容器: ${selector}`);
            resolve(); // 即使容器不存在也继续执行
            return;
        }

        // 添加时间戳参数防止缓存
        const cacheBuster = `?_=${new Date().getTime()}`;
        const urlWithCacheBuster = url + cacheBuster;

        fetch(urlWithCacheBuster)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP错误，状态码: ${response.status}`);
                }
                return response.text();
            })
            .then(html => {
                // 替换容器内容，这里容器是专门为组件准备的空div
                container.innerHTML = html;
                resolve();
            })
            .catch(error => {
                console.error(`加载组件 ${url} 失败:`, error);
                reject(error);
            });
    });
}

/**
 * 设置当前页面在导航和侧边栏中的高亮
 */
function highlightCurrentPage() {
    // 从URL获取当前页面标识符
    const currentPage = getCurrentPageIdentifier();

    // 在顶部导航栏中设置当前页面的高亮
    document.querySelectorAll('#navbarNav .nav-link').forEach(link => {
        const linkPage = link.getAttribute('data-page');
        if (linkPage === currentPage) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });

    // 在侧边栏中设置当前页面的高亮
    document.querySelectorAll('.sidebar-nav-link').forEach(link => {
        const linkPage = link.getAttribute('data-page');
        if (linkPage === currentPage) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
}

/**
 * 根据当前URL获取页面标识符
 * @returns {string} 页面标识符
 */
function getCurrentPageIdentifier() {
    // 从URL路径中获取当前文件名
    const path = window.location.pathname;
    const filename = path.substring(path.lastIndexOf('/') + 1);

    // 根据文件名返回对应的标识符（去掉.html后缀）
    if (!filename || filename === '' || filename === 'index.html') {
        return 'index';
    } else {
        return filename.replace('.html', '');
    }
}
