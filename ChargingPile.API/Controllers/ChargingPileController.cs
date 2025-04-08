using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Services;
using System.ComponentModel.DataAnnotations;
using System;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 充电桩控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargingPileController : ControllerBase
    {
        private readonly ILogger<ChargingPileController> _logger;
        private readonly ChargingPileService _chargingPileService;

        public ChargingPileController(
            ILogger<ChargingPileController> logger,
            ChargingPileService chargingPileService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chargingPileService = chargingPileService ?? throw new ArgumentNullException(nameof(chargingPileService));
        }

        /// <summary>
        /// 启动充电
        /// </summary>
        /// <param name="deviceAddress">设备地址</param>
        /// <param name="request">启动充电请求</param>
        /// <returns>操作结果</returns>
        /// <response code="200">启动充电命令已发送</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">设备未连接</response>
        [HttpPost("{deviceAddress}/start")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StartCharging(
            [FromRoute][Required] string deviceAddress,
            [FromBody] StartChargingRequest request)
        {
            try
            {
                _logger.LogInformation("收到启动充电请求：设备 {DeviceAddress}, 订单 {OrderId}", deviceAddress, request.OrderId);
                
                var result = await _chargingPileService.StartChargingAsync(
                    deviceAddress,          // pileId
                    request.Port,           // 端口号
                    request.OrderId,        // 订单号
                    request.StartMode,      // 启动方式
                    request.CardId,         // 卡号
                    request.ChargingMode,   // 充电方式
                    request.ChargingParam,  // 充电参数
                    request.AvailableAmount // 可用金额
                );
                
                if (result.Success)
                {
                    return Ok(new { message = "启动充电命令已发送" });
                }
                else
                {
                    // 根据错误信息判断返回状态码
                    if (result.Message.Contains("未找到") || result.Message.Contains("未连接") || result.Message.Contains("离线"))
                    {
                        return NotFound(new { error = result.Message });
                    }
                    else if (result.Message.Contains("已存在") || result.Message.Contains("正在充电中") || result.Message.Contains("请使用新的订单号"))
                    {
                        return BadRequest(new { error = result.Message });
                    }
                    else
                    {
                        return StatusCode(500, new { error = result.Message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "启动充电请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动充电操作失败");
                return StatusCode(500, new { error = "启动充电操作失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 停止充电
        /// </summary>
        /// <param name="deviceAddress">设备地址</param>
        /// <param name="request">停止充电请求</param>
        /// <returns>操作结果</returns>
        /// <response code="200">停止充电命令已发送</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">设备未连接</response>
        [HttpPost("{deviceAddress}/stop")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StopCharging(
            [FromRoute][Required] string deviceAddress,
            [FromBody] StopChargingRequest request)
        {
            try
            {
                _logger.LogInformation("收到停止充电请求：设备 {DeviceAddress}, 订单 {OrderId}", deviceAddress, request.OrderId);
                
                var result = await _chargingPileService.StopChargingAsync(
                    deviceAddress,   // pileId
                    request.Port,    // 端口号
                    request.OrderId  // 订单号
                );
                
                if (result.Success)
                {
                    return Ok(new { message = "停止充电命令已发送" });
                }
                else if (result.Message.Contains("未连接"))
                {
                    return NotFound(new { error = result.Message });
                }
                else
                {
                    return BadRequest(new { error = result.Message });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "停止充电请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理停止充电请求时发生异常");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 查询充电状态
        /// </summary>
        /// <param name="deviceAddress">设备地址</param>
        /// <param name="port">充电端口号</param>
        /// <returns>操作结果</returns>
        /// <response code="200">查询命令已发送</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">设备未连接</response>
        [HttpGet("{deviceAddress}/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetChargingStatus(
            [FromRoute][Required] string deviceAddress,
            [FromQuery] byte port = 1)
        {
            try
            {
                _logger.LogDebug("收到查询充电状态请求：设备 {DeviceAddress}, 端口 {Port}", deviceAddress, port);
                
                var result = await _chargingPileService.QueryChargingStatusAsync(deviceAddress, port);
                
                if (result.Success)
                {
                    return Ok(new { message = "查询充电状态命令已发送" });
                }
                else
                {
                    // 根据错误信息判断返回状态码
                    if (result.Message.Contains("未找到") || result.Message.Contains("未连接") || result.Message.Contains("离线"))
                    {
                        return NotFound(new { error = result.Message });
                    }
                    else
                    {
                        return BadRequest(new { error = result.Message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "查询充电状态请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询充电状态请求处理异常");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 配置充电桩
        /// </summary>
        [HttpPost("{deviceAddress}/config")]
        public async Task<IActionResult> ConfigureChargingPile(
            string deviceAddress,
            [FromBody] byte[] config)
        {
            try
            {
                _logger.LogInformation("收到配置充电桩请求：设备 {DeviceAddress}", deviceAddress);
                
                var result = await _chargingPileService.ConfigureChargingPileAsync(deviceAddress, config);
                
                if (result.Success)
                {
                    return Ok(new { message = "配置命令已发送" });
                }
                else
                {
                    // 根据错误信息判断返回状态码
                    if (result.Message.Contains("未找到") || result.Message.Contains("未连接") || result.Message.Contains("离线"))
                    {
                        return NotFound(new { error = result.Message });
                    }
                    else
                    {
                        return BadRequest(new { error = result.Message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "配置充电桩请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置充电桩请求处理异常");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }
    }

    /// <summary>
    /// 启动充电请求
    /// </summary>
    public class StartChargingRequest
    {
        /// <summary>
        /// 端口号 (1-N)
        /// </summary>
        [Required(ErrorMessage = "端口号不能为空")]
        [Range(1, 255, ErrorMessage = "端口号必须在1-255之间")]
        public byte Port { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required(ErrorMessage = "订单号不能为空")]
        public uint OrderId { get; set; }

        /// <summary>
        /// 启动方式 (1:扫码支付 2:刷卡支付 3:管理员启动)
        /// </summary>
        [Required(ErrorMessage = "启动方式不能为空")]
        [Range(1, 3, ErrorMessage = "启动方式必须是1-3之间的值")]
        public byte StartMode { get; set; }

        /// <summary>
        /// 卡号 (非IC卡启动时为0)
        /// </summary>
        [Required(ErrorMessage = "卡号不能为空")]
        public uint CardId { get; set; }

        /// <summary>
        /// 充电方式 (1:充满自停 2:按金额 3:按时间 4:按电量 5:其它)
        /// </summary>
        [Required(ErrorMessage = "充电方式不能为空")]
        [Range(1, 5, ErrorMessage = "充电方式必须是1-5之间的值")]
        public byte ChargingMode { get; set; }

        /// <summary>
        /// 充电参数 (秒/0.01元/0.01度)
        /// </summary>
        [Required(ErrorMessage = "充电参数不能为空")]
        public uint ChargingParam { get; set; }

        /// <summary>
        /// 可用金额 (0.01元)
        /// </summary>
        [Required(ErrorMessage = "可用金额不能为空")]
        public uint AvailableAmount { get; set; }
    }

    /// <summary>
    /// 停止充电请求
    /// </summary>
    public class StopChargingRequest
    {
        /// <summary>
        /// 端口号 (1-N)
        /// </summary>
        [Required(ErrorMessage = "端口号不能为空")]
        [Range(1, 255, ErrorMessage = "端口号必须在1-255之间")]
        public byte Port { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required(ErrorMessage = "订单号不能为空")]
        public uint OrderId { get; set; }
    }
} 