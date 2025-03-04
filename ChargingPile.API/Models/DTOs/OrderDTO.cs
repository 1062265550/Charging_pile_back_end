using System;
using System.ComponentModel.DataAnnotations;

namespace ChargingPile.API.Models.DTOs
{
    /// <summary>
    /// 订单数据传输对象
    /// </summary>
    public class OrderDTO
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 订单号 (UUID)
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440000</example>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 充电站ID
        /// </summary>
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// 充电站名称
        /// </summary>
        public string StationName { get; set; } = string.Empty;

        /// <summary>
        /// 充电口ID
        /// </summary>
        public string PortId { get; set; } = string.Empty;

        /// <summary>
        /// 充电口编号
        /// </summary>
        public string PortNo { get; set; } = string.Empty;

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; } = string.Empty;

        /// <summary>
        /// 充电开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 充电结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 订单金额（元）
        /// </summary>
        /// <example>50.00</example>
        public decimal Amount { get; set; }

        /// <summary>
        /// 充电量（度）
        /// </summary>
        /// <example>3.61</example>
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        /// <remarks>
        /// - 0: 进行中
        /// - 1: 已完成
        /// - 2: 已取消
        /// </remarks>
        /// <example>0</example>
        public int Status { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        /// <remarks>
        /// - 0: 未支付
        /// - 1: 已支付
        /// - 2: 退款中
        /// - 3: 已退款
        /// </remarks>
        /// <example>0</example>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 订单更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
} 