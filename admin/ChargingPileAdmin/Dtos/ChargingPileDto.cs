using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 充电桩数据传输对象
    /// </summary>
    public class ChargingPileDto
    {
        /// <summary>
        /// 充电桩唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属充电站ID
        /// </summary>
        public string StationId { get; set; }

        /// <summary>
        /// 所属充电站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 充电桩类型：1-直流快充，2-交流慢充
        /// </summary>
        public int PileType { get; set; }

        /// <summary>
        /// 充电桩类型描述
        /// </summary>
        public string PileTypeDescription => PileType switch
        {
            1 => "直流快充",
            2 => "交流慢充",
            _ => "未知"
        };

        /// <summary>
        /// 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 充电桩状态描述
        /// </summary>
        public string StatusDescription => Status switch
        {
            0 => "离线",
            1 => "空闲",
            2 => "使用中",
            3 => "故障",
            _ => "未知"
        };

        /// <summary>
        /// 额定功率(kW)
        /// </summary>
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 设备IMEI号
        /// </summary>
        public string IMEI { get; set; }

        /// <summary>
        /// 总端口数
        /// </summary>
        public int TotalPorts { get; set; }

        /// <summary>
        /// 协议版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// 在线状态：0-离线，1-在线
        /// </summary>
        public short? OnlineStatus { get; set; }

        /// <summary>
        /// 在线状态描述
        /// </summary>
        public string OnlineStatusDescription => OnlineStatus switch
        {
            0 => "离线",
            1 => "在线",
            _ => "未知"
        };

        /// <summary>
        /// 信号强度
        /// </summary>
        public int? SignalStrength { get; set; }

        /// <summary>
        /// 设备温度
        /// </summary>
        public decimal? Temperature { get; set; }

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTime? LastHeartbeatTime { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// 最后维护日期
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 创建充电桩请求
    /// </summary>
    [SwaggerSchema("创建充电桩请求")]
    public class CreateChargingPileDto
    {
        /// <summary>
        /// 所属充电站ID
        /// </summary>
        [Required(ErrorMessage = "充电站ID不能为空")]
        [SwaggerSchema("所属充电站ID", Description = "必填，关联到充电站的唯一标识符")]
        [DisplayName("充电站ID")]
        public string StationId { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        [Required(ErrorMessage = "充电桩编号不能为空")]
        [StringLength(50, ErrorMessage = "充电桩编号长度不能超过50个字符")]
        [SwaggerSchema("充电桩编号", Description = "必填，如：CP001")]
        [DisplayName("充电桩编号")]
        public string PileNo { get; set; }

        /// <summary>
        /// 充电桩类型：1-直流快充，2-交流慢充
        /// </summary>
        [Required(ErrorMessage = "充电桩类型不能为空")]
        [Range(1, 2, ErrorMessage = "充电桩类型必须为1或2")]
        [SwaggerSchema("充电桩类型", Description = "必填，1-直流快充，2-交流慢充")]
        [DisplayName("充电桩类型")]
        public int PileType { get; set; }

        /// <summary>
        /// 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        [Range(0, 3, ErrorMessage = "充电桩状态必须在0至3之间")]
        [SwaggerSchema("充电桩状态", Description = "可选，0-离线，1-空闲，2-使用中，3-故障，默认为1")]
        [DefaultValue(1)]
        [DisplayName("充电桩状态")]
        public int Status { get; set; } = 1;

        /// <summary>
        /// 额定功率(kW)
        /// </summary>
        [Required(ErrorMessage = "额定功率不能为空")]
        [Range(0.1, 500, ErrorMessage = "额定功率必须在0.1至500kW之间")]
        [SwaggerSchema("额定功率", Description = "必填，单位为kW，如：60kW")]
        [DisplayName("额定功率(kW)")]
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        [SwaggerSchema("制造商", Description = "可选，充电桩的制造厂商名称")]
        [DisplayName("制造商")]
        public string Manufacturer { get; set; }

        /// <summary>
        /// 设备IMEI号
        /// </summary>
        [SwaggerSchema("设备IMEI号", Description = "可选，设备的唯一标识码")]
        [DisplayName("设备IMEI号")]
        public string IMEI { get; set; }

        /// <summary>
        /// 总端口数
        /// </summary>
        [Required(ErrorMessage = "端口数不能为空")]
        [Range(1, 10, ErrorMessage = "端口数必须在1至10之间")]
        [SwaggerSchema("总端口数", Description = "必填，充电桩上的充电端口数量")]
        [DisplayName("总端口数")]
        public int TotalPorts { get; set; }

        /// <summary>
        /// 协议版本
        /// </summary>
        [SwaggerSchema("协议版本", Description = "可选，如：2.0")]
        [DisplayName("协议版本")]
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        [SwaggerSchema("软件版本", Description = "可选，如：V1.2.3")]
        [DisplayName("软件版本")]
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        [SwaggerSchema("硬件版本", Description = "可选，如：H3.0")]
        [DisplayName("硬件版本")]
        public string HardwareVersion { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        [SwaggerSchema("安装日期", Description = "可选，设备安装的日期，如：2023-01-15")]
        [DisplayName("安装日期")]
        public DateTime? InstallationDate { get; set; }
    }

    /// <summary>
    /// 更新充电桩请求
    /// </summary>
    public class UpdateChargingPileDto
    {
        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 充电桩类型：1-直流快充，2-交流慢充
        /// </summary>
        public int PileType { get; set; }

        /// <summary>
        /// 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 额定功率(kW)
        /// </summary>
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 设备IMEI号
        /// </summary>
        public string IMEI { get; set; }

        /// <summary>
        /// 总端口数
        /// </summary>
        public int TotalPorts { get; set; }

        /// <summary>
        /// 协议版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// 最后维护日期
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }
    }
}
