using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 用户角色管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;

        public UserRolesController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _userRoleService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取角色列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取角色详情
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色详情</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _userRoleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound($"角色ID '{id}' 不存在");
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取角色详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="dto">创建角色请求</param>
        /// <returns>创建的角色ID</returns>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateUserRoleDto dto)
        {
            try
            {
                var roleId = await _userRoleService.CreateRoleAsync(dto);
                return CreatedAtAction(nameof(GetRoleById), new { id = roleId }, new { id = roleId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"创建角色失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="dto">更新角色请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            try
            {
                var result = await _userRoleService.UpdateRoleAsync(id, dto);
                if (!result)
                {
                    return NotFound($"角色ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"更新角色失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var result = await _userRoleService.DeleteRoleAsync(id);
                if (!result)
                {
                    return NotFound($"角色ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"删除角色失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>角色列表</returns>
        [HttpGet("byUser/{userId}")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            try
            {
                var roles = await _userRoleService.GetUserRolesAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户角色列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 分配用户角色
        /// </summary>
        /// <param name="dto">分配用户角色请求</param>
        /// <returns>分配结果</returns>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignUserRoleDto dto)
        {
            try
            {
                var result = await _userRoleService.AssignRoleToUserAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"分配用户角色失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 移除用户角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>移除结果</returns>
        [HttpDelete("remove/{userId}/{roleId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            try
            {
                var result = await _userRoleService.RemoveRoleFromUserAsync(userId, roleId);
                if (!result)
                {
                    return NotFound($"用户ID '{userId}' 未分配角色ID '{roleId}'");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"移除用户角色失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查用户是否拥有指定角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否拥有角色</returns>
        [HttpGet("check/{userId}/{roleId}")]
        public async Task<IActionResult> CheckUserHasRole(int userId, int roleId)
        {
            try
            {
                var result = await _userRoleService.HasRoleAsync(userId, roleId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"检查用户角色失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查用户是否拥有指定权限级别
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permissionLevel">权限级别</param>
        /// <returns>是否拥有权限</returns>
        [HttpGet("checkPermission/{userId}/{permissionLevel}")]
        public async Task<IActionResult> CheckUserHasPermission(int userId, int permissionLevel)
        {
            try
            {
                var result = await _userRoleService.HasPermissionAsync(userId, permissionLevel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"检查用户权限失败: {ex.Message}");
            }
        }
    }
}
