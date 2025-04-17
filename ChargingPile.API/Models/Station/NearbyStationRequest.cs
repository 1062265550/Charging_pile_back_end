using System.ComponentModel.DataAnnotations;

namespace ChargingPile.API.Models.Station
{
    /// <summary>
    /// 获取附近充电站请求
    /// </summary>
    public class NearbyStationRequest
    {
        /// <summary>
        /// 纬度
        /// </summary>
        [Required(ErrorMessage = "纬度不能为空")]
        [Range(-90, 90, ErrorMessage = "纬度必须在-90到90之间")]
        public double Latitude { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        [Required(ErrorMessage = "经度不能为空")]
        [Range(-180, 180, ErrorMessage = "经度必须在-180到180之间")]
        public double Longitude { get; set; }

        /// <summary>
        /// 搜索半径(米)，默认5000
        /// </summary>
        [Range(100, 50000, ErrorMessage = "搜索半径必须在100到50000米之间")]
        public int Radius { get; set; } = 5000;

        /// <summary>
        /// 页码，默认1
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
        public int PageNum { get; set; } = 1;

        /// <summary>
        /// 每页数量，默认10
        /// </summary>
        [Range(1, 100, ErrorMessage = "每页数量必须在1到100之间")]
        public int PageSize { get; set; } = 10;
    }
}
