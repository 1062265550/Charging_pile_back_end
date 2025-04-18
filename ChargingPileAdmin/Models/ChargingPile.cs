using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 充电桩信息
    /// </summary>
    public class ChargingPile
    {
        /// <summary>
        /// 充电桩唯一标识符
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; }

        /// <summary>
        /// 所属充电站ID
        /// </summary>
        [Required]
        [Column("station_id")]
        public string StationId { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        [Required]
        [Column("pile_no")]
        [StringLength(50)]
        public string PileNo { get; set; }

        /// <summary>
        /// 充电桩类型：1-直流快充，2-交流慢充
        /// </summary>
        [Column("pile_type")]
        public int PileType { get; set; }

        /// <summary>
        /// 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        [Column("status")]
        public int Status { get; set; }

        /// <summary>
        /// 额定功率(kW)
        /// </summary>
        [Column("power_rate")]
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        [Column("manufacturer")]
        [StringLength(100)]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// 设备IMEI号
        /// </summary>
        [Column("imei")]
        [StringLength(20)]
        public string? IMEI { get; set; }

        /// <summary>
        /// 总端口数
        /// </summary>
        [Column("total_ports")]
        public int TotalPorts { get; set; }

        /// <summary>
        /// 浮充参考功率(W)
        /// </summary>
        [Column("floating_power")]
        public decimal? FloatingPower { get; set; }

        /// <summary>
        /// 充满自停开关状态
        /// </summary>
        [Column("auto_stop_enabled")]
        public bool? AutoStopEnabled { get; set; }

        /// <summary>
        /// 充电器插入等待时间(秒)
        /// </summary>
        [Column("plugin_wait_time")]
        public int? PluginWaitTime { get; set; }

        /// <summary>
        /// 充电器移除等待时间(秒)
        /// </summary>
        [Column("removal_wait_time")]
        public int? RemovalWaitTime { get; set; }

        /// <summary>
        /// 是否支持新协议格式(带IMEI)
        /// </summary>
        [Column("supports_new_protocol")]
        public bool? SupportsNewProtocol { get; set; }

        /// <summary>
        /// 协议版本
        /// </summary>
        [Column("protocol_version")]
        [StringLength(50)]
        public string? ProtocolVersion { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        [Column("software_version")]
        [StringLength(50)]
        public string? SoftwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        [Column("hardware_version")]
        [StringLength(50)]
        public string? HardwareVersion { get; set; }

        /// <summary>
        /// CCID号
        /// </summary>
        [Column("ccid")]
        [StringLength(50)]
        public string? CCID { get; set; }

        /// <summary>
        /// 额定电压
        /// </summary>
        [Column("voltage")]
        [StringLength(20)]
        public string? Voltage { get; set; }

        /// <summary>
        /// 额定电流
        /// </summary>
        [Column("ampere_value")]
        [StringLength(20)]
        public string? AmpereValue { get; set; }

        /// <summary>
        /// 在线状态：0-离线，1-在线
        /// </summary>
        [Column("online_status")]
        public short? OnlineStatus { get; set; }

        /// <summary>
        /// 信号强度
        /// </summary>
        [Column("signal_strength")]
        public int? SignalStrength { get; set; }

        /// <summary>
        /// 设备温度
        /// </summary>
        [Column("temperature")]
        public decimal? Temperature { get; set; }

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        [Column("last_heartbeat_time")]
        public DateTime? LastHeartbeatTime { get; set; }

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
        /// 充电桩描述
        /// </summary>
        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ChargingStation ChargingStation { get; set; }
        public virtual ICollection<ChargingPort> ChargingPorts { get; set; }
    }
}
