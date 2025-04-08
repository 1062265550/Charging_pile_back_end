using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Communication;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 设备控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly TcpServer _tcpServer;

        public DevicesController(
            ILogger<DevicesController> logger,
            TcpServer tcpServer)
        {
            _logger = logger;
            _tcpServer = tcpServer;
        }

        /// <summary>
        /// 获取当前已连接的设备列表
        /// </summary>
        /// <returns>设备列表</returns>
        [HttpGet]
        public IActionResult GetConnectedDevices()
        {
            try
            {
                // 获取已连接的设备IMEI列表
                var connectedDevices = _tcpServer.GetConnectedDevices();
                
                _logger.LogInformation("获取已连接设备列表：共{Count}个设备", connectedDevices.Count);
                return Ok(connectedDevices);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "获取已连接设备列表失败");
                return StatusCode(500, new { error = "获取设备列表失败" });
            }
        }

        /// <summary>
        /// 获取设备连接状态
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <returns>设备连接状态</returns>
        [HttpGet("{imei}/status")]
        public IActionResult GetDeviceStatus(string imei)
        {
            try
            {
                var endpoint = _tcpServer.GetDeviceEndPoint(imei);
                bool isConnected = !string.IsNullOrEmpty(endpoint);
                
                _logger.LogInformation("获取设备状态：IMEI={IMEI}, 连接状态={Status}", 
                    imei, isConnected ? "已连接" : "未连接");
                
                return Ok(new { 
                    imei = imei,
                    isConnected = isConnected,
                    endpoint = isConnected ? endpoint : null
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "获取设备状态失败：IMEI={IMEI}", imei);
                return StatusCode(500, new { error = "获取设备状态失败" });
            }
        }
    }
} 