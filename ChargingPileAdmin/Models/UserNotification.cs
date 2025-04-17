using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 用户消息通知
    /// </summary>
    public class UserNotification
    {
        /// <summary>
        /// 通知唯一标识符
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// 通知标题
        /// </summary>
        [Required]
        [Column("title")]
        [StringLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// 通知内容
        /// </summary>
        [Required]
        [Column("content")]
        [StringLength(1000)]
        public string Content { get; set; }

        /// <summary>
        /// 通知类型：1-系统通知，2-订单通知，3-活动通知，4-充值通知
        /// </summary>
        [Column("type")]
        public int Type { get; set; }

        /// <summary>
        /// 相关ID，如订单ID
        /// </summary>
        [Column("related_id")]
        [StringLength(50)]
        public string RelatedId { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        [Column("is_read")]
        public bool IsRead { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        [Column("read_time")]
        public DateTime? ReadTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        // 导航属性
        public virtual User User { get; set; }
    }
}
