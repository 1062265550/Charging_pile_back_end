using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 用户地址信息
    /// </summary>
    public class UserAddress
    {
        /// <summary>
        /// 地址唯一标识符
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
        /// 收件人姓名
        /// </summary>
        [Required]
        [Column("recipient_name")]
        [StringLength(50)]
        public string RecipientName { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        [Required]
        [Column("recipient_phone")]
        [StringLength(20)]
        public string RecipientPhone { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        [Required]
        [Column("province")]
        [StringLength(50)]
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [Required]
        [Column("city")]
        [StringLength(50)]
        public string City { get; set; }

        /// <summary>
        /// 区县
        /// </summary>
        [Required]
        [Column("district")]
        [StringLength(50)]
        public string District { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        [Required]
        [Column("detail_address")]
        [StringLength(200)]
        public string DetailAddress { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        [Column("postal_code")]
        [StringLength(20)]
        public string PostalCode { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        [Column("is_default")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// 标签：家、公司、学校等
        /// </summary>
        [Column("tag")]
        [StringLength(20)]
        public string Tag { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual User User { get; set; }
    }
}
