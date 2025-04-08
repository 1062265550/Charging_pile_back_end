using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 用户管理 - 提供用户账户相关的增删改查操作
    /// </summary>
    /// <remarks>
    /// 本控制器负责用户的所有管理功能，包括：
    /// - 用户信息的查询（按ID、分页查询等）
    /// - 用户的创建、更新和删除
    /// - 用户认证（登录）
    /// - 密码管理
    /// - 账户余额管理
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// 获取所有用户信息
        /// </summary>
        /// <remarks>
        /// 返回系统中所有用户的完整列表，包含用户基本信息、联系方式、账户状态等数据
        /// 
        /// 使用场景：
        /// - 管理后台展示所有用户
        /// - 系统用户数据分析
        /// 
        /// 注意：当用户数量较多时，建议使用分页接口获取数据
        /// </remarks>
        /// <returns>用户列表</returns>
        /// <response code="200">成功返回用户列表</response>
        /// <response code="500">服务器错误</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定ID的用户详细信息
        /// </summary>
        /// <remarks>
        /// 根据用户ID获取单个用户的详细信息，包括：
        /// - 基本信息（用户名、真实姓名、电话等）
        /// - 账户信息（注册时间、余额、状态等）
        /// - 角色信息（用户角色、权限等）
        /// 
        /// 使用场景：
        /// - 查看用户详情页面
        /// - 用户资料管理
        /// - 客服查询用户信息
        /// </remarks>
        /// <param name="id">用户ID（唯一标识符）</param>
        /// <returns>用户详情</returns>
        /// <response code="200">成功返回用户详情</response>
        /// <response code="404">未找到指定ID的用户</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"用户ID '{id}' 不存在");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 分页获取用户列表（支持关键字搜索）
        /// </summary>
        /// <remarks>
        /// 提供带分页功能的用户查询接口，支持按关键字搜索用户信息
        /// 
        /// 搜索范围包括：
        /// - 用户名
        /// - 手机号码
        /// - 真实姓名
        /// - 邮箱地址
        /// 
        /// 使用场景：
        /// - 管理后台的用户列表页
        /// - 带搜索功能的用户查询
        /// </remarks>
        /// <param name="pageNumber">页码，从1开始</param>
        /// <param name="pageSize">每页记录数，默认10条</param>
        /// <param name="keyword">关键字，可选，用于搜索用户名、手机号、真实姓名或邮箱</param>
        /// <returns>用户分页数据</returns>
        /// <response code="200">成功返回分页数据</response>
        /// <response code="400">分页参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponseDto<UserDto>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetUsersPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string keyword = null)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("页码必须大于等于1");
                }

                if (pageSize < 1)
                {
                    return BadRequest("每页记录数必须大于等于1");
                }
                
                var users = await _userService.GetUsersPagedAsync(pageNumber, pageSize, keyword);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户分页列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <remarks>
        /// 创建一个新的用户账户，需要提供用户的基本信息、联系方式、密码等
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "username": "zhangsan",
        ///   "password": "Pwd123456",
        ///   "realName": "张三",
        ///   "phone": "13812345678",
        ///   "email": "zhangsan@example.com",
        ///   "roleId": 2
        /// }
        /// ```
        /// 
        /// 使用场景：
        /// - 管理员添加新用户
        /// - 用户自行注册
        /// </remarks>
        /// <param name="dto">创建用户的数据传输对象</param>
        /// <returns>创建的用户ID</returns>
        /// <response code="201">用户创建成功，返回新创建的用户ID</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="500">服务器错误</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                var userId = await _userService.CreateUserAsync(dto);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { id = userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"创建用户失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <remarks>
        /// 根据用户ID更新用户的信息，可以更新的内容包括：
        /// - 用户基本信息（真实姓名、联系方式等）
        /// - 账户状态（启用/禁用）
        /// - 用户角色
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "realName": "张三(新)",
        ///   "phone": "13987654321",
        ///   "email": "zhangsan_new@example.com",
        ///   "status": 1,
        ///   "roleId": 3
        /// }
        /// ```
        /// 
        /// 使用场景：
        /// - 管理员修改用户信息
        /// - 用户更新个人资料
        /// </remarks>
        /// <param name="id">用户ID</param>
        /// <param name="dto">更新用户的数据传输对象</param>
        /// <returns>更新结果</returns>
        /// <response code="204">用户更新成功</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="404">未找到指定ID的用户</response>
        /// <response code="500">服务器错误</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("用户ID必须大于0");
                }
                
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                var result = await _userService.UpdateUserAsync(id, dto);
                if (!result)
                {
                    return NotFound($"用户ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"更新用户失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <remarks>
        /// 根据用户ID删除指定的用户记录。删除前会进行以下检查：
        /// - 用户是否存在
        /// - 用户是否有未完成的订单
        /// - 用户是否有未处理的充值/退款记录
        /// 
        /// 使用场景：
        /// - 管理员删除违规用户
        /// - 用户申请注销账号
        /// 
        /// 注意：删除用户可能会保留部分历史数据用于统计和分析
        /// </remarks>
        /// <param name="id">用户ID</param>
        /// <returns>删除结果</returns>
        /// <response code="204">用户删除成功</response>
        /// <response code="400">ID参数错误</response>
        /// <response code="404">未找到指定ID的用户</response>
        /// <response code="500">服务器错误</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("用户ID必须大于0");
                }
                
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound($"用户ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"删除用户失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <remarks>
        /// 验证用户身份并返回登录凭证和用户基本信息
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "username": "zhangsan",
        ///   "password": "Pwd123456"
        /// }
        /// ```
        /// 
        /// 返回内容包括：
        /// - 用户基本信息
        /// - 登录凭证（Token）
        /// - 权限信息
        /// 
        /// 使用场景：
        /// - 用户登录系统
        /// - APP用户登录
        /// </remarks>
        /// <param name="dto">登录的数据传输对象，包含用户名和密码</param>
        /// <returns>登录响应，包含用户信息和Token</returns>
        /// <response code="200">登录成功，返回用户信息和Token</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="401">用户名或密码错误</response>
        /// <response code="500">服务器错误</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(UserLoginResponseDto), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                {
                    return BadRequest("用户名和密码不能为空");
                }
                
                var response = await _userService.LoginAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"登录失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <remarks>
        /// 修改指定用户的登录密码，需要提供原密码和新密码
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "oldPassword": "OldPwd123",
        ///   "newPassword": "NewPwd456",
        ///   "confirmPassword": "NewPwd456"
        /// }
        /// ```
        /// 
        /// 密码要求：
        /// - 长度不少于6位
        /// - 包含字母和数字
        /// - 新密码不能与旧密码相同
        /// 
        /// 使用场景：
        /// - 用户主动修改密码
        /// - 管理员重置用户密码
        /// </remarks>
        /// <param name="id">用户ID</param>
        /// <param name="dto">修改密码的数据传输对象</param>
        /// <returns>修改结果</returns>
        /// <response code="204">密码修改成功</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="404">未找到指定ID的用户</response>
        /// <response code="500">服务器错误</response>
        [HttpPut("{id}/password")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("用户ID必须大于0");
                }
                
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                if (string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return BadRequest("新密码和确认密码不能为空");
                }
                
                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return BadRequest("新密码和确认密码不一致");
                }
                
                var result = await _userService.ChangePasswordAsync(id, dto);
                if (!result)
                {
                    return NotFound($"用户ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"修改密码失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 用户账户充值
        /// </summary>
        /// <remarks>
        /// 为指定用户的账户充值余额，用于支付充电订单
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "amount": 100.00,
        ///   "paymentMethod": 1,
        ///   "transactionId": "PAY123456789",
        ///   "remark": "微信支付充值"
        /// }
        /// ```
        /// 
        /// 支付方式说明：
        /// - 0：其他方式
        /// - 1：微信支付
        /// - 2：支付宝支付
        /// - 3：银行卡支付
        /// 
        /// 使用场景：
        /// - 用户在APP中充值余额
        /// - 管理员代客户充值
        /// - 线下支付后管理员手动增加余额
        /// </remarks>
        /// <param name="id">用户ID</param>
        /// <param name="dto">充值余额的数据传输对象</param>
        /// <returns>充值结果</returns>
        /// <response code="204">充值成功</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="404">未找到指定ID的用户</response>
        /// <response code="500">服务器错误</response>
        [HttpPost("{id}/recharge")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> RechargeBalance(int id, [FromBody] RechargeBalanceDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("用户ID必须大于0");
                }
                
                if (dto == null)
                {
                    return BadRequest("请求数据不能为空");
                }
                
                if (dto.Amount <= 0)
                {
                    return BadRequest("充值金额必须大于0");
                }
                
                var result = await _userService.RechargeBalanceAsync(id, dto);
                if (!result)
                {
                    return NotFound($"用户ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"充值余额失败: {ex.Message}");
            }
        }
    }
}
