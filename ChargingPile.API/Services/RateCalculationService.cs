using System;
using System.Collections.Generic;
using System.Linq;
using ChargingPile.API.Models.Entities;
using Microsoft.Extensions.Logging;

namespace ChargingPile.API.Services
{
    public class RateCalculationService
    {
        private readonly ILogger<RateCalculationService> _logger;
        private readonly List<TimeRateConfig> _rateConfigs;

        public RateCalculationService(ILogger<RateCalculationService> logger)
        {
            _logger = logger;
            // 初始化费率配置
            _rateConfigs = new List<TimeRateConfig>
            {
                new TimeRateConfig { StartTime = TimeSpan.FromHours(0), EndTime = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(59)), Rate = 1.0263M },
                new TimeRateConfig { StartTime = TimeSpan.FromHours(7), EndTime = TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(59)), Rate = 1.0263M },
                new TimeRateConfig { StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(59)), Rate = 1.0593M },
                new TimeRateConfig { StartTime = TimeSpan.FromHours(12), EndTime = TimeSpan.FromHours(14).Add(TimeSpan.FromMinutes(59)), Rate = 1.0263M },
                new TimeRateConfig { StartTime = TimeSpan.FromHours(15), EndTime = TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(59)), Rate = 1.0593M },
                new TimeRateConfig { StartTime = TimeSpan.FromHours(19), EndTime = TimeSpan.FromHours(20).Add(TimeSpan.FromMinutes(59)), Rate = 1.0593M },
                new TimeRateConfig { StartTime = TimeSpan.FromHours(21), EndTime = TimeSpan.FromHours(22).Add(TimeSpan.FromMinutes(59)), Rate = 1.0263M },
                new TimeRateConfig { StartTime = TimeSpan.FromHours(23), EndTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)), Rate = 1.0263M }
            };
        }

        /// <summary>
        /// 计算指定时间段内的充电费用
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="powerRate">充电功率</param>
        /// <returns>(充电量, 费用)</returns>
        public (decimal consumption, decimal amount) CalculateChargingFee(DateTime startTime, DateTime endTime, decimal powerRate)
        {
            if (endTime <= startTime)
            {
                _logger.LogWarning("结束时间不能小于或等于开始时间");
                return (0, 0);
            }

            decimal totalConsumption = 0;
            decimal totalAmount = 0;
            var currentTime = startTime;

            while (currentTime < endTime)
            {
                // 获取当前时间所在的费率配置
                var timeOfDay = currentTime.TimeOfDay;
                var rateConfig = _rateConfigs.First(r => timeOfDay >= r.StartTime && timeOfDay <= r.EndTime);

                // 计算当前费率时段的结束时间
                var periodEndTime = currentTime.Date.Add(rateConfig.EndTime);
                if (periodEndTime > endTime)
                {
                    periodEndTime = endTime;
                }
                if (periodEndTime.TimeOfDay < timeOfDay)
                {
                    periodEndTime = currentTime.Date.AddDays(1).Add(rateConfig.EndTime);
                }

                // 计算当前时段的充电时长（小时）
                var hours = (periodEndTime - currentTime).TotalHours;
                
                // 计算当前时段的充电量
                var consumption = powerRate * (decimal)hours;
                totalConsumption += consumption;

                // 计算当前时段的费用
                var amount = consumption * rateConfig.Rate;
                totalAmount += amount;

                _logger.LogInformation($"时段 {currentTime:HH:mm} - {periodEndTime:HH:mm} " +
                    $"费率：{rateConfig.Rate:F4} 元/度, " +
                    $"充电量：{consumption:F2} 度, " +
                    $"费用：{amount:F2} 元");

                currentTime = periodEndTime;
            }

            _logger.LogInformation($"总充电量：{totalConsumption:F2} 度, 总费用：{totalAmount:F2} 元");
            return (Math.Round(totalConsumption, 2), Math.Round(totalAmount, 2));
        }

        /// <summary>
        /// 实时计算当前充电费用
        /// </summary>
        public (decimal consumption, decimal amount) CalculateCurrentChargingFee(DateTime startTime, decimal powerRate)
        {
            return CalculateChargingFee(startTime, DateTime.Now, powerRate);
        }
    }
} 