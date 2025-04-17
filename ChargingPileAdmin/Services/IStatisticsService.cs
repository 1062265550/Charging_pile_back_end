using System;
using System.Threading.Tasks;
using ChargingPileAdmin.Dtos;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 统计数据服务接口
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// 获取所有统计数据
        /// </summary>
        /// <returns>统计数据DTO</returns>
        Task<StatisticsDto> GetAllStatisticsAsync();

        /// <summary>
        /// 获取充电桩统计数据
        /// </summary>
        /// <returns>充电桩统计数据DTO</returns>
        Task<ChargingPileStatisticsDto> GetPileStatisticsAsync();

        /// <summary>
        /// 获取充电口统计数据
        /// </summary>
        /// <returns>充电口统计数据DTO</returns>
        Task<ChargingPortStatisticsDto> GetPortStatisticsAsync();

        /// <summary>
        /// 获取用户统计数据
        /// </summary>
        /// <returns>用户统计数据DTO</returns>
        Task<UserStatisticsDto> GetUserStatisticsAsync();

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        /// <returns>订单统计数据DTO</returns>
        Task<StatOrderDto> GetOrderStatisticsAsync();
    }
}
