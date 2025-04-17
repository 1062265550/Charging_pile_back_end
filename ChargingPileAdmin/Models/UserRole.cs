using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingPileAdmin.Models
{
    /// <summary>
    /// 用户角色信息
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// 角色唯一标识符
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        [Column("description")]
        [StringLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// 权限级别：1-普通用户，2-VIP用户，3-管理员，4-超级管理员
        /// </summary>
        [Column("permission_level")]
        public int PermissionLevel { get; set; }

        /// <summary>
        /// 是否系统内置角色
        /// </summary>
        [Column("is_system")]
        public bool IsSystem { get; set; }
    }
}
