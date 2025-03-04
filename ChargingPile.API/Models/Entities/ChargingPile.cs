using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPile.API.Models.Entities
{
    /// <summary>
    /// 充电桩实体
    /// </summary>
    [Table("charging_piles")]
    public class ChargingPile
    {
        /// <summary>
        /// 充电桩ID
        /// </summary>
        [Key]
        [Column("id")]
        [Required]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 所属充电站ID
        /// </summary>
        [Required]
        [Column("station_id")]
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// 充电桩编号
        /// </summary>
        [Required]
        [Column("pile_no")]
        public string PileNo { get; set; } = string.Empty;

        /// <summary>
        /// 充电桩类型 (0: 快充, 1: 慢充)
        /// </summary>
        [Column("pile_type")]
        public int PileType { get; set; }

        /// <summary>
        /// 状态 (0: 正常, 1: 故障, 2: 维护中)
        /// </summary>
        [Column("status")]
        public int Status { get; set; }

        /// <summary>
        /// 额定功率（kW）
        /// </summary>
        [Column("power_rate")]
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 电压等级
        /// </summary>
        [Column("voltage")]
        public string? Voltage { get; set; }

        /// <summary>
        /// 电流规格
        /// </summary>
        [Column("current")]
        public string? Current { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        [Column("manufacturer")]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [Column("model_number")]
        public string? ModelNumber { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        [Column("installation_date")]
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// 最后维护日期
        /// </summary>
        [Column("last_maintenance_date")]
        public DateTime? LastMaintenanceDate { get; set; }

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
        /// 所属充电站
        /// </summary>
        [ForeignKey("StationId")]
        public virtual ChargingStation? ChargingStation { get; set; }

        /// <summary>
        /// 充电口列表
        /// </summary>
        public virtual ICollection<ChargingPort> ChargingPorts { get; set; } = new List<ChargingPort>();
    }
} 