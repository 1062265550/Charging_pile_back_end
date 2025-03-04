using System.ComponentModel.DataAnnotations;

namespace ChargingPile.API.Models.DTOs
{
    /// <summary>
    /// 充电站更新数据传输对象
    /// </summary>
    public class UpdateChargingStationDTO
    {
        /// <summary>
        /// 充电站名称
        /// </summary>
        /// <example>某某充电站</example>
        [Required(ErrorMessage = "充电站名称不能为空")]
        [StringLength(100, ErrorMessage = "充电站名称不能超过100个字符")]
        public required string Name { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        /// <example>四川省成都市武侯区某某路123号</example>
        [StringLength(200, ErrorMessage = "地址不能超过200个字符")]
        public string? Address { get; set; }

        /// <summary>
        /// 充电端口总数
        /// </summary>
        /// <example>10</example>
        [Range(1, 100, ErrorMessage = "充电端口总数必须在1到100之间")]
        public int TotalPorts { get; set; }

        /// <summary>
        /// 当前可用端口数
        /// </summary>
        /// <example>5</example>
        [Range(0, 100, ErrorMessage = "可用端口数必须在0到100之间")]
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
        [Range(0, 2, ErrorMessage = "状态值必须为0(正常运营)、1(维护中)或2(故障)")]
        public int Status { get; set; }
    }
} 