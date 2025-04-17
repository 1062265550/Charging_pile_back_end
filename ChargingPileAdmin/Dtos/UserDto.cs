using System;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 用户数据传输对象
    /// </summary>
    public class UserDto
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
        public int? Gender { get; set; }

        /// <summary>
        /// 性别描述
        /// </summary>
        public string GenderDescription => Gender switch
        {
            0 => "未知",
            1 => "男",
            2 => "女",
            _ => "未知"
        };

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
    /// 创建用户请求
    /// </summary>
    public class CreateUserDto
    {
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
        /// 用户密码
        /// </summary>
        public string Password { get; set; }

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
    /// 更新用户请求
    /// </summary>
    public class UpdateUserDto
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
    /// 用户登录请求
    /// </summary>
    public class UserLoginDto
    {
        /// <summary>
        /// 用户开放平台ID或昵称
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// 用户登录响应
    /// </summary>
    public class UserLoginResponseDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 登录令牌
        /// </summary>
        public string Token { get; set; }
    }

    /// <summary>
    /// 修改密码请求
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// 旧密码
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// 确认新密码
        /// </summary>
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// 充值余额请求
    /// </summary>
    public class RechargeBalanceDto
    {
        /// <summary>
        /// 充值金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-银行卡
        /// </summary>
        public int PaymentMethod { get; set; }
    }
}
