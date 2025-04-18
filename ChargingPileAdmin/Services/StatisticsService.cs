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
            // 使用投影只选择需要的字段，避免NULL值问题
            var pilesData = await _context.ChargingPiles
                .Select(p => new {
                    p.OnlineStatus,
                    p.Status,
                    p.PileType
                })
                .ToListAsync();

            var result = new ChargingPileStatisticsDto
            {
                TotalCount = pilesData.Count,
                OnlineCount = pilesData.Count(p => p.OnlineStatus == 1), // 在线
                OfflineCount = pilesData.Count(p => p.OnlineStatus == 0), // 离线
                FaultCount = pilesData.Count(p => p.Status == 3), // 故障
                DcFastCount = pilesData.Count(p => p.PileType == 1), // 直流快充
                AcSlowCount = pilesData.Count(p => p.PileType == 2)  // 交流慢充
            };

            return result;
        }

        /// <summary>
        /// 获取充电口统计数据
        /// </summary>
        public async Task<ChargingPortStatisticsDto> GetPortStatisticsAsync()
        {
            // 使用投影只选择需要的字段，避免NULL值问题
            var portsData = await _context.ChargingPorts
                .Select(p => new {
                    p.Status,
                    p.PortType
                })
                .ToListAsync();

            var result = new ChargingPortStatisticsDto
            {
                TotalCount = portsData.Count,
                IdleCount = portsData.Count(p => p.Status == 1), // 空闲
                InUseCount = portsData.Count(p => p.Status == 2), // 使用中
                FaultCount = portsData.Count(p => p.Status == 3), // 故障
                GbCount = portsData.Count(p => p.PortType == 1), // 国标
                EuCount = portsData.Count(p => p.PortType == 2), // 欧标
                UsCount = portsData.Count(p => p.PortType == 3)  // 美标
            };

            return result;
        }

        /// <summary>
        /// 获取用户统计数据
        /// </summary>
        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var now = DateTime.Now;
            var sevenDaysAgo = now.AddDays(-7);
            var thirtyDaysAgo = now.AddDays(-30);

            // 使用投影只选择需要的字段，避免NULL值问题
            var usersData = await _context.Users
                .Select(u => new {
                    u.Gender,
                    u.UpdateTime
                })
                .ToListAsync();

            // 获取活跃用户（30天内有订单的用户）
            var activeUserIds = await _context.Orders
                .Where(o => o.StartTime != null && o.StartTime > thirtyDaysAgo)
                .Select(o => o.UserId)
                .Distinct()
                .ToListAsync();

            var result = new UserStatisticsDto
            {
                TotalCount = usersData.Count,
                NewUsersLast7Days = usersData.Count(u => u.UpdateTime != null && u.UpdateTime > sevenDaysAgo),
                NewUsersLast30Days = usersData.Count(u => u.UpdateTime != null && u.UpdateTime > thirtyDaysAgo),
                ActiveUsers = activeUserIds.Count,
                MaleCount = usersData.Count(u => u.Gender == 1), // 男
                FemaleCount = usersData.Count(u => u.Gender == 2), // 女
                UnknownGenderCount = usersData.Count(u => u.Gender == 0 || u.Gender == null) // 未知
            };

            return result;
        }

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        public async Task<StatOrderDto> GetOrderStatisticsAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var sevenDaysAgo = now.AddDays(-7);
            var thirtyDaysAgo = now.AddDays(-30);

            // 使用投影只选择需要的字段，避免NULL值问题
            var ordersData = await _context.Orders
                .Select(o => new {
                    o.Status,
                    o.StartTime,
                    o.PowerConsumption,
                    o.ChargingTime,
                    o.TotalAmount
                })
                .ToListAsync();

            var result = new StatOrderDto
            {
                TotalCount = ordersData.Count,
                TodayCount = ordersData.Count(o => o.StartTime != null && o.StartTime.Value.Date == today),
                Last7DaysCount = ordersData.Count(o => o.StartTime != null && o.StartTime > sevenDaysAgo),
                Last30DaysCount = ordersData.Count(o => o.StartTime != null && o.StartTime > thirtyDaysAgo),
                CompletedCount = ordersData.Count(o => o.Status == 2), // 已完成
                ChargingCount = ordersData.Count(o => o.Status == 1), // 充电中
                PendingPaymentCount = ordersData.Count(o => o.Status == 0), // 待支付
                CancelledCount = ordersData.Count(o => o.Status == 3), // 已取消
                AbnormalCount = ordersData.Count(o => o.Status == 4), // 异常
                TotalPowerConsumption = Convert.ToDouble(ordersData.Where(o => o.Status == 2).Sum(o => o.PowerConsumption) ?? 0),
                TotalChargingDuration = Convert.ToInt64(ordersData.Where(o => o.Status == 2).Sum(o => o.ChargingTime) ?? 0),
                TotalAmount = ordersData.Where(o => o.Status == 2).Sum(o => o.TotalAmount) ?? 0
            };

            return result;
        }
    }
}
