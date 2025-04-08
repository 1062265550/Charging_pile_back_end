using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChargingPileAdmin.Services;
using ChargingPileAdmin.Dtos;

namespace ChargingPileAdmin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// 获取所有统计数据
        /// </summary>
        /// <returns>所有统计数据</returns>
        [HttpGet]
        public async Task<ActionResult<StatisticsDto>> GetAllStatistics()
        {
            var statistics = await _statisticsService.GetAllStatisticsAsync();
            return Ok(statistics);
        }

        /// <summary>
        /// 获取充电桩统计数据
        /// </summary>
        /// <returns>充电桩统计数据</returns>
        [HttpGet("piles")]
        public async Task<ActionResult<ChargingPileStatisticsDto>> GetPileStatistics()
        {
            var statistics = await _statisticsService.GetPileStatisticsAsync();
            return Ok(statistics);
        }

        /// <summary>
        /// 获取充电口统计数据
        /// </summary>
        /// <returns>充电口统计数据</returns>
        [HttpGet("ports")]
        public async Task<ActionResult<ChargingPortStatisticsDto>> GetPortStatistics()
        {
            var statistics = await _statisticsService.GetPortStatisticsAsync();
            return Ok(statistics);
        }

        /// <summary>
        /// 获取用户统计数据
        /// </summary>
        /// <returns>用户统计数据</returns>
        [HttpGet("users")]
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
        {
            var statistics = await _statisticsService.GetUserStatisticsAsync();
            return Ok(statistics);
        }

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        /// <returns>订单统计数据</returns>
        [HttpGet("orders")]
        public async Task<ActionResult<StatOrderDto>> GetOrderStatistics()
        {
            var statistics = await _statisticsService.GetOrderStatisticsAsync();
            return Ok(statistics);
        }
    }
}
