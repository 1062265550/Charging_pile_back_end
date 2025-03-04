using System;

namespace ChargingPile.API.Models.Entities
{
    /// <summary>
    /// 时间段费率配置
    /// </summary>
    public class TimeRateConfig
    {
        public int Id { get; set; }
        
        /// <summary>
        /// 开始时间（格式：HH:mm）
        /// </summary>
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// 结束时间（格式：HH:mm）
        /// </summary>
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// 费率（元/度）
        /// </summary>
        public decimal Rate { get; set; }
    }
} 