using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 订单信息
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 订单唯一标识符
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// 订单编号
        /// </summary>
        [Required]
        [Column("order_no")]
        [StringLength(36)]
        public string OrderNo { get; set; } = null!;

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// 充电桩ID
        /// </summary>
        [Column("pile_id")]
        public string PileId { get; set; } = null!;

        /// <summary>
        /// 充电端口ID
        /// </summary>
        [Column("port_id")]
        public string PortId { get; set; } = null!;

        /// <summary>
        /// 订单状态：0-待支付，1-充电中，2-已完成，3-已取消，4-异常
        /// </summary>
        [Column("status")]
        public int Status { get; set; }

        /// <summary>
        /// 计费模式：0-按时间，1-电量+电量服务费，2-电量+时间服务费
        /// </summary>
        [Column("billing_mode")]
        public short? BillingMode { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 充电时长(秒)
        /// </summary>
        [Column("charging_time")]
        public int? ChargingTime { get; set; }

        /// <summary>
        /// 充电电量(kWh)
        /// </summary>
        [Column("power_consumption")]
        public decimal? PowerConsumption { get; set; }

        /// <summary>
        /// 充电金额(元)
        /// </summary>
        [Column("amount")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// 服务费(元)
        /// </summary>
        [Column("service_fee")]
        public decimal? ServiceFee { get; set; }

        /// <summary>
        /// 总金额(元)
        /// </summary>
        [Column("total_amount")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 支付状态：0-未支付，1-已支付，2-已退款
        /// </summary>
        [Column("payment_status")]
        public int? PaymentStatus { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        [Column("payment_time")]
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-余额
        /// </summary>
        [Column("payment_method")]
        public int? PaymentMethod { get; set; }

        /// <summary>
        /// 交易流水号
        /// </summary>
        [Column("transaction_id")]
        [StringLength(100)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// 充电功率(kW)
        /// </summary>
        [Column("power")]
        public decimal? Power { get; set; }

        /// <summary>
        /// 充电模式：1-充满自停，2-按金额，3-按时间，4-按电量，5-其他
        /// </summary>
        [Column("charging_mode")]
        public int? ChargingMode { get; set; }

        /// <summary>
        /// 停止原因：0-充满自停，1-时间用完，2-金额用完，3-手动停止，4-电量用完
        /// </summary>
        [Column("stop_reason")]
        public short? StopReason { get; set; }

        /// <summary>
        /// 尖峰时段电量(kWh)
        /// </summary>
        [Column("sharp_electricity")]
        public decimal? SharpElectricity { get; set; }

        /// <summary>
        /// 尖峰时段金额(元)
        /// </summary>
        [Column("sharp_amount")]
        public decimal? SharpAmount { get; set; }

        /// <summary>
        /// 峰时段电量(kWh)
        /// </summary>
        [Column("peak_electricity")]
        public decimal? PeakElectricity { get; set; }

        /// <summary>
        /// 峰时段金额(元)
        /// </summary>
        [Column("peak_amount")]
        public decimal? PeakAmount { get; set; }

        /// <summary>
        /// 平时段电量(kWh)
        /// </summary>
        [Column("flat_electricity")]
        public decimal? FlatElectricity { get; set; }

        /// <summary>
        /// 平时段金额(元)
        /// </summary>
        [Column("flat_amount")]
        public decimal? FlatAmount { get; set; }

        /// <summary>
        /// 谷时段电量(kWh)
        /// </summary>
        [Column("valley_electricity")]
        public decimal? ValleyElectricity { get; set; }

        /// <summary>
        /// 谷时段金额(元)
        /// </summary>
        [Column("valley_amount")]
        public decimal? ValleyAmount { get; set; }

        /// <summary>
        /// 深谷时段电量(kWh)
        /// </summary>
        [Column("deep_valley_electricity")]
        public decimal? DeepValleyElectricity { get; set; }

        /// <summary>
        /// 深谷时段金额(元)
        /// </summary>
        [Column("deep_valley_amount")]
        public decimal? DeepValleyAmount { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        [Column("remark")]
        [StringLength(200)]
        public string? Remark { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual User? User { get; set; }
        
        [ForeignKey("PileId")]
        public virtual ChargingPile? ChargingPile { get; set; }
        
        [ForeignKey("PortId")]
        public virtual ChargingPort? ChargingPort { get; set; }
    }
}
