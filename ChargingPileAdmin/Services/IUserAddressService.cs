using ChargingPileAdmin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 用户地址服务接口
    /// </summary>
    public interface IUserAddressService
    {
        /// <summary>
        /// 获取用户的所有地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>地址列表</returns>
        Task<IEnumerable<UserAddressDto>> GetUserAddressesAsync(int userId);

        /// <summary>
        /// 获取地址详情
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <returns>地址详情</returns>
        Task<UserAddressDto> GetAddressByIdAsync(int id);

        /// <summary>
        /// 获取用户的默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>默认地址</returns>
        Task<UserAddressDto> GetUserDefaultAddressAsync(int userId);

        /// <summary>
        /// 创建用户地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">创建地址请求</param>
        /// <returns>创建的地址ID</returns>
        Task<int> CreateUserAddressAsync(int userId, CreateUserAddressDto dto);

        /// <summary>
        /// 更新用户地址
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <param name="dto">更新地址请求</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateUserAddressAsync(int id, UpdateUserAddressDto dto);

        /// <summary>
        /// 删除用户地址
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteUserAddressAsync(int id);

        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="addressId">地址ID</param>
        /// <returns>是否成功</returns>
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
    }
}
