using ChargingPileAdmin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 用户服务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns>用户列表</returns>
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// 获取用户详情
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户详情</returns>
        Task<UserDto> GetUserByIdAsync(int id);

        /// <summary>
        /// 获取用户分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>用户分页列表</returns>
        Task<PagedResponseDto<UserDto>> GetUsersPagedAsync(int pageNumber, int pageSize, string keyword = null);

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="dto">创建用户请求</param>
        /// <returns>创建的用户ID</returns>
        Task<int> CreateUserAsync(CreateUserDto dto);

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="dto">更新用户请求</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteUserAsync(int id);

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="dto">登录请求</param>
        /// <returns>登录响应</returns>
        Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto);

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">修改密码请求</param>
        /// <returns>是否成功</returns>
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);

        /// <summary>
        /// 充值余额
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">充值余额请求</param>
        /// <returns>是否成功</returns>
        Task<bool> RechargeBalanceAsync(int userId, RechargeBalanceDto dto);
    }
}
