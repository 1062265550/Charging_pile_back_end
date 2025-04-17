using System;
using System.Collections.Generic;

namespace ChargingPile.API.Models.Station
{
    /// <summary>
    /// 充电站详情响应
    /// </summary>
    public class StationDetailResponse
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
        /// 充电站地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// 充电站描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 可用端口数
        /// </summary>
        public int AvailablePorts { get; set; }

        /// <summary>
        /// 总端口数
        /// </summary>
        public int TotalPorts { get; set; }

        /// <summary>
        /// 充电桩列表
        /// </summary>
        public List<PileInfo> Piles { get; set; } = new List<PileInfo>();

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 充电桩信息
    /// </summary>
    public class PileInfo
    {
        /// <summary>
        /// 充电桩唯一标识符
        /// </summary>
        public string Id { get; set; }

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
        /// 总端口数
        /// </summary>
        public int TotalPorts { get; set; }

        /// <summary>
        /// 可用端口数
        /// </summary>
        public int AvailablePorts { get; set; }
    }
}
