using Microsoft.AspNetCore.Mvc;

namespace ChargingPile.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// 获取API信息
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                name = "微信小程序后端微信小程序后端API接口项目",
                version = "1.0",
                status = "框架就绪",
                message = "充电桩后端项目API接口"
            });
        }
    }
} 