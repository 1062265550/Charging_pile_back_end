using System;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 充电端口数据传输对象
    /// </summary>
    public class ChargingPortDto
    {
        /// <summary>
        /// 充电端口唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属充电桩ID
        /// </summary>
        public string PileId { get; set; }

        /// <summary>
        /// 所属充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int? PortType { get; set; }

        /// <summary>
        /// 端口类型描述
        /// </summary>
        public string PortTypeDescription => PortType switch
        {
            1 => "国标",
            2 => "欧标",
            3 => "美标",
            _ => "未知"
        };

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 端口状态描述
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
        /// 故障类型：0-无故障，1-保险丝熔断，2-继电器粘连
        /// </summary>
        public short? FaultType { get; set; }

        /// <summary>
        /// 故障类型描述
        /// </summary>
        public string FaultTypeDescription => FaultType switch
        {
            0 => "无故障",
            1 => "保险丝熔断",
            2 => "继电器粘连",
            _ => "未知"
        };

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// 当前订单ID
        /// </summary>
        public string CurrentOrderId { get; set; }

        /// <summary>
        /// 当前电压(V)
        /// </summary>
        public decimal? Voltage { get; set; }

        /// <summary>
        /// 当前电流(A)
        /// </summary>
        public decimal? CurrentAmpere { get; set; }

        /// <summary>
        /// 当前功率(kW)
        /// </summary>
        public decimal? Power { get; set; }

        /// <summary>
        /// 端口温度
        /// </summary>
        public decimal? Temperature { get; set; }

        /// <summary>
        /// 当前电量(kWh)
        /// </summary>
        public decimal? Electricity { get; set; }

        /// <summary>
        /// 总充电次数
        /// </summary>
        public int? TotalChargingTimes { get; set; }

        /// <summary>
        /// 总充电时长(秒)
        /// </summary>
        public int? TotalChargingDuration { get; set; }

        /// <summary>
        /// 总耗电量(kWh)
        /// </summary>
        public decimal? TotalPowerConsumption { get; set; }

        /// <summary>
        /// 最后检查时间
        /// </summary>
        public DateTime? LastCheckTime { get; set; }

        /// <summary>
        /// 最后一个订单ID
        /// </summary>
        public string LastOrderId { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 创建充电端口请求
    /// </summary>
    public class CreateChargingPortDto
    {
        /// <summary>
        /// 所属充电桩ID
        /// </summary>
        public string PileId { get; set; }

        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int? PortType { get; set; }
        
        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int? Status { get; set; }
        
        /// <summary>
        /// 当前电压(V)
        /// </summary>
        public decimal? Voltage { get; set; }
        
        /// <summary>
        /// 当前电流(A)
        /// </summary>
        public decimal? CurrentAmpere { get; set; }
        
        /// <summary>
        /// 当前功率(kW)
        /// </summary>
        public decimal? Power { get; set; }
        
        /// <summary>
        /// 端口温度
        /// </summary>
        public decimal? Temperature { get; set; }
    }

    /// <summary>
    /// 更新充电端口请求
    /// </summary>
    public class UpdateChargingPortDto
    {
        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int? PortType { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool? IsDisabled { get; set; }
        
        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int? Status { get; set; }
        
        /// <summary>
        /// 当前电压(V)
        /// </summary>
        public decimal? Voltage { get; set; }
        
        /// <summary>
        /// 当前电流(A)
        /// </summary>
        public decimal? CurrentAmpere { get; set; }
        
        /// <summary>
        /// 当前功率(kW)
        /// </summary>
        public decimal? Power { get; set; }
        
        /// <summary>
        /// 端口温度
        /// </summary>
        public decimal? Temperature { get; set; }
    }
}
