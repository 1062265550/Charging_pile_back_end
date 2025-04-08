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
    /// 用户地址服务实现
    /// </summary>
    public class UserAddressService : IUserAddressService
    {
        private readonly IRepository<UserAddress> _addressRepository;
        private readonly IRepository<User> _userRepository;

        public UserAddressService(
            IRepository<UserAddress> addressRepository,
            IRepository<User> userRepository)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 获取用户的所有地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>地址列表</returns>
        public async Task<IEnumerable<UserAddressDto>> GetUserAddressesAsync(int userId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 获取用户地址
            var addresses = await _addressRepository.GetAsync(a => a.UserId == userId);
            return addresses.Select(MapToDto);
        }

        /// <summary>
        /// 获取地址详情
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <returns>地址详情</returns>
        public async Task<UserAddressDto> GetAddressByIdAsync(int id)
        {
            var address = await _addressRepository.GetByIdAsync(id);
            if (address == null)
            {
                return null;
            }

            return MapToDto(address);
        }

        /// <summary>
        /// 获取用户的默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>默认地址</returns>
        public async Task<UserAddressDto> GetUserDefaultAddressAsync(int userId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 获取用户默认地址
            var address = (await _addressRepository.GetAsync(a => a.UserId == userId && a.IsDefault)).FirstOrDefault();
            if (address == null)
            {
                return null;
            }

            return MapToDto(address);
        }

        /// <summary>
        /// 创建用户地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">创建地址请求</param>
        /// <returns>创建的地址ID</returns>
        public async Task<int> CreateUserAddressAsync(int userId, CreateUserAddressDto dto)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 如果是默认地址，需要将其他地址设置为非默认
            if (dto.IsDefault)
            {
                await ClearDefaultAddressAsync(userId);
            }

            // 创建地址
            var address = new UserAddress
            {
                UserId = userId,
                RecipientName = dto.RecipientName,
                RecipientPhone = dto.RecipientPhone,
                Province = dto.Province,
                City = dto.City,
                District = dto.District,
                DetailAddress = dto.DetailAddress,
                PostalCode = dto.PostalCode,
                IsDefault = dto.IsDefault,
                Tag = dto.Tag,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            await _addressRepository.AddAsync(address);
            await _addressRepository.SaveChangesAsync();

            return address.Id;
        }

        /// <summary>
        /// 更新用户地址
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <param name="dto">更新地址请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateUserAddressAsync(int id, UpdateUserAddressDto dto)
        {
            // 检查地址是否存在
            var address = await _addressRepository.GetByIdAsync(id);
            if (address == null)
            {
                return false;
            }

            // 如果是默认地址，需要将其他地址设置为非默认
            if (dto.IsDefault && !address.IsDefault)
            {
                await ClearDefaultAddressAsync(address.UserId);
            }

            // 更新地址
            address.RecipientName = dto.RecipientName;
            address.RecipientPhone = dto.RecipientPhone;
            address.Province = dto.Province;
            address.City = dto.City;
            address.District = dto.District;
            address.DetailAddress = dto.DetailAddress;
            address.PostalCode = dto.PostalCode;
            address.IsDefault = dto.IsDefault;
            address.Tag = dto.Tag;
            address.UpdateTime = DateTime.Now;

            _addressRepository.Update(address);
            await _addressRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除用户地址
        /// </summary>
        /// <param name="id">地址ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteUserAddressAsync(int id)
        {
            // 检查地址是否存在
            var address = await _addressRepository.GetByIdAsync(id);
            if (address == null)
            {
                return false;
            }

            // 删除地址
            _addressRepository.Delete(address);
            await _addressRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="addressId">地址ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"用户ID '{userId}' 不存在");
            }

            // 检查地址是否存在
            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null)
            {
                return false;
            }

            // 检查地址是否属于该用户
            if (address.UserId != userId)
            {
                throw new Exception($"地址ID '{addressId}' 不属于用户ID '{userId}'");
            }

            // 如果已经是默认地址，直接返回成功
            if (address.IsDefault)
            {
                return true;
            }

            // 清除其他默认地址
            await ClearDefaultAddressAsync(userId);

            // 设置为默认地址
            address.IsDefault = true;
            address.UpdateTime = DateTime.Now;

            _addressRepository.Update(address);
            await _addressRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 清除用户的默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        private async Task ClearDefaultAddressAsync(int userId)
        {
            var defaultAddresses = await _addressRepository.GetAsync(a => a.UserId == userId && a.IsDefault);
            foreach (var defaultAddress in defaultAddresses)
            {
                defaultAddress.IsDefault = false;
                defaultAddress.UpdateTime = DateTime.Now;
                _addressRepository.Update(defaultAddress);
            }

            await _addressRepository.SaveChangesAsync();
        }

        /// <summary>
        /// 将地址实体映射为DTO
        /// </summary>
        /// <param name="address">地址实体</param>
        /// <returns>地址DTO</returns>
        private UserAddressDto MapToDto(UserAddress address)
        {
            return new UserAddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                RecipientName = address.RecipientName,
                RecipientPhone = address.RecipientPhone,
                Province = address.Province,
                City = address.City,
                District = address.District,
                DetailAddress = address.DetailAddress,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault,
                Tag = address.Tag,
                CreateTime = address.CreateTime,
                UpdateTime = address.UpdateTime
            };
        }
    }
}
