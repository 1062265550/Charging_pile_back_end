using ChargingPile.API.Models.User;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChargingPile.API.Services
{
    /// <summary>
    /// 用户服务
    /// </summary>
    public class UserService
    {
        private readonly IDbConnection _dbConnection;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbConnection">数据库连接</param>
        /// <param name="configuration">配置</param>
        /// <param name="jwtService">JWT服务</param>
        /// <param name="httpClientFactory">HTTP客户端工厂</param>
        /// <param name="httpContextAccessor">HTTP上下文访问器</param>
        /// <param name="logger">日志记录器</param>
        public UserService(
            IDbConnection dbConnection,
            IConfiguration configuration,
            JwtService jwtService,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserService> logger)
        {
            _dbConnection = dbConnection;
            _configuration = configuration;
            _jwtService = jwtService;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前登录用户的ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("用户未登录或会话已过期");
            }
            return userId;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns>用户信息响应</returns>
        public async Task<UserInfoResponse> GetUserInfoAsync()
        {
            int userId = GetCurrentUserId();
            _logger.LogInformation("获取用户信息，用户ID: {UserId}", userId);

            // 使用显式列名而不是通配符，确保字段映射正确
            var sql = @"
                SELECT
                    id,
                    open_id AS OpenId,
                    nickname AS Nickname,
                    avatar AS Avatar,
                    password AS Password,
                    balance AS Balance,
                    points AS Points,
                    gender AS Gender,
                    country AS Country,
                    province AS Province,
                    city AS City,
                    language AS Language,
                    last_login_time AS LastLoginTime,
                    update_time AS UpdateTime
                FROM users
                WHERE id = @UserId";

            var user = await _dbConnection.QueryFirstOrDefaultAsync<UserEntity>(sql, new { UserId = userId });

            if (user == null)
            {
                _logger.LogWarning("用户不存在，用户ID: {UserId}", userId);
                throw new InvalidOperationException("用户不存在");
            }

            _logger.LogInformation("用户信息获取成功，用户ID: {UserId}, OpenId: {OpenId}, Nickname: {Nickname}, Points: {Points}, LastLoginTime: {LastLoginTime}",
                user.Id, user.OpenId, user.Nickname, user.Points, user.LastLoginTime);

            return new UserInfoResponse
            {
                UserId = user.Id,
                OpenId = user.OpenId,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Gender = user.Gender,
                Country = user.Country,
                Province = user.Province,
                City = user.City,
                Language = user.Language,
                Balance = user.Balance,
                Points = user.Points,
                LastLoginTime = user.LastLoginTime
            };
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="request">更新用户信息请求</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateUserInfoAsync(UpdateUserInfoRequest request)
        {
            int userId = GetCurrentUserId();

            // 验证用户是否存在
            var user = await _dbConnection.QueryFirstOrDefaultAsync<UserEntity>(
                "SELECT * FROM users WHERE id = @UserId",
                new { UserId = userId });

            if (user == null)
            {
                throw new InvalidOperationException("用户不存在");
            }

            // 构建动态SQL语句和参数，只更新提供的字段
            var updateFields = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@UpdateTime", DateTime.Now);

            if (request.Nickname != null)
            {
                updateFields.Add("nickname = @Nickname");
                parameters.Add("@Nickname", request.Nickname);
            }

            if (request.Avatar != null)
            {
                updateFields.Add("avatar = @Avatar");
                parameters.Add("@Avatar", request.Avatar);
            }

            if (request.Gender.HasValue)
            {
                updateFields.Add("gender = @Gender");
                parameters.Add("@Gender", request.Gender.Value);
            }

            if (request.Country != null)
            {
                updateFields.Add("country = @Country");
                parameters.Add("@Country", request.Country);
            }

            if (request.Province != null)
            {
                updateFields.Add("province = @Province");
                parameters.Add("@Province", request.Province);
            }

            if (request.City != null)
            {
                updateFields.Add("city = @City");
                parameters.Add("@City", request.City);
            }

            if (request.Language != null)
            {
                updateFields.Add("language = @Language");
                parameters.Add("@Language", request.Language);
            }

            // 始终更新更新时间
            updateFields.Add("update_time = @UpdateTime");

            // 如果没有要更新的字段，仅更新更新时间
            if (updateFields.Count == 1)
            {
                _logger.LogInformation("没有提供要更新的字段，仅更新更新时间");
            }

            // 构建完整的SQL语句
            var sql = $"UPDATE users SET {string.Join(", ", updateFields)} WHERE id = @UserId";
            _logger.LogInformation($"执行用户更新SQL: {sql}");

            // 执行更新
            var result = await _dbConnection.ExecuteAsync(sql, parameters);

            return result > 0;
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

        /// <summary>
        /// 调用微信接口获取OpenId
        /// </summary>
        /// <param name="code">微信登录code</param>
        /// <returns>微信登录响应</returns>
        private async Task<WechatLoginResponse> GetWechatOpenIdAsync(string code)
        {
            // 获取微信小程序配置
            var appId = _configuration["WechatMiniProgram:AppId"];
            var appSecret = _configuration["WechatMiniProgram:AppSecret"];

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                throw new InvalidOperationException("微信小程序配置不完整");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appId}&secret={appSecret}&js_code={code}&grant_type=authorization_code";

                // 打印请求URL便于调试（不包含敏感信息）
                _logger.LogInformation($"调用微信接口获取OpenId: appid={appId}, js_code={MaskSensitiveInfo(code)}");

                // 使用HttpClient直接发送请求并获取原始响应
                var httpResponse = await client.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode(); // 确保请求成功

                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                // 不记录原始响应，可能包含敏感信息
                _logger.LogDebug("收到微信接口响应");

                // 使用忽略大小写选项进行反序列化
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = System.Text.Json.JsonSerializer.Deserialize<WechatLoginResponse>(responseContent, options) ?? new WechatLoginResponse();

                // 打印微信响应详情（对敏感信息进行脱敏）
                _logger.LogInformation($"微信响应详情: Openid={MaskSensitiveInfo(response.Openid)}, Errcode={response.Errcode}, Errmsg={response.Errmsg}");

                // 检查微信错误码
                if (response.Errcode != 0)
                {
                    var errorMessage = $"微信接口错误: 错误码={response.Errcode}, 错误信息={response.Errmsg}";
                    _logger.LogWarning(errorMessage);

                    switch (response.Errcode)
                    {
                        case 40163:
                            throw new InvalidOperationException($"登录凭证Code已被使用: {response.Errmsg}，请重新获取Code后再次尝试");
                        case 40029:
                            throw new InvalidOperationException($"登录凭证Code无效: {response.Errmsg}，请检查Code是否正确");
                        case 45011:
                            throw new InvalidOperationException($"API调用频率受限: {response.Errmsg}，请稍后再试");
                        case 40226:
                            throw new InvalidOperationException($"高风险用户，小程序登录拦截: {response.Errmsg}");
                        case -1:
                            throw new InvalidOperationException($"微信系统繁忙: {response.Errmsg}，请稍后再试");
                        default:
                            throw new InvalidOperationException($"微信登录失败: 错误码={response.Errcode}, 信息={response.Errmsg}");
                    }
                }

                // 确保Openid不为空
                if (string.IsNullOrEmpty(response.Openid))
                {
                    throw new InvalidOperationException("微信返回Openid为空，登录失败");
                }

                return response;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                _logger.LogError($"调用微信接口异常: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"内部异常: {ex.InnerException.Message}");
                    _logger.LogDebug($"异常堆栈: {ex.StackTrace}");
                }
                throw new InvalidOperationException($"调用微信接口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        /// <param name="openId">微信OpenId</param>
        /// <param name="userInfo">用户信息</param>
        /// <returns>创建的用户实体</returns>
        private async Task<UserEntity> CreateUserAsync(string openId, WechatUserInfo userInfo)
        {
            if (string.IsNullOrEmpty(openId))
            {
                throw new InvalidOperationException("微信OpenId为空，无法创建用户");
            }

            // 生成默认昵称
            string defaultNickname = GenerateDefaultNickname(openId);
            _logger.LogInformation($"生成默认昵称: {defaultNickname}");

            // 创建新用户
            var user = new UserEntity
            {
                OpenId = openId,
                // 使用默认值代替空值
                Nickname = userInfo?.NickName ?? defaultNickname,
                Avatar = userInfo?.AvatarUrl ?? "default_avatar.png",
                Gender = userInfo?.Gender ?? 0,
                Country = userInfo?.Country ?? "",
                Province = userInfo?.Province ?? "",
                City = userInfo?.City ?? "",
                Language = userInfo?.Language ?? "zh_CN",
                Balance = 0,
                Points = 0,
                LastLoginTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            _logger.LogInformation($"准备创建新用户: OpenId={MaskSensitiveInfo(user.OpenId)}, Nickname={user.Nickname}");

            try
            {
                var userId = await _dbConnection.ExecuteScalarAsync<int>(@"
                    INSERT INTO users (open_id, nickname, avatar, gender, country, province, city, language, balance, points, last_login_time, update_time)
                    VALUES (@OpenId, @Nickname, @Avatar, @Gender, @Country, @Province, @City, @Language, @Balance, @Points, @LastLoginTime, @UpdateTime);
                    SELECT SCOPE_IDENTITY();",
                    user);

                user.Id = userId;
                _logger.LogInformation($"新用户创建成功: UserId={userId}, OpenId={MaskSensitiveInfo(user.OpenId)}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"创建新用户异常: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"内部异常: {ex.InnerException.Message}");
                }
                throw new InvalidOperationException($"创建新用户失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成默认昵称
        /// </summary>
        /// <param name="openId">微信OpenId</param>
        /// <returns>默认昵称</returns>
        private string GenerateDefaultNickname(string openId)
        {
            try
            {
                // 如果OpenId长度足够，则取8位，否则使用完整的OpenId
                return $"User_{(openId.Length >= 8 ? openId.Substring(0, 8) : openId)}";
            }
            catch
            {
                // 如果出现异常，使用固定的默认昵称
                return "User_Default";
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <param name="userInfo">用户信息</param>
        /// <returns>更新后的用户实体</returns>
        private async Task<UserEntity> UpdateUserInfoAsync(UserEntity user, WechatUserInfo userInfo)
        {
            try
            {
                _logger.LogInformation($"用户已存在: UserId={user.Id}, OpenId={MaskSensitiveInfo(user.OpenId)}");

                // 更新用户信息
                if (userInfo != null)
                {
                    _logger.LogInformation($"准备更新用户信息: UserId={user.Id}, 新昵称={userInfo.NickName}");

                    await _dbConnection.ExecuteAsync(@"
                        UPDATE users
                        SET nickname = @Nickname,
                            avatar = @Avatar,
                            gender = @Gender,
                            country = @Country,
                            province = @Province,
                            city = @City,
                            language = @Language,
                            last_login_time = @LastLoginTime,
                            update_time = @UpdateTime
                        WHERE id = @Id",
                        new
                        {
                            Id = user.Id,
                            Nickname = userInfo.NickName ?? user.Nickname,
                            Avatar = userInfo.AvatarUrl ?? user.Avatar,
                            Gender = userInfo.Gender,
                            Country = userInfo.Country ?? user.Country,
                            Province = userInfo.Province ?? user.Province,
                            City = userInfo.City ?? user.City,
                            Language = userInfo.Language ?? user.Language,
                            LastLoginTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        });

                    _logger.LogInformation($"用户信息更新成功: UserId={user.Id}");
                }
                else
                {
                    _logger.LogInformation($"仅更新用户登录时间: UserId={user.Id}");

                    // 仅更新登录时间
                    await _dbConnection.ExecuteAsync(@"
                        UPDATE users
                        SET last_login_time = @LastLoginTime,
                            update_time = @UpdateTime
                        WHERE id = @Id",
                        new
                        {
                            Id = user.Id,
                            LastLoginTime = DateTime.Now,
                            UpdateTime = DateTime.Now
                        });

                    _logger.LogInformation($"用户登录时间更新成功: UserId={user.Id}");
                }

                // 重新查询用户信息以获取最新数据
                var updatedUser = await _dbConnection.QueryFirstOrDefaultAsync<UserEntity>(
                    "SELECT * FROM users WHERE id = @Id",
                    new { Id = user.Id });

                // 确保 OpenId 不为 null
                if (updatedUser != null && string.IsNullOrEmpty(updatedUser.OpenId))
                {
                    _logger.LogWarning($"数据库中用户 OpenId 为空，尝试更新: UserId={updatedUser.Id}");

                    // 如果数据库中的 OpenId 为空，但原始用户对象中有 OpenId，则使用原始的
                    if (!string.IsNullOrEmpty(user.OpenId))
                    {
                        _logger.LogInformation($"使用原始用户的 OpenId: {MaskSensitiveInfo(user.OpenId)}");
                        updatedUser.OpenId = user.OpenId;

                        // 更新数据库中的 OpenId
                        await _dbConnection.ExecuteAsync(
                            "UPDATE users SET open_id = @OpenId WHERE id = @Id",
                            new { Id = updatedUser.Id, OpenId = updatedUser.OpenId });
                    }
                    else
                    {
                        // 如果原始用户对象中也没有 OpenId，则生成一个默认的
                        updatedUser.OpenId = $"user_{updatedUser.Id}";
                        _logger.LogInformation($"生成默认 OpenId: {MaskSensitiveInfo(updatedUser.OpenId)}");

                        // 更新数据库中的 OpenId
                        await _dbConnection.ExecuteAsync(
                            "UPDATE users SET open_id = @OpenId WHERE id = @Id",
                            new { Id = updatedUser.Id, OpenId = updatedUser.OpenId });
                    }
                }

                return updatedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError($"更新用户信息异常: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"内部异常: {ex.InnerException.Message}");
                }
                throw new InvalidOperationException($"更新用户信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成登录响应
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>登录响应</returns>
        private LoginResponse GenerateLoginResponse(UserEntity user)
        {
            try
            {
                // 确保 OpenId 不为 null
                string openId = user.OpenId ?? "";
                if (string.IsNullOrEmpty(openId))
                {
                    _logger.LogWarning($"用户 OpenId 为空，使用用户ID作为替代: UserId={user.Id}");
                    openId = $"user_{user.Id}";
                }

                // 生成JWT令牌
                _logger.LogInformation($"准备生成JWT令牌: UserId={user.Id}, OpenId={MaskSensitiveInfo(openId)}");
                var token = _jwtService.GenerateToken(user.Id, openId);
                _logger.LogInformation($"JWT令牌生成成功");

                // 构建响应
                var loginResponse = new LoginResponse
                {
                    UserId = user.Id,
                    OpenId = openId,
                    Token = token,
                    Balance = user.Balance,
                    Points = user.Points
                };

                _logger.LogInformation($"登录成功: UserId={user.Id}, OpenId={MaskSensitiveInfo(openId)}");
                return loginResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"生成响应异常: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"内部异常: {ex.InnerException.Message}");
                }
                throw new InvalidOperationException($"生成登录响应失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成密码哈希
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <returns>哈希后的密码</returns>
        private string HashPassword(string password)
        {
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
        /// <returns>是否验证成功</returns>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        /// <summary>
        /// 用户名密码登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录响应</returns>
        public async Task<LoginResponse> LoginAsync(UserLoginRequest request)
        {
            // 查询用户是否存在
            // 先尝试通过nickname查询
            var user = await _dbConnection.QueryFirstOrDefaultAsync<UserEntity>(
                "SELECT * FROM users WHERE nickname = @Username",
                new { Username = request.Username });

            // 如果没有找到，尝试通过open_id查询（如果用户名是一个GUID格式）
            if (user == null && Guid.TryParse(request.Username, out _))
            {
                user = await _dbConnection.QueryFirstOrDefaultAsync<UserEntity>(
                    "SELECT * FROM users WHERE open_id = @OpenId",
                    new { OpenId = request.Username });
            }

            if (user == null)
            {
                throw new InvalidOperationException("用户名或密码错误");
            }

            // 验证密码
            if (string.IsNullOrEmpty(user.Password) || !VerifyPassword(request.Password, user.Password))
            {
                throw new InvalidOperationException("用户名或密码错误");
            }

            // 生成JWT令牌
            var token = _jwtService.GenerateToken(user.Id, user.OpenId ?? "");

            // 更新登录时间
            await _dbConnection.ExecuteAsync(
                "UPDATE users SET last_login_time = @LastLoginTime, update_time = @UpdateTime WHERE id = @Id",
                new
                {
                    Id = user.Id,
                    LastLoginTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                });

            return new LoginResponse
            {
                UserId = user.Id,
                OpenId = user.OpenId ?? "",
                Token = token,
                Balance = user.Balance,
                Points = user.Points
            };
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>登录响应</returns>
        public async Task<LoginResponse> RegisterAsync(UserRegisterRequest request)
        {
            // 检查用户名是否已存在
            var existingUser = await _dbConnection.QueryFirstOrDefaultAsync<UserEntity>(
                "SELECT * FROM users WHERE nickname = @Username",
                new { Username = request.Username });

            if (existingUser != null)
            {
                throw new InvalidOperationException("用户名已存在");
            }

            // 生成随机OpenId
            var openId = Guid.NewGuid().ToString("N");

            // 确保昵称不为空，如果没有提供昵称，则使用用户名
            var nickname = string.IsNullOrEmpty(request.Nickname) ? request.Username : request.Nickname;

            // 创建新用户
            var user = new UserEntity
            {
                OpenId = openId,
                Nickname = nickname,
                Avatar = request.Avatar,
                Password = HashPassword(request.Password),
                Gender = request.Gender,
                Balance = 0,
                Points = 0,
                LastLoginTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            // 插入用户数据
            var userId = await _dbConnection.ExecuteScalarAsync<int>(@"
                INSERT INTO users (open_id, nickname, avatar, password, gender, balance, points, last_login_time, update_time)
                VALUES (@OpenId, @Nickname, @Avatar, @Password, @Gender, @Balance, @Points, @LastLoginTime, @UpdateTime);
                SELECT SCOPE_IDENTITY();",
                user);

            user.Id = userId;

            // 生成JWT令牌
            var token = _jwtService.GenerateToken(user.Id, user.OpenId);

            return new LoginResponse
            {
                UserId = user.Id,
                OpenId = user.OpenId,
                Token = token,
                Balance = user.Balance,
                Points = user.Points
            };
        }

        /// <summary>
        /// 微信小程序登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录响应</returns>
        public async Task<LoginResponse> WechatLoginAsync(WechatLoginRequest request)
        {
            try
            {
                // 1. 调用微信接口获取OpenId
                var wechatResponse = await GetWechatOpenIdAsync(request.Code ?? throw new InvalidOperationException("登录Code不能为空"));

                // 2. 查询用户是否存在
                var user = await _dbConnection.QueryFirstOrDefaultAsync<UserEntity>(
                    "SELECT * FROM users WHERE open_id = @OpenId",
                    new { OpenId = wechatResponse.Openid });

                // 3. 处理用户信息
                if (user == null)
                {
                    // 创建新用户
                    user = await CreateUserAsync(wechatResponse.Openid, request.UserInfo);
                }
                else
                {
                    // 更新现有用户信息
                    user = await UpdateUserInfoAsync(user, request.UserInfo);
                }

                // 4. 生成登录响应
                return GenerateLoginResponse(user);
            }
            catch (InvalidOperationException)
            {
                // 直接向上抛出已经格式化的异常
                throw;
            }
            catch (Exception ex)
            {
                // 对于其他异常，记录日志并包装为 InvalidOperationException
                _logger.LogError($"微信登录异常: {ex.Message}");
                throw new InvalidOperationException($"微信登录失败: {ex.Message}", ex);
            }
        }
    }
}
