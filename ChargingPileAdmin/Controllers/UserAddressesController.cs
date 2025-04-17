using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 用户地址管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserAddressesController : ControllerBase
    {
        private readonly IUserAddressService _userAddressService;

        public UserAddressesController(IUserAddressService userAddressService)
        {
            _userAddressService = userAddressService;
        }

        /// <summary>
        /// 获取用户的所有地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>地址列表</returns>
        [HttpGet("byUser/{userId}")]
        public async Task<IActionResult> GetUserAddresses(int userId)
        {
            try
            {
                var addresses = await _userAddressService.GetUserAddressesAsync(userId);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户地址列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取地址详情
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <returns>地址详情</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            try
            {
                var address = await _userAddressService.GetAddressByIdAsync(id);
                if (address == null)
                {
                    return NotFound($"地址ID '{id}' 不存在");
                }

                return Ok(address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取地址详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取用户的默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>默认地址</returns>
        [HttpGet("default/{userId}")]
        public async Task<IActionResult> GetUserDefaultAddress(int userId)
        {
            try
            {
                var address = await _userAddressService.GetUserDefaultAddressAsync(userId);
                if (address == null)
                {
                    return NotFound($"用户ID '{userId}' 没有设置默认地址");
                }

                return Ok(address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户默认地址失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建用户地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">创建地址请求</param>
        /// <returns>创建的地址ID</returns>
        [HttpPost("{userId}")]
        public async Task<IActionResult> CreateUserAddress(int userId, [FromBody] CreateUserAddressDto dto)
        {
            try
            {
                var addressId = await _userAddressService.CreateUserAddressAsync(userId, dto);
                return CreatedAtAction(nameof(GetAddressById), new { id = addressId }, new { id = addressId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"创建用户地址失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新用户地址
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <param name="dto">更新地址请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAddress(int id, [FromBody] UpdateUserAddressDto dto)
        {
            try
            {
                var result = await _userAddressService.UpdateUserAddressAsync(id, dto);
                if (!result)
                {
                    return NotFound($"地址ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"更新用户地址失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除用户地址
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAddress(int id)
        {
            try
            {
                var result = await _userAddressService.DeleteUserAddressAsync(id);
                if (!result)
                {
                    return NotFound($"地址ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"删除用户地址失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="addressId">地址ID</param>
        /// <returns>设置结果</returns>
        [HttpPut("{userId}/default/{addressId}")]
        public async Task<IActionResult> SetDefaultAddress(int userId, int addressId)
        {
            try
            {
                var result = await _userAddressService.SetDefaultAddressAsync(userId, addressId);
                if (!result)
                {
                    return NotFound($"地址ID '{addressId}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"设置默认地址失败: {ex.Message}");
            }
        }
    }
}
