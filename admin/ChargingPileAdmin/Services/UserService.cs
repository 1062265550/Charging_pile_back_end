using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Models;
using ChargingPileAdmin.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 用户服务实现
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns>用户列表</returns>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        /// <summary>
        /// 获取用户详情
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户详情</returns>
        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            return MapToDto(user);
        }

        /// <summary>
        /// 获取用户分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>用户分页列表</returns>
        public async Task<PagedResponseDto<UserDto>> GetUsersPagedAsync(int pageNumber, int pageSize, string keyword = null)
        {
            // 构建查询条件
            Expression<Func<User, bool>> filter = u => true;

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = u => u.Nickname.Contains(keyword) || u.OpenId.Contains(keyword);
            }

            // 获取分页数据
            var (users, totalCount, totalPages) = await _userRepository.GetPagedAsync(
                filter,
                pageNumber,
                pageSize,
                query => query.OrderByDescending(u => u.UpdateTime)
            );

            // 返回分页响应
            return new PagedResponseDto<UserDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = users.Select(MapToDto).ToList()
            };
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="dto">创建用户请求</param>
        /// <returns>创建的用户ID</returns>
        public async Task<int> CreateUserAsync(CreateUserDto dto)
        {
            // 检查OpenId是否已存在
            var existingUsers = await _userRepository.GetAsync(u => u.OpenId == dto.OpenId);
            if (existingUsers.Any())
            {
                throw new Exception($"用户OpenId '{dto.OpenId}' 已存在");
            }

            // 创建用户
            var user = new User
            {
                OpenId = dto.OpenId,
                Nickname = dto.Nickname,
                Avatar = dto.Avatar,
                Password = HashPassword(dto.Password),
                Balance = 0,
                Points = 0,
                Gender = dto.Gender,
                Country = dto.Country,
                Province = dto.Province,
                City = dto.City,
                Language = dto.Language,
                LastLoginTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user.Id;
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="dto">更新用户请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // 更新用户
            user.Nickname = dto.Nickname ?? user.Nickname;
            user.Avatar = dto.Avatar ?? user.Avatar;
            user.Gender = dto.Gender ?? user.Gender;
            user.Country = dto.Country ?? user.Country;
            user.Province = dto.Province ?? user.Province;
            user.City = dto.City ?? user.City;
            user.Language = dto.Language ?? user.Language;
            user.UpdateTime = DateTime.Now;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteUserAsync(int id)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // 删除用户
            await _userRepository.DeleteAsync(id);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="dto">登录请求</param>
        /// <returns>登录响应</returns>
        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto)
        {
            // 查找用户
            var users = await _userRepository.GetAsync(u => u.OpenId == dto.Username || u.Nickname == dto.Username);
            var user = users.FirstOrDefault();

            if (user == null || !VerifyPassword(dto.Password, user.Password))
            {
                throw new Exception("用户名或密码错误");
            }

            // 更新最后登录时间
            user.LastLoginTime = DateTime.Now;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            // 生成登录令牌（简化版，实际应使用JWT）
            var token = GenerateToken(user.Id);

            return new UserLoginResponseDto
            {
                UserId = user.Id,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Token = token
            };
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">修改密码请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // 验证旧密码
            if (!VerifyPassword(dto.OldPassword, user.Password))
            {
                throw new Exception("旧密码错误");
            }

            // 更新密码
            user.Password = HashPassword(dto.NewPassword);
            user.UpdateTime = DateTime.Now;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 充值余额
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">充值余额请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> RechargeBalanceAsync(int userId, RechargeBalanceDto dto)
        {
            // 检查用户是否存在
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // 充值余额
            user.Balance += dto.Amount;
            user.UpdateTime = DateTime.Now;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 将用户实体映射为DTO
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>用户DTO</returns>
        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                OpenId = user.OpenId,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Balance = user.Balance,
                Points = user.Points,
                Gender = user.Gender,
                Country = user.Country,
                Province = user.Province,
                City = user.City,
                Language = user.Language,
                LastLoginTime = user.LastLoginTime,
                UpdateTime = user.UpdateTime
            };
        }

        /// <summary>
        /// 哈希密码
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <returns>哈希后的密码</returns>
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return null;
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <param name="hashedPassword">哈希后的密码</param>
        /// <returns>是否匹配</returns>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        /// <summary>
        /// 生成登录令牌（简化版，实际应使用JWT）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>登录令牌</returns>
        private string GenerateToken(int userId)
        {
            // 简化版，实际应使用JWT
            var tokenData = $"{userId}:{DateTime.Now.AddDays(1):yyyy-MM-dd HH:mm:ss}";
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenData));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
