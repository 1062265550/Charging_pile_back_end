using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Models;
using ChargingPileAdmin.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 用户消息通知服务实现
    /// </summary>
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IRepository<UserNotification> _notificationRepository;
        private readonly IRepository<User> _userRepository;

        public UserNotificationService(
            IRepository<UserNotification> notificationRepository,
            IRepository<User> userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 获取用户的所有通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>通知列表</returns>
        public async Task<IEnumerable<UserNotificationDto>> GetUserNotificationsAsync(int userId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 获取用户通知
            var notifications = await _notificationRepository.GetAsync(
                n => n.UserId == userId);

            // 手动排序结果
            var sortedNotifications = notifications.OrderByDescending(n => n.CreateTime);
            
            return sortedNotifications.Select(MapToDto);
        }

        /// <summary>
        /// 获取用户的未读通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>未读通知列表</returns>
        public async Task<IEnumerable<UserNotificationDto>> GetUserUnreadNotificationsAsync(int userId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 获取用户未读通知
            var notifications = await _notificationRepository.GetAsync(
                n => n.UserId == userId && !n.IsRead);

            // 手动排序结果
            var sortedNotifications = notifications.OrderByDescending(n => n.CreateTime);
            
            return sortedNotifications.Select(MapToDto);
        }

        /// <summary>
        /// 获取通知详情
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>通知详情</returns>
        public async Task<UserNotificationDto> GetNotificationByIdAsync(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return new UserNotificationDto(); // 返回空对象而不是null
            }

            return MapToDto(notification);
        }

        /// <summary>
        /// 创建用户通知
        /// </summary>
        /// <param name="dto">创建通知请求</param>
        /// <returns>创建的通知ID</returns>
        public async Task<int> CreateNotificationAsync(CreateUserNotificationDto dto)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new Exception($"用户ID '{dto.UserId}' 不存在");
            }

            // 创建通知
            var notification = new UserNotification
            {
                UserId = dto.UserId,
                Title = dto.Title ?? "",
                Content = dto.Content ?? "",
                Type = dto.Type,
                RelatedId = dto.RelatedId ?? "",
                IsRead = false,
                CreateTime = DateTime.Now
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            return notification.Id;
        }

        /// <summary>
        /// 批量创建用户通知
        /// </summary>
        /// <param name="dto">批量创建通知请求</param>
        /// <returns>创建的通知数量</returns>
        public async Task<int> BatchCreateNotificationsAsync(BatchCreateUserNotificationDto dto)
        {
            if (dto.UserIds == null || !dto.UserIds.Any())
            {
                throw new Exception("用户ID列表不能为空");
            }

            // 检查用户是否存在
            var userIds = dto.UserIds.Distinct().ToArray();
            var users = await _userRepository.GetAsync(u => userIds.Contains(u.Id));
            var existingUserIds = users.Select(u => u.Id).ToArray();
            var nonExistingUserIds = userIds.Except(existingUserIds).ToArray();

            if (nonExistingUserIds.Any())
            {
                throw new Exception($"以下用户ID不存在: {string.Join(", ", nonExistingUserIds)}");
            }

            // 创建通知
            var now = DateTime.Now;
            var notifications = new List<UserNotification>();

            foreach (var userId in userIds)
            {
                var notification = new UserNotification
                {
                    UserId = userId,
                    Title = dto.Title ?? "",
                    Content = dto.Content ?? "",
                    Type = dto.Type,
                    RelatedId = dto.RelatedId ?? "",
                    IsRead = false,
                    CreateTime = now
                };
                
                await _notificationRepository.AddAsync(notification);
                notifications.Add(notification);
            }

            await _notificationRepository.SaveChangesAsync();

            return notifications.Count;
        }

        /// <summary>
        /// 标记通知为已读
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> MarkNotificationAsReadAsync(int id)
        {
            // 检查通知是否存在
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return false;
            }

            // 检查通知是否已读
            if (notification.IsRead)
            {
                return true;
            }

            // 标记为已读
            notification.IsRead = true;
            notification.ReadTime = DateTime.Now;
            _notificationRepository.Update(notification);
            await _notificationRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 批量标记通知为已读
        /// </summary>
        /// <param name="dto">批量标记已读请求</param>
        /// <returns>标记成功的数量</returns>
        public async Task<int> BatchMarkNotificationsAsReadAsync(BatchMarkNotificationReadDto dto)
        {
            if (dto.NotificationIds == null || !dto.NotificationIds.Any())
            {
                throw new Exception("通知ID列表不能为空");
            }

            // 获取未读通知
            var notificationIds = dto.NotificationIds.Distinct().ToArray();
            var notifications = await _notificationRepository.GetAsync(
                n => notificationIds.Contains(n.Id) && !n.IsRead);

            if (!notifications.Any())
            {
                return 0;
            }

            // 批量标记为已读
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadTime = DateTime.Now;
                _notificationRepository.Update(notification);
            }

            await _notificationRepository.SaveChangesAsync();

            return notifications.Count();
        }

        /// <summary>
        /// 标记用户所有通知为已读
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>标记成功的数量</returns>
        public async Task<int> MarkAllNotificationsAsReadAsync(int userId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 获取用户未读通知
            var notifications = await _notificationRepository.GetAsync(
                n => n.UserId == userId && !n.IsRead);

            if (!notifications.Any())
            {
                return 0;
            }

            // 批量标记为已读
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadTime = DateTime.Now;
                _notificationRepository.Update(notification);
            }

            await _notificationRepository.SaveChangesAsync();

            return notifications.Count();
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="id">通知ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteNotificationAsync(int id)
        {
            // 检查通知是否存在
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                return false;
            }

            // 删除通知
            _notificationRepository.Delete(notification);
            await _notificationRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 获取用户未读通知数量
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>未读通知数量</returns>
        public async Task<int> GetUserUnreadNotificationCountAsync(int userId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 获取用户未读通知数量
            var count = (await _notificationRepository.GetAsync(
                n => n.UserId == userId && !n.IsRead)).Count();

            return count;
        }

        /// <summary>
        /// 将通知实体映射为DTO
        /// </summary>
        /// <param name="notification">通知实体</param>
        /// <returns>通知DTO</returns>
        private UserNotificationDto MapToDto(UserNotification notification)
        {
            return new UserNotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title ?? "",
                Content = notification.Content ?? "",
                Type = notification.Type,
                RelatedId = notification.RelatedId ?? "",
                IsRead = notification.IsRead,
                ReadTime = notification.ReadTime,
                CreateTime = notification.CreateTime
            };
        }
    }
}
