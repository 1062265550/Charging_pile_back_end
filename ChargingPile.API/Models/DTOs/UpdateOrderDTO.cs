using System;
using System.ComponentModel.DataAnnotations;

namespace ChargingPile.API.Models.DTOs
{
    /// <summary>
    /// 更新订单的DTO
    /// </summary>
    public class UpdateOrderDTO
    {
        /// <summary>
        /// 订单状态（0：进行中，1：已完成，2：已取消）
        /// </summary>
        /// <example>1</example>
        public int? Status { get; set; }

        /// <summary>
        /// 支付状态（0：未支付，1：已支付）
        /// </summary>
        /// <example>1</example>
        public int? PaymentStatus { get; set; }
    }
} 