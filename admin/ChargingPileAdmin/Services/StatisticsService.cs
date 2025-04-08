using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChargingPileAdmin.Data;
using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Models;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 统计数据服务实现
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取所有统计数据
        /// </summary>
        public async Task<StatisticsDto> GetAllStatisticsAsync()
        {
            var statistics = new StatisticsDto
            {
                PileStatistics = await GetPileStatisticsAsync(),
                PortStatistics = await GetPortStatisticsAsync(),
                UserStatistics = await GetUserStatisticsAsync(),
                OrderStatistics = await GetOrderStatisticsAsync()
            };

            return statistics;
        }

        /// <summary>
        /// 获取充电桩统计数据
        /// </summary>
        public async Task<ChargingPileStatisticsDto> GetPileStatisticsAsync()
        {
            var piles = await _context.ChargingPiles.ToListAsync();
            
            var result = new ChargingPileStatisticsDto
            {
                TotalCount = piles.Count,
                OnlineCount = piles.Count(p => p.OnlineStatus == 1), // 在线
                OfflineCount = piles.Count(p => p.OnlineStatus == 0), // 离线
                FaultCount = piles.Count(p => p.Status == 3), // 故障
                DcFastCount = piles.Count(p => p.PileType == 1), // 直流快充
                AcSlowCount = piles.Count(p => p.PileType == 2)  // 交流慢充
            };

            return result;
        }

        /// <summary>
        /// 获取充电口统计数据
        /// </summary>
        public async Task<ChargingPortStatisticsDto> GetPortStatisticsAsync()
        {
            var ports = await _context.ChargingPorts.ToListAsync();
            
            var result = new ChargingPortStatisticsDto
            {
                TotalCount = ports.Count,
                IdleCount = ports.Count(p => p.Status == 1), // 空闲
                InUseCount = ports.Count(p => p.Status == 2), // 使用中
                FaultCount = ports.Count(p => p.Status == 3), // 故障
                GbCount = ports.Count(p => p.PortType == 1), // 国标
                EuCount = ports.Count(p => p.PortType == 2), // 欧标
                UsCount = ports.Count(p => p.PortType == 3)  // 美标
            };

            return result;
        }

        /// <summary>
        /// 获取用户统计数据
        /// </summary>
        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var now = DateTime.Now;
            var sevenDaysAgo = now.AddDays(-7);
            var thirtyDaysAgo = now.AddDays(-30);

            // 获取活跃用户（30天内有订单的用户）
            var activeUserIds = await _context.Orders
                .Where(o => o.StartTime > thirtyDaysAgo)
                .Select(o => o.UserId)
                .Distinct()
                .ToListAsync();

            var result = new UserStatisticsDto
            {
                TotalCount = users.Count,
                NewUsersLast7Days = users.Count(u => u.UpdateTime > sevenDaysAgo),
                NewUsersLast30Days = users.Count(u => u.UpdateTime > thirtyDaysAgo),
                ActiveUsers = activeUserIds.Count,
                MaleCount = users.Count(u => u.Gender == 1), // 男
                FemaleCount = users.Count(u => u.Gender == 2), // 女
                UnknownGenderCount = users.Count(u => u.Gender == 0 || u.Gender == null) // 未知
            };

            return result;
        }

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        public async Task<StatOrderDto> GetOrderStatisticsAsync()
        {
            var orders = await _context.Orders.ToListAsync();
            var now = DateTime.Now;
            var today = now.Date;
            var sevenDaysAgo = now.AddDays(-7);
            var thirtyDaysAgo = now.AddDays(-30);

            var result = new StatOrderDto
            {
                TotalCount = orders.Count,
                TodayCount = orders.Count(o => o.StartTime?.Date == today),
                Last7DaysCount = orders.Count(o => o.StartTime > sevenDaysAgo),
                Last30DaysCount = orders.Count(o => o.StartTime > thirtyDaysAgo),
                CompletedCount = orders.Count(o => o.Status == 2), // 已完成
                ChargingCount = orders.Count(o => o.Status == 1), // 充电中
                PendingPaymentCount = orders.Count(o => o.Status == 0), // 待支付
                CancelledCount = orders.Count(o => o.Status == 3), // 已取消
                AbnormalCount = orders.Count(o => o.Status == 4), // 异常
                TotalPowerConsumption = Convert.ToDouble(orders.Where(o => o.Status == 2).Sum(o => o.PowerConsumption) ?? 0),
                TotalChargingDuration = Convert.ToInt64(orders.Where(o => o.Status == 2).Sum(o => o.ChargingTime) ?? 0),
                TotalAmount = orders.Where(o => o.Status == 2).Sum(o => o.TotalAmount) ?? 0
            };

            return result;
        }
    }
}
