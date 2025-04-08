using ChargingPileAdmin.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 订单服务接口
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <returns>订单列表</returns>
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <returns>订单详情</returns>
        Task<OrderDto?> GetOrderByIdAsync(string id);

        /// <summary>
        /// 获取订单分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="userId">用户ID，可选</param>
        /// <param name="pileId">充电桩ID，可选</param>
        /// <param name="status">订单状态，可选</param>
        /// <param name="startDate">开始日期，可选</param>
        /// <param name="endDate">结束日期，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>订单分页列表</returns>
        Task<PagedResponseDto<OrderDto>> GetOrdersPagedAsync(
            int pageNumber, 
            int pageSize, 
            int? userId = null, 
            string? pileId = null, 
            int? status = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            string? keyword = null);

        /// <summary>
        /// 获取用户订单列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>订单列表</returns>
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);

        /// <summary>
        /// 获取充电桩订单列表
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>订单列表</returns>
        Task<IEnumerable<OrderDto>> GetOrdersByPileIdAsync(string pileId);

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="dto">创建订单请求</param>
        /// <returns>创建的订单ID</returns>
        Task<string> CreateOrderAsync(CreateOrderDto dto);

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="dto">更新订单状态请求</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto);

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="dto">订单支付请求</param>
        /// <returns>是否成功</returns>
        Task<bool> PayOrderAsync(string id, OrderPaymentDto dto);

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <returns>是否成功</returns>
        Task<bool> CancelOrderAsync(string id);

        /// <summary>
        /// 完成订单
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="powerConsumption">充电电量(kWh)</param>
        /// <param name="chargingTime">充电时长(秒)</param>
        /// <returns>是否成功</returns>
        Task<bool> CompleteOrderAsync(string id, decimal powerConsumption, int chargingTime);

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        /// <param name="startDate">开始日期，可选</param>
        /// <param name="endDate">结束日期，可选</param>
        /// <returns>订单统计数据</returns>
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
