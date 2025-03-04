using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPile.API.Models.Entities
{
    /// <summary>
    /// 充电口实体
    /// </summary>
    [Table("charging_ports")]
    public class ChargingPort
    {
        /// <summary>
        /// 充电口ID
        /// </summary>
        [Key]
        [Column("id")]
        [Required]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 所属充电桩ID
        /// </summary>
        [Required]
        [Column("pile_id")]
        public string PileId { get; set; } = string.Empty;

        /// <summary>
        /// 充电口编号
        /// </summary>
        [Required]
        [Column("port_no")]
        public string PortNo { get; set; } = string.Empty;

        /// <summary>
        /// 接口类型 (0: 国标, 1: 特斯拉, 2: 其他)
        /// </summary>
        [Column("port_type")]
        public int PortType { get; set; }

        /// <summary>
        /// 状态 (0: 空闲, 1: 使用中, 2: 故障, 3: 维护中)
        /// </summary>
        [Column("status")]
        public int Status { get; set; }

        /// <summary>
        /// 当前正在进行的订单ID
        /// </summary>
        [Column("current_order_id")]
        public int? CurrentOrderId { get; set; }

        /// <summary>
        /// 最后一个完成的订单ID
        /// </summary>
        [Column("last_order_id")]
        public int? LastOrderId { get; set; }

        /// <summary>
        /// 总充电次数
        /// </summary>
        [Column("total_charging_times")]
        public int TotalChargingTimes { get; set; }

        /// <summary>
        /// 总充电时长（分钟）
        /// </summary>
        [Column("total_charging_duration")]
        public int TotalChargingDuration { get; set; }

        /// <summary>
        /// 总耗电量（度）
        /// </summary>
        [Column("total_power_consumption")]
        public decimal TotalPowerConsumption { get; set; }

        /// <summary>
        /// 最后检查时间
        /// </summary>
        [Column("last_check_time")]
        public DateTime? LastCheckTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 所属充电桩
        /// </summary>
        [ForeignKey("PileId")]
        [Required]
        public virtual ChargingPile? ChargingPile { get; set; }

        /// <summary>
        /// 当前订单
        /// </summary>
        [ForeignKey("CurrentOrderId")]
        public virtual Order? CurrentOrder { get; set; }

        /// <summary>
        /// 最后完成的订单
        /// </summary>
        [ForeignKey("LastOrderId")]
        public virtual Order? LastOrder { get; set; }
    }
} 