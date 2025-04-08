using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 充电站管理 - 提供充电站相关的增删改查操作
    /// </summary>
    /// <remarks>
    /// 本控制器负责充电站的所有管理功能，包括：
    /// - 充电站信息的查询（按ID、分页查询等）
    /// - 充电站的创建、更新和删除
    /// - 充电站基础数据维护
    /// 
    /// 充电站是充电桩的集合，代表一个物理位置上的充电设施群
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargingStationsController : ControllerBase
    {
        private readonly IChargingStationService _stationService;

        public ChargingStationsController(IChargingStationService stationService)
        {
            _stationService = stationService;
        }

        /// <summary>
        /// 获取所有充电站信息
        /// </summary>
        /// <remarks>
        /// 返回系统中所有充电站的完整列表，包含充电站基本信息、地址、状态等数据
        /// 
        /// 使用场景：
        /// - 充电站地图展示
        /// - 管理后台展示所有充电站
        /// - 数据分析和统计
        /// 
        /// 注意：当充电站数量较多时，建议使用分页接口获取数据
        /// </remarks>
        /// <returns>充电站列表</returns>
        /// <response code="200">成功返回充电站列表</response>
        /// <response code="500">服务器错误</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ChargingStationDto>), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetAllStations()
        {
            try
            {
                var stations = await _stationService.GetAllStationsAsync();
                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电站列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定ID的充电站详细信息
        /// </summary>
        /// <remarks>
        /// 根据充电站ID获取单个充电站的详细信息，包括：
        /// - 基本信息（名称、地址、联系方式等）
        /// - 运营信息（营业时间、收费标准等）
        /// - 设备信息（充电桩数量、可用状态等）
        /// - 状态信息（开放状态、维护状态等）
        /// 
        /// 使用场景：
        /// - 查看充电站详情页面
        /// - 充电站管理和监控
        /// - APP中展示充电站详情
        /// </remarks>
        /// <param name="id">充电站ID（唯一标识符）</param>
        /// <returns>充电站详情</returns>
        /// <response code="200">成功返回充电站详情</response>
        /// <response code="404">未找到指定ID的充电站</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ChargingStationDto), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetStationById(string id)
        {
            try
            {
                var station = await _stationService.GetStationByIdAsync(id);
                if (station == null)
                {
                    return NotFound($"充电站ID '{id}' 不存在");
                }

                return Ok(station);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电站详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 分页获取充电站列表（支持条件筛选）
        /// </summary>
        /// <remarks>
        /// 提供带分页功能的充电站查询接口，支持筛选条件：
        /// - 按充电站状态筛选（开放/关闭/维护中等）
        /// - 按关键字搜索（支持充电站名称、地址等）
        /// 
        /// 使用场景：
        /// - 管理后台的充电站列表页
        /// - 带筛选条件的充电站查询
        /// - APP中的充电站查询
        /// 
        /// 充电站状态说明：
        /// - 0：关闭（不对外开放）
        /// - 1：开放（正常运营）
        /// - 2：维护中（暂停服务）
        /// </remarks>
        /// <param name="pageNumber">页码，从1开始</param>
        /// <param name="pageSize">每页记录数，默认10条</param>
        /// <param name="status">充电站状态，可选，用于筛选特定状态的充电站</param>
        /// <param name="keyword">关键字，可选，用于搜索充电站名称或地址</param>
        /// <returns>充电站分页数据</returns>
        /// <response code="200">成功返回分页数据</response>
        /// <response code="400">分页参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponseDto<ChargingStationDto>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetStationsPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? status = null,
            [FromQuery] string keyword = null)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("页码必须大于等于1");
                }

                if (pageSize < 1)
                {
                    return BadRequest("每页记录数必须大于等于1");
                }
                
                var stations = await _stationService.GetStationsPagedAsync(pageNumber, pageSize, status, keyword);
                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电站分页列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建新的充电站
        /// </summary>
        /// <remarks>
        /// 创建一个新的充电站记录，需要提供充电站的基本信息、地址、联系方式等
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "stationNo": "ST001",
        ///   "name": "城市中心充电站",
        ///   "address": "北京市朝阳区建国路88号",
        ///   "latitude": 39.9088,
        ///   "longitude": 116.4716,
        ///   "contactPerson": "张经理",
        ///   "contactPhone": "13800138000",
        ///   "openTime": "08:00-22:00",
        ///   "status": 1,
        ///   "description": "提供快充和慢充服务，配备休息区和便利店"
        /// }
        /// ```
        /// 
        /// 使用场景：
        /// - 管理员添加新充电站
        /// - 系统初始化导入充电站数据
        /// </remarks>
        /// <param name="dto">创建充电站的数据传输对象</param>
        /// <returns>创建的充电站ID</returns>
        /// <response code="201">充电站创建成功，返回新创建的充电站ID</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="500">服务器错误</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CreateStation([FromBody] CreateChargingStationDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                var stationId = await _stationService.CreateStationAsync(dto);
                return CreatedAtAction(nameof(GetStationById), new { id = stationId }, new { id = stationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"创建充电站失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新充电站信息
        /// </summary>
        /// <remarks>
        /// 根据充电站ID更新充电站的信息，可以更新的内容包括：
        /// - 充电站基本信息（名称、地址、联系方式等）
        /// - 运营信息（营业时间、收费标准等）
        /// - 状态信息（开放状态、维护状态等）
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "name": "城市中心超级充电站",
        ///   "contactPerson": "李经理",
        ///   "contactPhone": "13900139000",
        ///   "openTime": "07:00-23:00",
        ///   "status": 1,
        ///   "description": "全新升级，新增10个快充桩，配备高级休息区和自助咖啡厅"
        /// }
        /// ```
        /// 
        /// 使用场景：
        /// - 管理员修改充电站信息
        /// - 更新充电站运营状态
        /// - 调整充电站服务信息
        /// </remarks>
        /// <param name="id">充电站ID</param>
        /// <param name="dto">更新充电站的数据传输对象</param>
        /// <returns>更新结果</returns>
        /// <response code="204">充电站更新成功</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="404">未找到指定ID的充电站</response>
        /// <response code="500">服务器错误</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> UpdateStation(string id, [FromBody] UpdateChargingStationDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("充电站ID不能为空");
                }
                
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                var result = await _stationService.UpdateStationAsync(id, dto);
                if (!result)
                {
                    return NotFound($"充电站ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"更新充电站失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除充电站
        /// </summary>
        /// <remarks>
        /// 根据充电站ID删除指定的充电站记录。删除前会进行以下检查：
        /// - 充电站是否存在
        /// - 充电站下是否有充电桩
        /// - 充电站是否有未完成的订单
        /// 
        /// 使用场景：
        /// - 管理员移除不再使用的充电站
        /// - 系统维护清理废弃数据
        /// 
        /// 注意：删除充电站将同时删除与之关联的所有充电桩和历史数据
        /// </remarks>
        /// <param name="id">充电站ID</param>
        /// <returns>删除结果</returns>
        /// <response code="204">充电站删除成功</response>
        /// <response code="400">ID参数错误</response>
        /// <response code="404">未找到指定ID的充电站</response>
        /// <response code="500">服务器错误</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> DeleteStation(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("充电站ID不能为空");
                }
                
                var result = await _stationService.DeleteStationAsync(id);
                if (!result)
                {
                    return NotFound($"充电站ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"删除充电站失败: {ex.Message}");
            }
        }
    }
}
