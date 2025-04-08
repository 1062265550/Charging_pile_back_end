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
    /// 用户角色服务实现
    /// </summary>
    public class UserRoleService : IUserRoleService
    {
        private readonly IRepository<UserRole> _roleRepository;
        private readonly IRepository<UserRoleMapping> _roleMappingRepository;
        private readonly IRepository<User> _userRepository;

        public UserRoleService(
            IRepository<UserRole> roleRepository,
            IRepository<UserRoleMapping> roleMappingRepository,
            IRepository<User> userRepository)
        {
            _roleRepository = roleRepository;
            _roleMappingRepository = roleMappingRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        public async Task<IEnumerable<UserRoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(MapToDto);
        }

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色详情</returns>
        public async Task<UserRoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return null;
            }

            return MapToDto(role);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="dto">创建角色请求</param>
        /// <returns>创建的角色ID</returns>
        public async Task<int> CreateRoleAsync(CreateUserRoleDto dto)
        {
            // 检查角色名称是否已存在
            var existingRoles = await _roleRepository.GetAsync(r => r.Name == dto.Name);
            if (existingRoles.Any())
            {
                throw new Exception($"角色名称 '{dto.Name}' 已存在");
            }

            // 创建角色
            var role = new UserRole
            {
                Name = dto.Name,
                Description = dto.Description,
                PermissionLevel = dto.PermissionLevel,
                IsSystem = false
            };

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangesAsync();

            return role.Id;
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="dto">更新角色请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateRoleAsync(int id, UpdateUserRoleDto dto)
        {
            // 检查角色是否存在
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return false;
            }

            // 检查是否为系统内置角色
            if (role.IsSystem)
            {
                throw new Exception("系统内置角色不允许修改");
            }

            // 检查角色名称是否已存在（排除当前角色）
            var existingRoles = await _roleRepository.GetAsync(r => r.Name == dto.Name && r.Id != id);
            if (existingRoles.Any())
            {
                throw new Exception($"角色名称 '{dto.Name}' 已存在");
            }

            // 更新角色
            role.Name = dto.Name;
            role.Description = dto.Description;
            role.PermissionLevel = dto.PermissionLevel;

            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteRoleAsync(int id)
        {
            // 检查角色是否存在
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return false;
            }

            // 检查是否为系统内置角色
            if (role.IsSystem)
            {
                throw new Exception("系统内置角色不允许删除");
            }

            // 检查角色是否被用户使用
            var roleMappings = await _roleMappingRepository.GetAsync(rm => rm.RoleId == id);
            if (roleMappings.Any())
            {
                throw new Exception("角色已分配给用户，无法删除");
            }

            // 删除角色
            _roleRepository.Delete(role);
            await _roleRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>角色列表</returns>
        public async Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 获取用户角色映射
            var roleMappings = await _roleMappingRepository.GetAsync(
                rm => rm.UserId == userId,
                includeProperties: rm => rm.Role);

            return roleMappings.Select(rm => MapToDto(rm.Role));
        }

        /// <summary>
        /// 分配用户角色
        /// </summary>
        /// <param name="dto">分配用户角色请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> AssignRoleToUserAsync(AssignUserRoleDto dto)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new Exception($"用户ID '{dto.UserId}' 不存在");
            }

            // 检查角色是否存在
            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null)
            {
                throw new Exception($"角色ID '{dto.RoleId}' 不存在");
            }

            // 检查用户是否已分配该角色
            var existingMapping = await _roleMappingRepository.GetAsync(
                rm => rm.UserId == dto.UserId && rm.RoleId == dto.RoleId);
            if (existingMapping.Any())
            {
                return true; // 用户已分配该角色，直接返回成功
            }

            // 创建用户角色映射
            var roleMapping = new UserRoleMapping
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId
            };

            await _roleMappingRepository.AddAsync(roleMapping);
            await _roleMappingRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 移除用户角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 检查角色是否存在
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new Exception($"角色ID '{roleId}' 不存在");
            }

            // 获取用户角色映射
            var roleMapping = (await _roleMappingRepository.GetAsync(
                rm => rm.UserId == userId && rm.RoleId == roleId)).FirstOrDefault();
            if (roleMapping == null)
            {
                return false; // 用户未分配该角色，直接返回失败
            }

            // 移除用户角色映射
            _roleMappingRepository.Delete(roleMapping);
            await _roleMappingRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否拥有角色</returns>
        public async Task<bool> HasRoleAsync(int userId, int roleId)
        {
            var roleMapping = await _roleMappingRepository.GetAsync(
                rm => rm.UserId == userId && rm.RoleId == roleId);
            return roleMapping.Any();
        }

        /// <summary>
        /// 检查用户是否拥有指定权限级别
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permissionLevel">权限级别</param>
        /// <returns>是否拥有权限</returns>
        public async Task<bool> HasPermissionAsync(int userId, int permissionLevel)
        {
            // 获取用户角色映射
            var roleMappings = await _roleMappingRepository.GetAsync(
                rm => rm.UserId == userId,
                includeProperties: rm => rm.Role);

            // 检查是否有任何角色的权限级别大于等于指定级别
            return roleMappings.Any(rm => rm.Role.PermissionLevel >= permissionLevel);
        }

        /// <summary>
        /// 将角色实体映射为DTO
        /// </summary>
        /// <param name="role">角色实体</param>
        /// <returns>角色DTO</returns>
        private UserRoleDto MapToDto(UserRole role)
        {
            return new UserRoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                PermissionLevel = role.PermissionLevel,
                IsSystem = role.IsSystem
            };
        }
    }
}
