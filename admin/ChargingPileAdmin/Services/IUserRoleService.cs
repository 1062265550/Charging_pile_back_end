using ChargingPileAdmin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 用户角色服务接口
    /// </summary>
    public interface IUserRoleService
    {
        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        Task<IEnumerable<UserRoleDto>> GetAllRolesAsync();

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色详情</returns>
        Task<UserRoleDto> GetRoleByIdAsync(int id);

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="dto">创建角色请求</param>
        /// <returns>创建的角色ID</returns>
        Task<int> CreateRoleAsync(CreateUserRoleDto dto);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="dto">更新角色请求</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateRoleAsync(int id, UpdateUserRoleDto dto);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteRoleAsync(int id);

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>角色列表</returns>
        Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId);

        /// <summary>
        /// 分配用户角色
        /// </summary>
        /// <param name="dto">分配用户角色请求</param>
        /// <returns>是否成功</returns>
        Task<bool> AssignRoleToUserAsync(AssignUserRoleDto dto);

        /// <summary>
        /// 移除用户角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否成功</returns>
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否拥有角色</returns>
        Task<bool> HasRoleAsync(int userId, int roleId);

        /// <summary>
        /// 检查用户是否拥有指定权限级别
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permissionLevel">权限级别</param>
        /// <returns>是否拥有权限</returns>
        Task<bool> HasPermissionAsync(int userId, int permissionLevel);
    }
}
