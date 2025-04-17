using Microsoft.SqlServer.Types;
using System;

namespace ChargingPile.API.Models.Station
{
    /// <summary>
    /// 充电站实体
    /// </summary>
    public class ChargingStationEntity
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
        /// 充电站地理位置坐标
        /// </summary>
        public SqlGeography Location { get; set; }

        /// <summary>
        /// 充电站描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
