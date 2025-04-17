using ChargingPileAdmin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 用户消息通知服务接口
    /// </summary>
    public interface IUserNotificationService
    {
        /// <summary>
        /// 获取用户的所有通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>通知列表</returns>
        Task<IEnumerable<UserNotificationDto>> GetUserNotificationsAsync(int userId);

        /// <summary>
        /// 获取用户的未读通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>未读通知列表</returns>
        Task<IEnumerable<UserNotificationDto>> GetUserUnreadNotificationsAsync(int userId);

        /// <summary>
        /// 获取通知详情
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>通知详情</returns>
        Task<UserNotificationDto> GetNotificationByIdAsync(int id);

        /// <summary>
        /// 创建用户通知
        /// </summary>
        /// <param name="dto">创建通知请求</param>
        /// <returns>创建的通知ID</returns>
        Task<int> CreateNotificationAsync(CreateUserNotificationDto dto);

        /// <summary>
        /// 批量创建用户通知
        /// </summary>
        /// <param name="dto">批量创建通知请求</param>
        /// <returns>创建的通知数量</returns>
        Task<int> BatchCreateNotificationsAsync(BatchCreateUserNotificationDto dto);

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>是否成功</returns>
        Task<bool> MarkNotificationAsReadAsync(int id);

        /// <summary>
        /// 批量标记通知为已读
        /// </summary>
        /// <param name="dto">批量标记已读请求</param>
        /// <returns>标记成功的数量</returns>
        Task<int> BatchMarkNotificationsAsReadAsync(BatchMarkNotificationReadDto dto);

        /// <summary>
        /// 标记用户所有通知为已读
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>标记成功的数量</returns>
        Task<int> MarkAllNotificationsAsReadAsync(int userId);

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteNotificationAsync(int id);

        /// <summary>
        /// 获取用户未读通知数量
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>未读通知数量</returns>
        Task<int> GetUserUnreadNotificationCountAsync(int userId);
    }
}
