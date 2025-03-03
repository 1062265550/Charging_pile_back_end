using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingPile.API.Data;
using ChargingPile.API.Models.Entities;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 用户管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取用户个人资料
        /// </summary>
        /// <param name="openId">微信OpenID</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     GET /api/User/profile?openId=wx123456789
        /// </remarks>
        /// <response code="200">返回用户信息和当前充电状态</response>
        /// <response code="404">用户不存在</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfile([FromQuery] string openId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.OpenId == openId);

            if (user == null)
            {
                return NotFound();
            }

            // 获取用户正在充电的订单数量
            var chargingCount = await _context.Orders
                .CountAsync(o => o.UserId == user.Id && o.Status == 0);

            var result = new
            {
                user.Id,
                user.Nickname,
                user.Avatar,
                user.Balance,
                user.Points,
                ChargingCount = chargingCount
            };

            return Ok(result);
        }

        /// <summary>
        /// 用户登录或注册
        /// </summary>
        /// <param name="openId">微信OpenID</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     POST /api/User/login
        ///     "wx123456789"
        /// 
        /// 说明：
        /// - 如果用户不存在，将自动注册新用户
        /// - 如果用户已存在，将更新最后登录时间
        /// </remarks>
        /// <response code="200">返回用户信息</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(User), 200)]
        public async Task<IActionResult> Login([FromBody] string openId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.OpenId == openId);

            if (user == null)
            {
                // 新用户注册
                user = new User
                {
                    OpenId = openId,
                    CreateTime = DateTime.UtcNow,
                    Balance = 0,
                    Points = 0
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            user.LastLoginTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(user);
        }
    }
} 