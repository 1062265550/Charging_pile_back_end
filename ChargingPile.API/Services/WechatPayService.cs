using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using ChargingPile.API.Models.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LoggerExtensions;

namespace ChargingPile.API.Services
{
    /// <summary>
    /// 微信支付服务
    /// </summary>
    public class WechatPayService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WechatPayService> _logger;
        private readonly WechatPayConfig _wechatPayConfig;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WechatPayService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WechatPayService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            // 从配置中读取微信支付配置
            _wechatPayConfig = new WechatPayConfig
            {
                AppId = _configuration["WechatMiniProgram:AppId"],
                MchId = _configuration["WechatPay:MchId"] ?? "1900000109", // 使用测试商户号
                ApiKey = _configuration["WechatPay:ApiKey"] ?? "8934e7d15453e97507ef794cf7b0519d", // 使用测试密钥
                NotifyUrl = _configuration["WechatPay:NotifyUrl"] ?? "https://api.example.com/api/payment/wechat/notify"
            };

            if (string.IsNullOrEmpty(_wechatPayConfig.AppId))
            {
                throw new InvalidOperationException("微信小程序AppId未配置");
            }
        }

        /// <summary>
        /// 创建微信支付订单
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <param name="amount">金额（元）</param>
        /// <param name="body">商品描述</param>
        /// <param name="openId">用户OpenId</param>
        /// <param name="clientIp">客户端IP</param>
        /// <returns>支付参数</returns>
        public async Task<(bool Success, string? ErrorMessage, WechatPayJsApiParameters? JsApiParameters, string? QrCodeUrl)> CreateWechatPayOrderAsync(
            string orderNo,
            decimal amount,
            string body,
            string openId,
            string clientIp)
        {
            try
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "创建微信支付订单: 订单号={0}, 金额={1}, 用户OpenId={2}",
                    orderNo, amount, openId);

                // 构建统一下单请求
                var request = new WechatPayUnifiedOrderRequest
                {
                    OutTradeNo = orderNo,
                    Body = body,
                    TotalFee = (int)(amount * 100), // 转换为分
                    SpbillCreateIp = clientIp,
                    TradeType = "JSAPI",
                    OpenId = openId
                };

                // 调用微信支付统一下单接口
                var response = await UnifiedOrderAsync(request);

                if (response.ReturnCode != "SUCCESS" || response.ResultCode != "SUCCESS")
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信支付统一下单失败: {0}, {1}, {2}, {3}",
                        response.ReturnCode, response.ReturnMsg, response.ErrCode, response.ErrCodeDes);

                    return (false, $"微信支付下单失败: {response.ReturnMsg ?? response.ErrCodeDes}", null, null);
                }

                // 生成JSAPI支付参数
                var jsApiParameters = GenerateJsApiParameters(response.PrepayId);

                return (true, null, jsApiParameters, response.CodeUrl);
            }
            catch (Exception ex)
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogError(_logger, ex, "创建微信支付订单异常: {0}", ex.Message);
                return (false, $"创建微信支付订单异常: {ex.Message}", null, null);
            }
        }

        /// <summary>
        /// 统一下单接口
        /// </summary>
        private async Task<WechatPayUnifiedOrderResponse> UnifiedOrderAsync(WechatPayUnifiedOrderRequest request)
        {
            // 构建请求参数
            var parameters = new Dictionary<string, string>
            {
                { "appid", _wechatPayConfig.AppId },
                { "mch_id", _wechatPayConfig.MchId },
                { "nonce_str", GenerateNonceStr() },
                { "body", request.Body },
                { "out_trade_no", request.OutTradeNo },
                { "total_fee", request.TotalFee.ToString() },
                { "spbill_create_ip", request.SpbillCreateIp },
                { "notify_url", _wechatPayConfig.NotifyUrl },
                { "trade_type", request.TradeType }
            };

            // 如果是JSAPI支付，需要传入openid
            if (request.TradeType == "JSAPI" && !string.IsNullOrEmpty(request.OpenId))
            {
                parameters.Add("openid", request.OpenId);
            }

            // 生成签名
            parameters.Add("sign", GenerateSign(parameters));

            // 构建XML请求
            var requestXml = new XElement("xml",
                parameters.Select(p => new XElement(p.Key, p.Value))
            );

            // 发送请求
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(requestXml.ToString(), Encoding.UTF8, "application/xml");
            var response = await client.PostAsync("https://api.mch.weixin.qq.com/pay/unifiedorder", content);

            // 解析响应
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseXml = XDocument.Parse(responseContent);

            // 构建响应对象
            var result = new WechatPayUnifiedOrderResponse
            {
                ReturnCode = GetXmlValue(responseXml, "return_code"),
                ReturnMsg = GetXmlValue(responseXml, "return_msg"),
                ResultCode = GetXmlValue(responseXml, "result_code"),
                ErrCode = GetXmlValue(responseXml, "err_code"),
                ErrCodeDes = GetXmlValue(responseXml, "err_code_des"),
                PrepayId = GetXmlValue(responseXml, "prepay_id"),
                CodeUrl = GetXmlValue(responseXml, "code_url")
            };

            return result;
        }

        /// <summary>
        /// 生成JSAPI支付参数
        /// </summary>
        private WechatPayJsApiParameters GenerateJsApiParameters(string prepayId)
        {
            var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            var nonceStr = GenerateNonceStr();
            var package = $"prepay_id={prepayId}";
            var signType = "MD5";

            // 构建签名参数
            var parameters = new Dictionary<string, string>
            {
                { "appId", _wechatPayConfig.AppId },
                { "timeStamp", timeStamp },
                { "nonceStr", nonceStr },
                { "package", package },
                { "signType", signType }
            };

            // 生成签名
            var paySign = GenerateSign(parameters);

            // 返回JSAPI支付参数
            return new WechatPayJsApiParameters
            {
                AppId = _wechatPayConfig.AppId,
                TimeStamp = timeStamp,
                NonceStr = nonceStr,
                Package = package,
                SignType = signType,
                PaySign = paySign
            };
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        private string GenerateNonceStr()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        private string GenerateSign(Dictionary<string, string> parameters)
        {
            // 按照参数名ASCII码从小到大排序
            var sortedParameters = parameters.OrderBy(p => p.Key).ToList();

            // 组装签名字符串
            var stringBuilder = new StringBuilder();
            foreach (var parameter in sortedParameters)
            {
                if (!string.IsNullOrEmpty(parameter.Value))
                {
                    stringBuilder.Append($"{parameter.Key}={parameter.Value}&");
                }
            }
            stringBuilder.Append($"key={_wechatPayConfig.ApiKey}");

            // 计算MD5签名
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            var sign = BitConverter.ToString(bytes).Replace("-", "").ToUpper();

            return sign;
        }

        /// <summary>
        /// 获取XML节点值
        /// </summary>
        private string GetXmlValue(XDocument xml, string nodeName)
        {
            var node = xml.Root?.Element(nodeName);
            return node?.Value;
        }

        /// <summary>
        /// 验证微信支付回调签名
        /// </summary>
        public bool VerifyNotifySign(Dictionary<string, string> parameters, string sign)
        {
            // 移除sign参数
            var parametersToSign = new Dictionary<string, string>(parameters);
            parametersToSign.Remove("sign");

            // 生成签名
            var generatedSign = GenerateSign(parametersToSign);

            // 比较签名
            return generatedSign == sign;
        }

        /// <summary>
        /// 处理微信支付回调
        /// </summary>
        public async Task<(bool Success, string? OrderNo, decimal Amount, string? TransactionId)> ProcessNotifyAsync(string notifyXml)
        {
            try
            {
                // 解析XML
                var xml = XDocument.Parse(notifyXml);
                var parameters = xml.Root?.Elements()
                    .ToDictionary(e => e.Name.LocalName, e => e.Value);

                if (parameters == null)
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信支付回调XML解析失败");
                    return (false, null, 0, null);
                }

                // 获取返回状态码
                var returnCode = parameters.TryGetValue("return_code", out var returnCodeValue) ? returnCodeValue : null;
                var resultCode = parameters.TryGetValue("result_code", out var resultCodeValue) ? resultCodeValue : null;

                if (returnCode != "SUCCESS" || resultCode != "SUCCESS")
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信支付回调通知失败: {0}, {1}", returnCode, resultCode);
                    return (false, null, 0, null);
                }

                // 获取签名
                var sign = parameters.TryGetValue("sign", out var signValue) ? signValue : null;

                // 验证签名
                if (string.IsNullOrEmpty(sign) || !VerifyNotifySign(parameters, sign))
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信支付回调签名验证失败");
                    return (false, null, 0, null);
                }

                // 获取订单号和金额
                var orderNo = parameters.TryGetValue("out_trade_no", out var orderNoValue) ? orderNoValue : null;
                var totalFee = parameters.TryGetValue("total_fee", out var totalFeeValue) && int.TryParse(totalFeeValue, out var fee) ? fee : 0;
                var transactionId = parameters.TryGetValue("transaction_id", out var transactionIdValue) ? transactionIdValue : null;

                // 转换金额为元
                var amount = totalFee / 100m;

                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "微信支付回调成功: 订单号={0}, 金额={1}, 交易号={2}",
                    orderNo, amount, transactionId);

                return (true, orderNo, amount, transactionId);
            }
            catch (Exception ex)
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogError(_logger, ex, "处理微信支付回调异常: {0}", ex.Message);
                return (false, null, 0, null);
            }
        }
    }
}
