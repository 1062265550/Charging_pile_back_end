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
    /// 充电桩管理 - 提供充电桩相关的增删改查操作
    /// </summary>
    /// <remarks>
    /// 本控制器负责充电桩的所有管理功能，包括：
    /// - 充电桩信息的查询（按ID、按充电站、分页查询等）
    /// - 充电桩的创建、更新和删除
    /// - 充电桩状态管理
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargingPilesController : ControllerBase
    {
        private readonly IChargingPileService _pileService;

        public ChargingPilesController(IChargingPileService pileService)
        {
            _pileService = pileService;
        }

        /// <summary>
        /// 获取所有充电桩信息
        /// </summary>
        /// <remarks>
        /// 返回系统中所有充电桩的完整列表，包含充电桩基本信息、状态等数据
        /// 
        /// 使用场景：
        /// - 管理后台展示所有充电桩
        /// - 数据分析和统计
        /// </remarks>
        /// <returns>充电桩列表</returns>
        /// <response code="200">成功返回充电桩列表</response>
        /// <response code="500">服务器错误</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ChargingPileDto>), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetAllPiles()
        {
            try
            {
                var piles = await _pileService.GetAllPilesAsync();
                return Ok(piles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电桩列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定ID的充电桩详细信息
        /// </summary>
        /// <remarks>
        /// 根据充电桩ID获取单个充电桩的详细信息，包括：
        /// - 基本信息（编号、名称、位置等）
        /// - 技术参数（额定功率、电压、电流等）
        /// - 状态信息（在线状态、使用状态等）
        /// - 充电端口信息
        /// 
        /// 使用场景：
        /// - 查看充电桩详情页面
        /// - 充电桩管理和监控
        /// </remarks>
        /// <param name="id">充电桩ID（唯一标识符）</param>
        /// <returns>充电桩详情</returns>
        /// <response code="200">成功返回充电桩详情</response>
        /// <response code="400">ID参数错误</response>
        /// <response code="404">未找到指定ID的充电桩</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ChargingPileDto), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetPileById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("充电桩ID不能为空");
                }
                
                var pile = await _pileService.GetPileByIdAsync(id);
                if (pile == null)
                {
                    return NotFound($"充电桩ID '{id}' 不存在");
                }

                return Ok(pile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电桩详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定充电站下的所有充电桩
        /// </summary>
        /// <remarks>
        /// 根据充电站ID查询该充电站下的所有充电桩信息，支持管理系统按充电站筛选查看充电桩
        /// 
        /// 使用场景：
        /// - 充电站详情页查看所属充电桩
        /// - 充电站管理和监控
        /// - 数据统计分析
        /// </remarks>
        /// <param name="stationId">充电站ID（唯一标识符）</param>
        /// <returns>充电桩列表</returns>
        /// <response code="200">成功返回充电桩列表</response>
        /// <response code="400">充电站ID参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("byStation/{stationId}")]
        [ProducesResponseType(typeof(IEnumerable<ChargingPileDto>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetPilesByStationId(string stationId)
        {
            try
            {
                if (string.IsNullOrEmpty(stationId))
                {
                    return BadRequest("充电站ID不能为空");
                }
                
                var piles = await _pileService.GetPilesByStationIdAsync(stationId);
                return Ok(piles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电站下的充电桩列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 分页获取充电桩列表（支持多条件筛选）
        /// </summary>
        /// <remarks>
        /// 提供带分页功能的充电桩查询接口，支持多种筛选条件：
        /// - 按充电站ID筛选
        /// - 按充电桩状态筛选（在线/离线/充电中/空闲等）
        /// - 按关键字搜索（支持充电桩编号、名称等）
        /// 
        /// 使用场景：
        /// - 管理后台的充电桩列表页
        /// - 带筛选条件的数据查询
        /// - 移动端APP的充电桩查询
        /// </remarks>
        /// <param name="pageNumber">页码，从1开始</param>
        /// <param name="pageSize">每页记录数，默认10条</param>
        /// <param name="stationId">充电站ID，可选，用于筛选特定充电站的充电桩</param>
        /// <param name="status">充电桩状态，可选，用于筛选特定状态的充电桩（0-离线，1-空闲，2-充电中，3-故障）</param>
        /// <param name="keyword">关键字，可选，用于搜索充电桩编号或名称</param>
        /// <returns>充电桩分页数据</returns>
        /// <response code="200">成功返回分页数据</response>
        /// <response code="400">分页参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponseDto<ChargingPileDto>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetPilesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? stationId = null,
            [FromQuery] int? status = null,
            [FromQuery] string? keyword = null)
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
                
                var piles = await _pileService.GetPilesPagedAsync(pageNumber, pageSize, stationId, status, keyword);
                return Ok(piles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电桩分页列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建新的充电桩
        /// </summary>
        /// <remarks>
        /// 创建一个新的充电桩记录，需要提供充电桩的基本信息、所属充电站、技术参数等
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "stationId": "001",
        ///   "pileNo": "PILE001",
        ///   "name": "快充桩1号",
        ///   "type": 1,
        ///   "power": 60,
        ///   "voltage": 220,
        ///   "current": 125,
        ///   "status": 1,
        ///   "location": "停车场东侧",
        ///   "manufacturer": "特斯拉"
        /// }
        /// ```
        /// 
        /// 使用场景：
        /// - 管理员添加新充电桩
        /// - 系统初始化导入充电桩数据
        /// </remarks>
        /// <param name="dto">创建充电桩的数据传输对象</param>
        /// <returns>创建的充电桩ID</returns>
        /// <response code="201">充电桩创建成功，返回新创建的充电桩ID</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="500">服务器错误</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CreatePile([FromBody] CreateChargingPileDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                var pileId = await _pileService.CreatePileAsync(dto);
                return CreatedAtAction(nameof(GetPileById), new { id = pileId }, new { id = pileId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"创建充电桩失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新充电桩信息
        /// </summary>
        /// <remarks>
        /// 根据充电桩ID更新充电桩的信息，可以更新的内容包括：
        /// - 充电桩基本信息（名称、位置等）
        /// - 技术参数（功率、电压、电流等）
        /// - 状态信息（在线状态、使用状态等）
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "name": "快充桩1号(已升级)",
        ///   "status": 1,
        ///   "location": "停车场东北角",
        ///   "manufacturer": "特斯拉"
        /// }
        /// ```
        /// 
        /// 使用场景：
        /// - 管理员修改充电桩信息
        /// - 系统自动更新充电桩状态
        /// </remarks>
        /// <param name="id">充电桩ID</param>
        /// <param name="dto">更新充电桩的数据传输对象</param>
        /// <returns>更新结果</returns>
        /// <response code="204">充电桩更新成功</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="404">未找到指定ID的充电桩</response>
        /// <response code="500">服务器错误</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> UpdatePile(string id, [FromBody] UpdateChargingPileDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("充电桩ID不能为空");
                }
                
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                var result = await _pileService.UpdatePileAsync(id, dto);
                if (!result)
                {
                    return NotFound($"充电桩ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"更新充电桩失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除充电桩
        /// </summary>
        /// <remarks>
        /// 根据充电桩ID删除指定的充电桩记录。删除前会进行以下检查：
        /// - 充电桩是否存在
        /// - 充电桩是否有未完成的订单
        /// - 充电桩是否处于充电中状态
        /// 
        /// 使用场景：
        /// - 管理员移除不再使用的充电桩
        /// - 系统维护清理废弃数据
        /// </remarks>
        /// <param name="id">充电桩ID</param>
        /// <returns>删除结果</returns>
        /// <response code="204">充电桩删除成功</response>
        /// <response code="400">ID参数错误</response>
        /// <response code="404">未找到指定ID的充电桩</response>
        /// <response code="500">服务器错误</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> DeletePile(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("充电桩ID不能为空");
                }
                
                var result = await _pileService.DeletePileAsync(id);
                if (!result)
                {
                    return NotFound($"充电桩ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"删除充电桩失败: {ex.Message}");
            }
        }
    }
}
