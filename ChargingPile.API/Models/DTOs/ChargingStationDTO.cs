using System.ComponentModel.DataAnnotations;

namespace ChargingPile.API.Models.DTOs
{
    /// <summary>
    /// 创建充电站的数据传输对象
    /// </summary>
    public class CreateChargingStationDTO
    {
        /// <summary>
        /// 充电站名称
        /// </summary>
        /// <example>示例充电站</example>
        [Required(ErrorMessage = "充电站名称不能为空")]
        [MaxLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        /// <example>30.123456</example>
        [Required(ErrorMessage = "纬度不能为空")]
        [Range(-90, 90, ErrorMessage = "纬度必须在-90到90之间")]
        public decimal Latitude { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        /// <example>120.123456</example>
        [Required(ErrorMessage = "经度不能为空")]
        [Range(-180, 180, ErrorMessage = "经度必须在-180到180之间")]
        public decimal Longitude { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        /// <example>四川省成都市武侯区某某路123号</example>
        [MaxLength(200)]
        public string? Address { get; set; }

        /// <summary>
        /// 充电端口总数
        /// </summary>
        /// <example>10</example>
        [Range(0, int.MaxValue, ErrorMessage = "总端口数不能为负数")]
        public int TotalPorts { get; set; }

        /// <summary>
        /// 当前可用端口数
        /// </summary>
        /// <example>10</example>
        [Range(0, int.MaxValue, ErrorMessage = "可用端口数不能为负数")]
        public int AvailablePorts { get; set; }
    }
} 