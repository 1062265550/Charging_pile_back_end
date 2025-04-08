namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 用户角色数据传输对象
    /// </summary>
    public class UserRoleDto
    {
        /// <summary>
        /// 角色唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 权限级别：1-普通用户，2-VIP用户，3-管理员，4-超级管理员
        /// </summary>
        public int PermissionLevel { get; set; }

        /// <summary>
        /// 权限级别描述
        /// </summary>
        public string PermissionLevelDescription => PermissionLevel switch
        {
            1 => "普通用户",
            2 => "VIP用户",
            3 => "管理员",
            4 => "超级管理员",
            _ => "未知"
        };

        /// <summary>
        /// 是否系统内置角色
        /// </summary>
        public bool IsSystem { get; set; }
    }

    /// <summary>
    /// 创建用户角色请求
    /// </summary>
    public class CreateUserRoleDto
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 权限级别：1-普通用户，2-VIP用户，3-管理员，4-超级管理员
        /// </summary>
        public int PermissionLevel { get; set; }
    }

    /// <summary>
    /// 更新用户角色请求
    /// </summary>
    public class UpdateUserRoleDto
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 权限级别：1-普通用户，2-VIP用户，3-管理员，4-超级管理员
        /// </summary>
        public int PermissionLevel { get; set; }
    }

    /// <summary>
    /// 用户角色分配请求
    /// </summary>
    public class AssignUserRoleDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleId { get; set; }
    }
}
