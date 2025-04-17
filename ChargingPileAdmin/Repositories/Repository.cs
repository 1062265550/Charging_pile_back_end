using ChargingPileAdmin.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 通用仓储实现
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>实体集合</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter">过滤条件</param>
        /// <returns>实体集合</returns>
        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
        }

        /// <summary>
        /// 根据条件获取实体，并包含导航属性
        /// </summary>
        /// <param name="filter">过滤条件</param>
        /// <param name="includeProperties">导航属性</param>
        /// <returns>实体集合</returns>
        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns>实体</returns>
        public async Task<T> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// 根据ID获取实体，并包含导航属性
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <param name="includeProperties">导航属性</param>
        /// <returns>实体</returns>
        public async Task<T> GetByIdAsync(object id, params Expression<Func<T, object>>[] includeProperties)
        {
            // 获取主键属性名
            var keyName = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();

            // 构建参数表达式
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, keyName);
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

            IQueryable<T> query = _dbSet;

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }

            return await query.FirstOrDefaultAsync(lambda);
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity">实体</param>
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity">实体</param>
        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体</param>
        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体ID</param>
        public async Task DeleteAsync(object id)
        {
            T entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                Delete(entity);
            }
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="filter">过滤条件</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="orderBy">排序表达式</param>
        /// <param name="includeProperties">导航属性</param>
        /// <returns>分页数据</returns>
        public async Task<(IEnumerable<T> Items, int TotalCount, int TotalPages)> GetPagedAsync(
            Expression<Func<T, bool>> filter,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }

            // 计算总记录数和总页数
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 排序
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // 分页
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount, totalPages);
        }

        /// <summary>
        /// 保存更改
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
