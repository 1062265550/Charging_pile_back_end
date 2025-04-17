using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 用户消息通知控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserNotificationsController : ControllerBase
    {
        private readonly IUserNotificationService _userNotificationService;

        public UserNotificationsController(IUserNotificationService userNotificationService)
        {
            _userNotificationService = userNotificationService;
        }

        /// <summary>
        /// 获取用户的所有通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>通知列表</returns>
        [HttpGet("byUser/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            try
            {
                var notifications = await _userNotificationService.GetUserNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户通知列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取用户的未读通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>未读通知列表</returns>
        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUserUnreadNotifications(int userId)
        {
            try
            {
                var notifications = await _userNotificationService.GetUserUnreadNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户未读通知列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取通知详情
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>通知详情</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            try
            {
                var notification = await _userNotificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound($"通知ID '{id}' 不存在");
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取通知详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建用户通知
        /// </summary>
        /// <param name="dto">创建通知请求</param>
        /// <returns>创建的通知ID</returns>
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateUserNotificationDto dto)
        {
            try
            {
                var notificationId = await _userNotificationService.CreateNotificationAsync(dto);
                return CreatedAtAction(nameof(GetNotificationById), new { id = notificationId }, new { id = notificationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"创建用户通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量创建用户通知
        /// </summary>
        /// <param name="dto">批量创建通知请求</param>
        /// <returns>创建的通知数量</returns>
        [HttpPost("batch")]
        public async Task<IActionResult> BatchCreateNotifications([FromBody] BatchCreateUserNotificationDto dto)
        {
            try
            {
                var count = await _userNotificationService.BatchCreateNotificationsAsync(dto);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"批量创建用户通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>标记结果</returns>
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                var result = await _userNotificationService.MarkNotificationAsReadAsync(id);
                if (!result)
                {
                    return NotFound($"通知ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"标记通知为已读失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量标记通知为已读
        /// </summary>
        /// <param name="dto">批量标记已读请求</param>
        /// <returns>标记成功的数量</returns>
        [HttpPut("batchRead")]
        public async Task<IActionResult> BatchMarkNotificationsAsRead([FromBody] BatchMarkNotificationReadDto dto)
        {
            try
            {
                var count = await _userNotificationService.BatchMarkNotificationsAsReadAsync(dto);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"批量标记通知为已读失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 标记用户所有通知为已读
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>标记成功的数量</returns>
        [HttpPut("readAll/{userId}")]
        public async Task<IActionResult> MarkAllNotificationsAsRead(int userId)
        {
            try
            {
                var count = await _userNotificationService.MarkAllNotificationsAsReadAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"标记所有通知为已读失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var result = await _userNotificationService.DeleteNotificationAsync(id);
                if (!result)
                {
                    return NotFound($"通知ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"删除通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取用户未读通知数量
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>未读通知数量</returns>
        [HttpGet("unreadCount/{userId}")]
        public async Task<IActionResult> GetUserUnreadNotificationCount(int userId)
        {
            try
            {
                var count = await _userNotificationService.GetUserUnreadNotificationCountAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户未读通知数量失败: {ex.Message}");
            }
        }
    }
}
