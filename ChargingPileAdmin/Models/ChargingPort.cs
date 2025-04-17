using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 充电端口信息
    /// </summary>
    public class ChargingPort
    {
        /// <summary>
        /// 充电端口唯一标识符
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// 所属充电桩ID
        /// </summary>
        [Required]
        [Column("pile_id")]
        public string PileId { get; set; } = null!;

        /// <summary>
        /// 端口编号
        /// </summary>
        [Required]
        [Column("port_no")]
        [StringLength(50)]
        public string PortNo { get; set; } = null!;

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        [Column("port_type")]
        public int? PortType { get; set; }

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        [Column("status")]
        public int? Status { get; set; }

        /// <summary>
        /// 故障类型：0-无故障，1-保险丝熔断，2-继电器粘连
        /// </summary>
        [Column("fault_type")]
        public short? FaultType { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        [Column("is_disabled")]
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// 当前订单ID
        /// </summary>
        [Column("current_order_id")]
        [StringLength(36)]
        public string? CurrentOrderId { get; set; }

        /// <summary>
        /// 当前电压(V)
        /// </summary>
        [Column("voltage")]
        public decimal? Voltage { get; set; }

        /// <summary>
        /// 当前电流(A)
        /// </summary>
        [Column("current_ampere")]
        public decimal? CurrentAmpere { get; set; }

        /// <summary>
        /// 当前功率(kW)
        /// </summary>
        [Column("power")]
        public decimal? Power { get; set; }

        /// <summary>
        /// 端口温度
        /// </summary>
        [Column("temperature", TypeName = "decimal(18,2)")]
        public decimal? Temperature { get; set; }

        /// <summary>
        /// 当前电量(kWh)
        /// </summary>
        [Column("electricity")]
        public decimal? Electricity { get; set; }

        /// <summary>
        /// 总充电次数
        /// </summary>
        [Column("total_charging_times")]
        public int? TotalChargingTimes { get; set; }

        /// <summary>
        /// 总充电时长(秒)
        /// </summary>
        [Column("total_charging_duration")]
        public int? TotalChargingDuration { get; set; }

        /// <summary>
        /// 总耗电量(kWh)
        /// </summary>
        [Column("total_power_consumption")]
        public decimal? TotalPowerConsumption { get; set; }

        /// <summary>
        /// 最后检查时间
        /// </summary>
        [Column("last_check_time")]
        public DateTime? LastCheckTime { get; set; }

        /// <summary>
        /// 最后一个订单ID
        /// </summary>
        [Column("last_order_id")]
        [StringLength(36)]
        public string? LastOrderId { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ChargingPile? ChargingPile { get; set; }
    }
}
