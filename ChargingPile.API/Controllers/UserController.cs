using ChargingPile.API.Models.User;
using ChargingPile.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 用户控制器
    /// </summary>
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userService">用户服务</param>
        /// <param name="logger">日志记录器</param>
        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 微信小程序用户登录
        /// </summary>
        /// <param name="request">微信登录请求</param>
        /// <returns>登录响应</returns>
        [HttpPost("wechat/login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> WechatLogin([FromBody] WechatLoginRequest request)
        {
            try
            {
                // 记录请求信息（对敏感信息进行脱敏）
                _logger.LogInformation($"收到微信登录请求: Code={MaskSensitiveInfo(request?.Code)}, Nickname={request?.UserInfo?.NickName}");

                // 验证请求并设置默认值
                ValidateAndPrepareRequest(ref request);

                // 调用服务
                _logger.LogInformation($"准备调用微信登录服务: Code={MaskSensitiveInfo(request.Code)}");
                var response = await _userService.WechatLoginAsync(request);
                _logger.LogInformation($"微信登录成功: UserId={response.UserId}, OpenId={MaskSensitiveInfo(response.OpenId)}");

                return Ok(ApiResponse<LoginResponse>.Success(response, "登录成功"));
            }
            catch (Exception ex)
            {
                return HandleLoginException(ex);
            }
        }

        /// <summary>
        /// 验证并准备登录请求
        /// </summary>
        /// <param name="request">登录请求</param>
        private void ValidateAndPrepareRequest(ref WechatLoginRequest request)
        {
            // 检查请求是否为空
            if (request == null)
            {
                _logger.LogWarning("请求对象为空");
                throw new ArgumentNullException(nameof(request), "请求不能为空");
            }

            // 检查Code是否为空
            if (string.IsNullOrEmpty(request.Code))
            {
                _logger.LogWarning("微信登录code为空");
                throw new ArgumentException("微信登录code不能为空", nameof(request.Code));
            }

            // 确保UserInfo不为null
            if (request.UserInfo == null)
            {
                _logger.LogInformation("UserInfo为空，创建默认对象");
                request.UserInfo = new WechatUserInfo();
            }
        }

        /// <summary>
        /// 处理登录异常
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns>错误响应</returns>
        private ActionResult<ApiResponse<LoginResponse>> HandleLoginException(Exception ex)
        {
            _logger.LogError($"微信登录异常: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.LogError($"内部异常: {ex.InnerException.Message}");
            }

            // 根据异常类型返回不同的状态码
            if (ex is ArgumentNullException || ex is ArgumentException)
            {
                return BadRequest(ApiResponse<LoginResponse>.Fail(ex.Message));
            }

            // 返回更详细的错误信息
            string errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += $" | 内部错误: {ex.InnerException.Message}";
            }

            return BadRequest(ApiResponse<LoginResponse>.Fail($"登录失败: {errorMessage}"));
        }

        /// <summary>
        /// 用户名密码登录（测试用）
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录响应</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<LoginResponse>.Fail("请求参数验证失败"));
                }

                var response = await _userService.LoginAsync(request);
                return Ok(ApiResponse<LoginResponse>.Success(response, "登录成功"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoginResponse>.Fail($"登录失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 用户注册（测试用）
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>注册响应</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] UserRegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<LoginResponse>.Fail("请求参数验证失败"));
                }

                var response = await _userService.RegisterAsync(request);
                return Ok(ApiResponse<LoginResponse>.Success(response, "注册成功"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoginResponse>.Fail($"注册失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取当前登录用户的详细信息
        /// </summary>
        /// <returns>用户信息</returns>
        [HttpGet("info")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserInfoResponse>>> GetUserInfo()
        {
            try
            {
                var userInfo = await _userService.GetUserInfoAsync();
                return Ok(ApiResponse<UserInfoResponse>.Success(userInfo));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<UserInfoResponse>.Fail(ex.Message, 401));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserInfoResponse>.Fail($"获取用户信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 更新用户基本信息
        /// </summary>
        /// <param name="rawRequest">原始更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("update")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUserInfo([FromBody] object rawRequest)
        {
            try
            {
                // 记录原始请求内容
                _logger.LogInformation($"收到用户信息更新请求");

                // 手动将原始请求转换为 UpdateUserInfoRequest 对象
                UpdateUserInfoRequest request = new UpdateUserInfoRequest();

                // 如果原始请求不为空，尝试提取属性
                if (rawRequest != null)
                {
                    try
                    {
                        // 将原始请求转换为JSON字符串
                        string jsonString = System.Text.Json.JsonSerializer.Serialize(rawRequest);
                        // 反序列化为字典
                        var dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                        // 提取各个属性
                        if (dictionary.TryGetValue("nickname", out var nickname) && nickname != null)
                        {
                            request.Nickname = nickname.ToString();
                        }

                        if (dictionary.TryGetValue("avatar", out var avatar) && avatar != null)
                        {
                            request.Avatar = avatar.ToString();
                        }

                        if (dictionary.TryGetValue("gender", out var gender) && gender != null)
                        {
                            if (int.TryParse(gender.ToString(), out int genderValue))
                            {
                                request.Gender = genderValue;
                            }
                        }

                        if (dictionary.TryGetValue("country", out var country) && country != null)
                        {
                            request.Country = country.ToString();
                        }

                        if (dictionary.TryGetValue("province", out var province) && province != null)
                        {
                            request.Province = province.ToString();
                        }

                        if (dictionary.TryGetValue("city", out var city) && city != null)
                        {
                            request.City = city.ToString();
                        }

                        if (dictionary.TryGetValue("language", out var language) && language != null)
                        {
                            request.Language = language.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"解析请求异常: {ex.Message}");
                    }
                }

                // 记录转换后的请求内容
                _logger.LogInformation($"处理后的请求: Nickname={request.Nickname}, Gender={request.Gender}, Country={request.Country}, Province={request.Province}, City={request.City}, Language={request.Language}");

                var result = await _userService.UpdateUserInfoAsync(request);
                if (result)
                {
                    return Ok(ApiResponse<object>.Success(null, "更新成功"));
                }
                else
                {
                    return BadRequest(ApiResponse<object>.Fail("更新失败"));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.Fail(ex.Message, 401));
            }
            catch (Exception ex)
            {
                _logger.LogError($"更新用户信息异常: {ex.Message}");
                _logger.LogDebug($"异常堆栈: {ex.StackTrace}");
                return BadRequest(ApiResponse<object>.Fail($"更新用户信息失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 对敏感信息进行脱敏处理
        /// </summary>
        /// <param name="info">敏感信息</param>
        /// <returns>脱敏后的信息</returns>
        private string MaskSensitiveInfo(string info)
        {
            if (string.IsNullOrEmpty(info))
            {
                return "";
            }

            // 如果字符串长度小于等于6，只显示前1位和后1位
            if (info.Length <= 6)
            {
                return info.Length > 2
                    ? $"{info.Substring(0, 1)}***{info.Substring(info.Length - 1)}"
                    : "***";
            }

            // 如果字符串长度大于6，显示前3位和后3位
            return $"{info.Substring(0, 3)}***{info.Substring(info.Length - 3)}";
        }
    }
}
