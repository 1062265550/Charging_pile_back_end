using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Text.Json.Serialization;

namespace ChargingPile.API.Models.Entities
{
    /// <summary>
    /// 充电站实体
    /// </summary>
    [Table("charging_stations")]
    [Index(nameof(Location), Name = "spatial_location")]
    public class ChargingStation
    {
        /// <summary>
        /// 充电站ID
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 充电站名称
        /// </summary>
        /// <example>示例充电站</example>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public required string Name { get; set; }
        
        /// <summary>
        /// 纬度
        /// </summary>
        /// <example>30.123456</example>
        [Required]
        [Column("latitude")]
        public decimal Latitude { get; set; }
        
        /// <summary>
        /// 经度
        /// </summary>
        /// <example>120.123456</example>
        [Required]
        [Column("longitude")]
        public decimal Longitude { get; set; }
        
        /// <summary>
        /// 详细地址
        /// </summary>
        /// <example>四川省成都市武侯区某某路123号</example>
        [MaxLength(200)]
        [Column("address")]
        public string? Address { get; set; }
        
        /// <summary>
        /// 充电端口总数
        /// </summary>
        /// <example>10</example>
        [Column("total_ports")]
        public int TotalPorts { get; set; }
        
        /// <summary>
        /// 当前可用端口数
        /// </summary>
        /// <example>5</example>
        [Column("available_ports")]
        public int AvailablePorts { get; set; }
        
        /// <summary>
        /// 充电站状态
        /// </summary>
        /// <remarks>
        /// - 0: 正常运营
        /// - 1: 维护中
        /// - 2: 故障
        /// </remarks>
        /// <example>0</example>
        [Column("status")]
        public int Status { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }
        
        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 地理位置点
        /// </summary>
        [Column("location", TypeName = "point")]
        [JsonIgnore] // 防止序列化问题
        public Point? Location { get; set; }

        /// <summary>
        /// 更新Location字段以匹配经纬度
        /// </summary>
        public void UpdateLocation()
        {
            // 确保使用WGS84坐标系统 (SRID=4326)
            Location = new Point((double)Longitude, (double)Latitude) { SRID = 4326 };
        }
    }
} 