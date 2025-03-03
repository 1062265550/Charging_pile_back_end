using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPile.API.Models.Entities
{
    /// <summary>
    /// 充电订单实体
    /// </summary>
    [Table("orders")]
    public class Order
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// 充电站ID
        /// </summary>
        [Required]
        [Column("station_id")]
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// 充电开始时间
        /// </summary>
        [Column("start_time")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 充电结束时间
        /// </summary>
        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 订单金额（元）
        /// </summary>
        /// <example>50.00</example>
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        /// <remarks>
        /// - 0: 进行中
        /// - 1: 已完成
        /// - 2: 已取消
        /// </remarks>
        /// <example>0</example>
        [Column("status")]
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
        [Column("payment_status")]
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 关联的用户信息
        /// </summary>
        [ForeignKey("UserId")]
        public required User User { get; set; }

        /// <summary>
        /// 关联的充电站信息
        /// </summary>
        [ForeignKey("StationId")]
        public required ChargingStation ChargingStation { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 订单最后更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }
    }
} 