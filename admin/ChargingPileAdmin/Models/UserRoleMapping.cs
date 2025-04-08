using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 用户角色映射关系
    /// </summary>
    public class UserRoleMapping
    {
        /// <summary>
        /// 映射唯一标识符
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
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public int RoleId { get; set; }

        // 导航属性
        public virtual User User { get; set; }
        public virtual UserRole Role { get; set; }
    }
}
