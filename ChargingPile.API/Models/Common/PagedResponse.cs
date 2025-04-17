using System.Collections.Generic;

namespace ChargingPile.API.Models.Common
{
    /// <summary>
    /// 分页响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> List { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageNum { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int Pages { get; set; }

        /// <summary>
        /// 创建分页响应
        /// </summary>
        /// <param name="list">数据列表</param>
        /// <param name="total">总记录数</param>
        /// <param name="pageNum">当前页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>分页响应</returns>
        public static PagedResponse<T> Create(List<T> list, int total, int pageNum, int pageSize)
        {
            return new PagedResponse<T>
            {
                List = list,
                Total = total,
                PageNum = pageNum,
                PageSize = pageSize,
                Pages = (total + pageSize - 1) / pageSize
            };
        }
    }
}
