using Microsoft.AspNetCore.Mvc;
using System;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 健康检查控制器 - 提供系统健康状态监控
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// 获取系统健康状态
        /// </summary>
        /// <remarks>
        /// 返回系统健康状态信息，用于监控系统可用性
        /// </remarks>
        /// <returns>健康状态信息</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult Get()
        {
            try
            {
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
