/**
 * 公共组件和导航管理脚本
 * 负责加载通用组件（导航栏、侧边栏、页脚）并处理当前页面的导航高亮
 */

// 在DOM加载完成后执行
document.addEventListener('DOMContentLoaded', function() {
    // 加载共享组件
    loadComponents().then(() => {
        // 组件加载完成后，设置当前页面的菜单高亮
        highlightCurrentPage();
    });
});

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
