using System.ComponentModel.DataAnnotations;

namespace ChargingPile.API.Models.Station
{
    /// <summary>
    /// 搜索充电站请求
    /// </summary>
    public class SearchStationRequest
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        [Required(ErrorMessage = "搜索关键词不能为空")]
        [StringLength(50, ErrorMessage = "搜索关键词长度不能超过50个字符")]
        public string Keyword { get; set; }

        /// <summary>
        /// 纬度，可选
        /// </summary>
        [Range(-90, 90, ErrorMessage = "纬度必须在-90到90之间")]
        public double? Latitude { get; set; }

        /// <summary>
        /// 经度，可选
        /// </summary>
        [Range(-180, 180, ErrorMessage = "经度必须在-180到180之间")]
        public double? Longitude { get; set; }

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
