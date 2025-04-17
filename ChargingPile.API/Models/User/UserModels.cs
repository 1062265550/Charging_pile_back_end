using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChargingPile.API.Models.User
{
    /// <summary>
    /// 简单的Code请求模型
    /// </summary>
    public class CodeRequest
    {
        /// <summary>
        /// 微信登录code
        /// </summary>
        public string Code { get; set; }
    }

    /// <summary>
    /// 微信小程序登录请求模型
    /// </summary>
    public class WechatLoginRequest
    {
        /// <summary>
        /// 微信登录code
        /// </summary>
        [Required(ErrorMessage = "微信登录code不能为空")]
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonPropertyName("userInfo")]
        public WechatUserInfo UserInfo { get; set; }
    }

    /// <summary>
    /// 微信用户信息
    /// </summary>
    public class WechatUserInfo
    {
        /// <summary>
        /// 用户昵称
        /// </summary>
        [JsonPropertyName("nickName")]
        public string NickName { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        [JsonPropertyName("avatarUrl")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// 性别：0-未知，1-男，2-女
        /// </summary>
        [JsonPropertyName("gender")]
        public int Gender { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        [JsonPropertyName("province")]
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; }
    }

    /// <summary>
    /// 更新用户信息请求
    /// </summary>
    public class UpdateUserInfoRequest
    {
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 性别：0-未知，1-男，2-女
        /// </summary>
        public int? Gender { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 语言偏好
        /// </summary>
        public string Language { get; set; }
    }

    /// <summary>
    /// 用户名密码登录请求
    /// </summary>
    public class UserLoginRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }
    }

    /// <summary>
    /// 用户注册请求
    /// </summary>
    public class UserRegisterRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度应在3-50个字符之间")]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度应在6-100个字符之间")]
        public string Password { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 性别：0-未知，1-男，2-女
        /// </summary>
        public int Gender { get; set; } = 0;

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }
    }

    /// <summary>
    /// 登录响应模型
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// 用户令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 用户积分
        /// </summary>
        public int Points { get; set; }
    }

    /// <summary>
    /// 用户信息响应模型
    /// </summary>
    public class UserInfoResponse
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 性别：0-未知，1-男，2-女
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 语言偏好
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 用户积分
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
    }

    /// <summary>
    /// 微信登录响应
    /// </summary>
    public class WechatLoginResponse
    {
        /// <summary>
        /// 会话密钥
        /// </summary>
        [JsonPropertyName("session_key")]
        public string Session_key { get; set; }

        /// <summary>
        /// 用户唯一标识
        /// </summary>
        [JsonPropertyName("openid")]
        public string Openid { get; set; }

        /// <summary>
        /// 用户在开放平台的唯一标识符
        /// </summary>
        [JsonPropertyName("unionid")]
        public string Unionid { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        [JsonPropertyName("errcode")]
        public int Errcode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [JsonPropertyName("errmsg")]
        public string Errmsg { get; set; }
    }

    /// <summary>
    /// 数据库用户模型
    /// </summary>
    public class UserEntity
    {
        /// <summary>
        /// 用户唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户开放平台ID
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 用户密码（加密存储）
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 用户积分
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 性别：0-未知，1-男，2-女
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 语言偏好
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 通用API响应模型
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 状态码：0-成功，非0-失败
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        /// <param name="data">响应数据</param>
        /// <param name="message">提示信息</param>
        /// <returns>API响应</returns>
        public static ApiResponse<T> Success(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Code = 0,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="code">错误码</param>
        /// <returns>API响应</returns>
        public static ApiResponse<T> Fail(string message, int code = 1)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = default
            };
        }
    }
}
