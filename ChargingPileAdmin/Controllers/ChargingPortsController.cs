using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 充电端口管理接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingPortsController : ControllerBase
    {
        private readonly IChargingPortService _portService;

        public ChargingPortsController(IChargingPortService portService)
        {
            _portService = portService;
        }

        /// <summary>
        /// 获取所有充电端口
        /// </summary>
        /// <returns>充电端口列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllPorts()
        {
            try
            {
                var ports = await _portService.GetAllPortsAsync();
                return Ok(ports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 获取充电端口详情
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <returns>充电端口详情</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPort(string id)
        {
            try
            {
                var port = await _portService.GetPortByIdAsync(id);
                if (port == null)
                {
                    return NotFound(new { message = "充电端口不存在" });
                }
                return Ok(port);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 获取充电桩下的所有充电端口
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电端口列表</returns>
        [HttpGet("by-pile/{pileId}")]
        public async Task<IActionResult> GetPortsByPileId(string pileId)
        {
            try
            {
                var ports = await _portService.GetPortsByPileIdAsync(pileId);
                return Ok(ports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 获取充电端口分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pileId">充电桩ID，可选</param>
        /// <param name="status">充电端口状态，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>充电端口分页列表</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPortsPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string pileId = null,
            [FromQuery] int? status = null,
            [FromQuery] string keyword = null)
        {
            try
            {
                var response = await _portService.GetPortsPagedAsync(pageNumber, pageSize, pileId, status, keyword);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 创建充电端口
        /// </summary>
        /// <param name="dto">创建充电端口请求</param>
        /// <returns>创建的充电端口ID</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePort([FromBody] CreateChargingPortDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var portId = await _portService.CreatePortAsync(dto);
                return CreatedAtAction(nameof(GetPort), new { id = portId }, new { id = portId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 更新充电端口
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <param name="dto">更新充电端口请求</param>
        /// <returns>操作结果</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePort(string id, [FromBody] UpdateChargingPortDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _portService.UpdatePortAsync(id, dto);
                if (!result)
                {
                    return NotFound(new { message = "充电端口不存在" });
                }
                return Ok(new { message = "充电端口更新成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 删除充电端口
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePort(string id)
        {
            try
            {
                var result = await _portService.DeletePortAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "充电端口不存在" });
                }
                return Ok(new { message = "充电端口删除成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
