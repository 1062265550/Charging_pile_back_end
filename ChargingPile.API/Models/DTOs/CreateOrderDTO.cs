using System;
using System.ComponentModel.DataAnnotations;

namespace ChargingPile.API.Models.DTOs
{
    /// <summary>
    /// 创建订单的数据传输对象
    /// </summary>
    public class CreateOrderDTO
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "用户ID不能为空")]
        public int UserId { get; set; }

        /// <summary>
        /// 充电站ID
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440000</example>
        [Required(ErrorMessage = "充电站ID不能为空")]
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// 充电口ID
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440001</example>
        [Required(ErrorMessage = "充电口ID不能为空")]
        public string PortId { get; set; } = string.Empty;
    }
} 