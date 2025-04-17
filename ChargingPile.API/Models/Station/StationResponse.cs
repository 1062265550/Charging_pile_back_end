using System;

namespace ChargingPile.API.Models.Station
{
    /// <summary>
    /// 充电站响应
    /// </summary>
    public class StationResponse
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
        /// 距离(米)
        /// </summary>
        public double Distance { get; set; }

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
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }


    }
}
