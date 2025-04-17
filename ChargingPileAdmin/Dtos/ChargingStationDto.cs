using System;
using System.Collections.Generic;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 充电站数据传输对象
    /// </summary>
    public class ChargingStationDto
    {
        /// <summary>
        /// 充电站唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 充电站名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 充电站状态：0-离线，1-在线，2-维护中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 充电站状态描述
        /// </summary>
        public string StatusDescription => Status switch
        {
            0 => "离线",
            1 => "在线",
            2 => "维护中",
            3 => "故障",
            _ => "未知"
        };

        /// <summary>
        /// 充电站地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 充电站地理位置坐标
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 充电站描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 充电桩数量
        /// </summary>
        public int PileCount { get; set; }
    }

    /// <summary>
    /// 创建充电站请求
    /// </summary>
    public class CreateChargingStationDto
    {
        /// <summary>
        /// 充电站名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 充电站状态：0-离线，1-在线，2-维护中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 充电站地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 充电站地理位置坐标
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 充电站描述
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// 更新充电站请求
    /// </summary>
    public class UpdateChargingStationDto
    {
        /// <summary>
        /// 充电站名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 充电站状态：0-离线，1-在线，2-维护中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 充电站地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 充电站地理位置坐标
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 充电站描述
        /// </summary>
        public string Description { get; set; }
    }
}
