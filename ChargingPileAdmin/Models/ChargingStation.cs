using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NetTopologySuite.Geometries;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 充电站信息
    /// </summary>
    public class ChargingStation
    {
        /// <summary>
        /// 充电站唯一标识符
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; }

        /// <summary>
        /// 充电站名称
        /// </summary>
        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 充电站状态：0-离线，1-在线，2-维护中，3-故障
        /// </summary>
        [Column("status")]
        public int Status { get; set; }

        /// <summary>
        /// 充电站地址
        /// </summary>
        [Column("address")]
        [StringLength(200)]
        public string Address { get; set; }

        /// <summary>
        /// 充电站地理位置坐标 (SQL Server geography 类型)
        /// </summary>
        [Column("location")]
        public Point Location { get; set; }

        /// <summary>
        /// 获取经纬度字符串，格式为"经度,纬度"
        /// </summary>
        [NotMapped]
        public string LocationString
        {
            get
            {
                if (Location == null) return null;
                return $"{Location.X},{Location.Y}"; // X是经度，Y是纬度
            }
        }

        /// <summary>
        /// 充电站描述
        /// </summary>
        [Column("description")]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ICollection<ChargingPile> ChargingPiles { get; set; }
    }
}
