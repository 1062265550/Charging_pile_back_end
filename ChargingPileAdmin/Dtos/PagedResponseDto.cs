using System.Collections.Generic;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 分页响应数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedResponseDto<T>
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevious => PageNumber > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext => PageNumber < TotalPages;

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Items { get; set; }
    }
}
