using System;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 用户消息通知数据传输对象
    /// </summary>
    public class UserNotificationDto
    {
        /// <summary>
        /// 通知唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Content { get; set; } = "";

        /// <summary>
        /// 通知类型：1-系统通知，2-订单通知，3-活动通知，4-充值通知
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 通知类型描述
        /// </summary>
        public string TypeDescription => Type switch
        {
            1 => "系统通知",
            2 => "订单通知",
            3 => "活动通知",
            4 => "充值通知",
            _ => "未知"
        };

        /// <summary>
        /// 相关ID，如订单ID
        /// </summary>
        public string RelatedId { get; set; } = "";

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        public DateTime? ReadTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 创建用户通知请求
    /// </summary>
    public class CreateUserNotificationDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Content { get; set; } = "";

        /// <summary>
        /// 通知类型：1-系统通知，2-订单通知，3-活动通知，4-充值通知
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 相关ID，如订单ID
        /// </summary>
        public string RelatedId { get; set; } = "";
    }

    /// <summary>
    /// 批量创建用户通知请求
    /// </summary>
    public class BatchCreateUserNotificationDto
    {
        /// <summary>
        /// 用户ID数组
        /// </summary>
        public int[] UserIds { get; set; } = Array.Empty<int>();

        /// <summary>
        /// 通知标题
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// 通知内容
        /// </summary>
        public string Content { get; set; } = "";

        /// <summary>
        /// 通知类型：1-系统通知，2-订单通知，3-活动通知，4-充值通知
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 相关ID，如订单ID
        /// </summary>
        public string RelatedId { get; set; } = "";
    }

    /// <summary>
    /// 标记通知已读请求
    /// </summary>
    public class MarkNotificationReadDto
    {
        /// <summary>
        /// 通知ID
        /// </summary>
        public int NotificationId { get; set; }
    }

    /// <summary>
    /// 批量标记通知已读请求
    /// </summary>
    public class BatchMarkNotificationReadDto
    {
        /// <summary>
        /// 通知ID数组
        /// </summary>
        public int[] NotificationIds { get; set; } = Array.Empty<int>();
    }

    /// <summary>
    /// 标记所有通知已读请求
    /// </summary>
    public class MarkAllNotificationsReadDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
    }
}
