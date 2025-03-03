using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPile.API.Models.Entities
{
    /// <summary>
    /// 用户实体
    /// </summary>
    [Table("users")]
    public class User
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 微信OpenID
        /// </summary>
        /// <example>wx123456789</example>
        [Required]
        [MaxLength(100)]
        [Column("open_id")]
        public required string OpenId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        /// <example>张三</example>
        [MaxLength(50)]
        [Column("nickname")]
        public string? Nickname { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        /// <example>https://thirdwx.qlogo.cn/mmopen/xxx/132</example>
        [MaxLength(200)]
        [Column("avatar")]
        public string? Avatar { get; set; }

        /// <summary>
        /// 账户余额（元）
        /// </summary>
        /// <example>100.00</example>
        [Column("balance")]
        public decimal Balance { get; set; }

        /// <summary>
        /// 积分
        /// </summary>
        /// <example>500</example>
        [Column("points")]
        public int Points { get; set; }

        /// <summary>
        /// 账户创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [Column("last_login_time")]
        public DateTime? LastLoginTime { get; set; }
    }
} 