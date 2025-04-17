using ChargingPile.API.Models.Station;
using ChargingPile.API.Services;
using ChargingPile.API.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 充电站控制器
    /// </summary>
    [ApiController]
    [Route("api/station")]
    public class StationController : ControllerBase
    {
        private readonly StationService _stationService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stationService">充电站服务</param>
        public StationController(StationService stationService)
        {
            _stationService = stationService;
        }

        /// <summary>
        /// 获取附近充电站列表
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>充电站列表</returns>
        [HttpGet("nearby")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResponse<StationResponse>>>> GetNearbyStations([FromQuery] NearbyStationRequest request)
        {
            try
            {
                var stations = await _stationService.GetNearbyStationsAsync(request);
                return Ok(ApiResponse<PagedResponse<StationResponse>>.Success(stations));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PagedResponse<StationResponse>>.Fail($"获取附近充电站失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取充电站详情
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <returns>充电站详情</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<StationDetailResponse>>> GetStationDetail(string id)
        {
            try
            {
                var station = await _stationService.GetStationDetailAsync(id);
                if (station == null)
                {
                    return NotFound(ApiResponse<StationDetailResponse>.Fail($"未找到ID为{id}的充电站", 404));
                }
                return Ok(ApiResponse<StationDetailResponse>.Success(station));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<StationDetailResponse>.Fail($"获取充电站详情失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 搜索充电站
        /// </summary>
        /// <param name="request">搜索请求参数</param>
        /// <returns>充电站列表</returns>
        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResponse<StationResponse>>>> SearchStations([FromQuery] SearchStationRequest request)
        {
            try
            {
                // 记录请求参数
                Console.WriteLine($"搜索充电站请求参数: Keyword={request.Keyword}, Latitude={request.Latitude}, Longitude={request.Longitude}, PageNum={request.PageNum}, PageSize={request.PageSize}");

                // 验证关键词
                if (string.IsNullOrEmpty(request.Keyword))
                {
                    Console.WriteLine("搜索关键词为空");
                    return BadRequest(ApiResponse<PagedResponse<StationResponse>>.Fail("搜索关键词不能为空"));
                }

                // 安全检查关键词长度
                if (request.Keyword.Length > 50)
                {
                    Console.WriteLine($"关键词过长，原长度: {request.Keyword.Length}");
                    request.Keyword = request.Keyword.Substring(0, 50);
                }

                // 打印关键词的字节表示
                byte[] keywordBytes = System.Text.Encoding.UTF8.GetBytes(request.Keyword);
                string bytesString = BitConverter.ToString(keywordBytes);
                Console.WriteLine($"关键词字节: {bytesString}");

                var stations = await _stationService.SearchStationsAsync(request);
                Console.WriteLine($"搜索成功，返回 {stations.List.Count} 条记录");
                return Ok(ApiResponse<PagedResponse<StationResponse>>.Success(stations));
            }
            catch (Exception ex)
            {
                // 记录详细错误
                Console.WriteLine($"搜索充电站异常: {ex.Message}");
                Console.WriteLine($"异常堆栈: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.Message}");
                }

                return BadRequest(ApiResponse<PagedResponse<StationResponse>>.Fail($"搜索充电站失败: {ex.Message}"));
            }
        }
    }
}
