using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChargingPile.API.Models.Payment
{
    /// <summary>
    /// 支付请求
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        public string OrderId { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-余额
        /// </summary>
        [Required]
        [Range(1, 3)]
        public int PaymentMethod { get; set; }
    }

    /// <summary>
    /// 支付响应
    /// </summary>
    public class PaymentResponse
    {
        /// <summary>
        /// 支付ID
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 支付状态：0-未支付，1-支付中，2-支付成功，3-支付失败，4-已退款
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-余额
        /// </summary>
        public int PaymentMethod { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付链接，如果需要跳转支付
        /// </summary>
        public string PaymentUrl { get; set; }

        /// <summary>
        /// 支付二维码URL，如果需要扫码支付
        /// </summary>
        public string QrCodeUrl { get; set; }
    }

    /// <summary>
    /// 支付状态响应
    /// </summary>
    public class PaymentStatusResponse
    {
        /// <summary>
        /// 支付ID
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 支付状态：0-未支付，1-支付中，2-支付成功，3-支付失败，4-已退款
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-余额
        /// </summary>
        public int PaymentMethod { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 交易流水号
        /// </summary>
        public string TransactionId { get; set; }
    }

    /// <summary>
    /// 账户充值请求
    /// </summary>
    public class RechargeRequest
    {
        /// <summary>
        /// 充值金额
        /// </summary>
        [Required]
        [Range(0.01, 10000)]
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝
        /// </summary>
        [Required]
        [Range(1, 2)]
        public int PaymentMethod { get; set; }
    }

    /// <summary>
    /// 账户充值响应
    /// </summary>
    public class RechargeResponse
    {
        /// <summary>
        /// 充值记录ID
        /// </summary>
        public string RechargeId { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝
        /// </summary>
        public int PaymentMethod { get; set; }

        /// <summary>
        /// 支付链接
        /// </summary>
        public string PaymentUrl { get; set; }

        /// <summary>
        /// 支付二维码URL
        /// </summary>
        public string QrCodeUrl { get; set; }
    }

    /// <summary>
    /// 微信支付配置
    /// </summary>
    public class WechatPayConfig
    {
        /// <summary>
        /// 微信支付商户号
        /// </summary>
        public string MchId { get; set; }

        /// <summary>
        /// 微信支付API密钥
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// 微信支付AppId
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 微信支付通知URL
        /// </summary>
        public string NotifyUrl { get; set; }
    }

    /// <summary>
    /// 微信支付统一下单请求
    /// </summary>
    public class WechatPayUnifiedOrderRequest
    {
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string OutTradeNo { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 标价金额（分）
        /// </summary>
        public int TotalFee { get; set; }

        /// <summary>
        /// 终端IP
        /// </summary>
        public string SpbillCreateIp { get; set; }

        /// <summary>
        /// 交易类型
        /// </summary>
        public string TradeType { get; set; } = "JSAPI";

        /// <summary>
        /// 用户标识
        /// </summary>
        public string OpenId { get; set; }
    }

    /// <summary>
    /// 微信支付统一下单响应
    /// </summary>
    public class WechatPayUnifiedOrderResponse
    {
        /// <summary>
        /// 返回状态码
        /// </summary>
        public string ReturnCode { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string ReturnMsg { get; set; }

        /// <summary>
        /// 业务结果
        /// </summary>
        public string ResultCode { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string ErrCode { get; set; }

        /// <summary>
        /// 错误代码描述
        /// </summary>
        public string ErrCodeDes { get; set; }

        /// <summary>
        /// 预支付交易会话标识
        /// </summary>
        public string PrepayId { get; set; }

        /// <summary>
        /// 二维码链接
        /// </summary>
        public string CodeUrl { get; set; }
    }

    /// <summary>
    /// 微信支付JSAPI参数
    /// </summary>
    public class WechatPayJsApiParameters
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        [JsonPropertyName("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [JsonPropertyName("timeStamp")]
        public string TimeStamp { get; set; }

        /// <summary>
        /// 随机字符串
        /// </summary>
        [JsonPropertyName("nonceStr")]
        public string NonceStr { get; set; }

        /// <summary>
        /// 订单详情扩展字符串
        /// </summary>
        [JsonPropertyName("package")]
        public string Package { get; set; }

        /// <summary>
        /// 签名方式
        /// </summary>
        [JsonPropertyName("signType")]
        public string SignType { get; set; } = "MD5";

        /// <summary>
        /// 签名
        /// </summary>
        [JsonPropertyName("paySign")]
        public string PaySign { get; set; }
    }
}
