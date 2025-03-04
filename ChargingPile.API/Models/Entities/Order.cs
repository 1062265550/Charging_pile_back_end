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
        /// 订单号 (UUID)
        /// </summary>
        /// <example>550e8400-e29b-41d4-a716-446655440000</example>
        [Required]
        [Column("order_no")]
        public string OrderNo { get; set; } = Guid.NewGuid().ToString();

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
        /// 充电量（度）
        /// </summary>
        /// <example>3.61</example>
        [Column("power_consumption", TypeName = "decimal(10,2)")]
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
        public User? User { get; set; }

        /// <summary>
        /// 关联的充电站信息
        /// </summary>
        [ForeignKey("StationId")]
        public ChargingStation? ChargingStation { get; set; }

        /// <summary>
        /// 充电口ID
        /// </summary>
        [Required]
        [Column("port_id")]
        public string PortId { get; set; } = string.Empty;

        /// <summary>
        /// 充电口
        /// </summary>
        [ForeignKey("PortId")]
        public virtual ChargingPort? ChargingPort { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 订单更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }
    }
} 