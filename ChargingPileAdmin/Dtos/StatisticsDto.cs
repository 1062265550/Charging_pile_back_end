using System;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 统计数据DTO
    /// </summary>
    public class StatisticsDto
    {
        /// <summary>
        /// 充电桩统计
        /// </summary>
        public ChargingPileStatisticsDto PileStatistics { get; set; } = new ChargingPileStatisticsDto();

        /// <summary>
        /// 充电口统计
        /// </summary>
        public ChargingPortStatisticsDto PortStatistics { get; set; } = new ChargingPortStatisticsDto();

        /// <summary>
        /// 用户统计
        /// </summary>
        public UserStatisticsDto UserStatistics { get; set; } = new UserStatisticsDto();

        /// <summary>
        /// 订单统计
        /// </summary>
        public StatOrderDto OrderStatistics { get; set; } = new StatOrderDto();
    }

    /// <summary>
    /// 充电桩统计DTO
    /// </summary>
    public class ChargingPileStatisticsDto
    {
        /// <summary>
        /// 充电桩总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 在线充电桩数量
        /// </summary>
        public int OnlineCount { get; set; }

        /// <summary>
        /// 离线充电桩数量
        /// </summary>
        public int OfflineCount { get; set; }

        /// <summary>
        /// 故障充电桩数量
        /// </summary>
        public int FaultCount { get; set; }

        /// <summary>
        /// 直流快充数量
        /// </summary>
        public int DcFastCount { get; set; }

        /// <summary>
        /// 交流慢充数量
        /// </summary>
        public int AcSlowCount { get; set; }
    }

    /// <summary>
    /// 充电口统计DTO
    /// </summary>
    public class ChargingPortStatisticsDto
    {
        /// <summary>
        /// 充电口总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 空闲充电口数量
        /// </summary>
        public int IdleCount { get; set; }

        /// <summary>
        /// 使用中充电口数量
        /// </summary>
        public int InUseCount { get; set; }

        /// <summary>
        /// 故障充电口数量
        /// </summary>
        public int FaultCount { get; set; }

        /// <summary>
        /// 国标充电口数量
        /// </summary>
        public int GbCount { get; set; }

        /// <summary>
        /// 欧标充电口数量
        /// </summary>
        public int EuCount { get; set; }

        /// <summary>
        /// 美标充电口数量
        /// </summary>
        public int UsCount { get; set; }
    }

    /// <summary>
    /// 用户统计DTO
    /// </summary>
    public class UserStatisticsDto
    {
        /// <summary>
        /// 用户总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 近7天新增用户数
        /// </summary>
        public int NewUsersLast7Days { get; set; }

        /// <summary>
        /// 近30天新增用户数
        /// </summary>
        public int NewUsersLast30Days { get; set; }

        /// <summary>
        /// 活跃用户数（30天内有订单）
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// 男性用户数
        /// </summary>
        public int MaleCount { get; set; }

        /// <summary>
        /// 女性用户数
        /// </summary>
        public int FemaleCount { get; set; }

        /// <summary>
        /// 性别未知用户数
        /// </summary>
        public int UnknownGenderCount { get; set; }
    }

    /// <summary>
    /// 订单统计DTO（统计模块专用）
    /// </summary>
    public class StatOrderDto
    {
        /// <summary>
        /// 订单总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 今日订单数
        /// </summary>
        public int TodayCount { get; set; }

        /// <summary>
        /// 近7天订单数
        /// </summary>
        public int Last7DaysCount { get; set; }

        /// <summary>
        /// 近30天订单数
        /// </summary>
        public int Last30DaysCount { get; set; }

        /// <summary>
        /// 完成订单数
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// 充电中订单数
        /// </summary>
        public int ChargingCount { get; set; }

        /// <summary>
        /// 待支付订单数
        /// </summary>
        public int PendingPaymentCount { get; set; }

        /// <summary>
        /// 已取消订单数
        /// </summary>
        public int CancelledCount { get; set; }

        /// <summary>
        /// 异常订单数
        /// </summary>
        public int AbnormalCount { get; set; }

        /// <summary>
        /// 总充电量(kWh)
        /// </summary>
        public double TotalPowerConsumption { get; set; }

        /// <summary>
        /// 总充电时长(分钟)
        /// </summary>
        public long TotalChargingDuration { get; set; }

        /// <summary>
        /// 总消费金额(元)
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
