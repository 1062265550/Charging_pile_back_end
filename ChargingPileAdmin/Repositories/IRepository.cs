using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 通用仓储接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>实体集合</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter">过滤条件</param>
        /// <returns>实体集合</returns>
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// 根据条件获取实体，并包含导航属性
        /// </summary>
        /// <param name="filter">过滤条件</param>
        /// <param name="includeProperties">导航属性</param>
        /// <returns>实体集合</returns>
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>实体</returns>
        Task<T> GetByIdAsync(object id);

        /// <summary>
        /// 根据ID获取实体，并包含导航属性
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <param name="includeProperties">导航属性</param>
        /// <returns>实体</returns>
        Task<T> GetByIdAsync(object id, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity">实体</param>
        Task AddAsync(T entity);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">实体</param>
        void Update(T entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体</param>
        void Delete(T entity);

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体ID</param>
        Task DeleteAsync(object id);

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="filter">过滤条件</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderBy">排序表达式</param>
        /// <param name="includeProperties">导航属性</param>
        /// <returns>分页数据</returns>
        Task<(IEnumerable<T> Items, int TotalCount, int TotalPages)> GetPagedAsync(
            Expression<Func<T, bool>> filter,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// 保存更改
        /// </summary>
        Task SaveChangesAsync();
    }
}
