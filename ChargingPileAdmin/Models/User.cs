using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户唯一标识符
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 用户开放平台ID
        /// </summary>
        [Required]
        [Column("open_id")]
        [StringLength(100)]
        public string OpenId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        [Column("nickname")]
        [StringLength(50)]
        public string? Nickname { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        [Column("avatar")]
        [StringLength(200)]
        public string? Avatar { get; set; }

        /// <summary>
        /// 用户密码（加密存储）
        /// </summary>
        [Column("password")]
        [StringLength(100)]
        public string? Password { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [Column("balance")]
        public decimal Balance { get; set; }

        /// <summary>
        /// 用户积分
        /// </summary>
        [Column("points")]
        public int Points { get; set; }

        /// <summary>
        /// 性别：0-未知，1-男，2-女
        /// </summary>
        [Column("gender")]
        public int? Gender { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        [Column("country")]
        [StringLength(50)]
        public string? Country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        [Column("province")]
        [StringLength(50)]
        public string? Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [Column("city")]
        [StringLength(50)]
        public string? City { get; set; }

        /// <summary>
        /// 语言偏好
        /// </summary>
        [Column("language")]
        [StringLength(20)]
        public string? Language { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Column("last_login_time")]
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ICollection<Order> Orders { get; set; }
    }
}
