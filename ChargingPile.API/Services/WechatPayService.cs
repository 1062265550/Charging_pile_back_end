using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
        private readonly X509Certificate2 _certificate;

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
                NotifyUrl = _configuration["WechatPay:NotifyUrl"] ?? "https://api.example.com/api/payment/wechat/notify",
                RechargeNotifyUrl = _configuration["WechatPay:RechargeNotifyUrl"] ?? "https://api.example.com/api/payment/wechat/recharge/notify",
                CertPath = _configuration["WechatPay:CertPath"],
                CertPassword = _configuration["WechatPay:CertPassword"]
            };

            if (string.IsNullOrEmpty(_wechatPayConfig.AppId))
            {
                throw new InvalidOperationException("微信小程序AppId未配置");
            }

            // 加载证书
            try
            {
                if (!string.IsNullOrEmpty(_wechatPayConfig.CertPath) && !string.IsNullOrEmpty(_wechatPayConfig.CertPassword))
                {
                    // 尝试多种路径加载证书
                    string certPath = null;
                    bool certLoaded = false;

                    // 定义可能的证书路径
                    var projectRootPath = Path.Combine(Directory.GetCurrentDirectory(), _wechatPayConfig.CertPath);
                    var baseDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _wechatPayConfig.CertPath);
                    var absolutePath = _wechatPayConfig.CertPath;

                    // 1. 先尝试从项目根目录加载
                    if (File.Exists(projectRootPath))
                    {
                        certPath = projectRootPath;
                        _certificate = new X509Certificate2(certPath, _wechatPayConfig.CertPassword);
                        _logger.LogInformation("从项目根目录加载微信支付证书成功: {0}", certPath);
                        certLoaded = true;
                    }

                    // 2. 如果从项目根目录加载失败，尝试从应用程序基目录加载
                    if (!certLoaded && File.Exists(baseDirectoryPath))
                    {
                        certPath = baseDirectoryPath;
                        _certificate = new X509Certificate2(certPath, _wechatPayConfig.CertPassword);
                        _logger.LogInformation("从应用程序基目录加载微信支付证书成功: {0}", certPath);
                        certLoaded = true;
                    }

                    // 3. 如果上述方法都失败，尝试使用绝对路径
                    if (!certLoaded && File.Exists(absolutePath))
                    {
                        certPath = absolutePath;
                        _certificate = new X509Certificate2(certPath, _wechatPayConfig.CertPassword);
                        _logger.LogInformation("使用绝对路径加载微信支付证书成功: {0}", certPath);
                        certLoaded = true;
                    }

                    // 如果所有方法都失败，记录错误
                    if (!certLoaded)
                    {
                        _logger.LogError("微信支付证书加载失败: 无法找到证书文件");
                        _logger.LogError("尝试过的路径: {0}, {1}, {2}",
                            projectRootPath, baseDirectoryPath, absolutePath);
                    }
                }
                else
                {
                    _logger.LogWarning("微信支付证书路径或密码未配置");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "微信支付证书加载失败: {0}", ex.Message);
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
            // 检查证书是否已加载
            if (_certificate == null && !string.IsNullOrEmpty(_wechatPayConfig.CertPath))
            {
                _logger.LogWarning("微信支付证书未加载，可能影响支付功能");
            }

            try
            {
                // 记录支付请求信息
                _logger.LogInformation("微信支付请求参数: 商户号={0}, AppId={1}, 回调URL={2}",
                    _wechatPayConfig.MchId, _wechatPayConfig.AppId, _wechatPayConfig.NotifyUrl);

                Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "创建微信支付订单: 订单号={0}, 金额={1}, 用户OpenId={2}",
                    orderNo, amount, openId);

                // 构建统一下单请求
                // 检查OpenID是否有效（微信OpenID通常以o开头）
                bool useJsApi = !string.IsNullOrEmpty(openId) &&
                               (openId.StartsWith("o") || openId.StartsWith("wx")) &&
                               !openId.StartsWith("user_");

                if (!useJsApi && !string.IsNullOrEmpty(openId))
                {
                    _logger.LogWarning($"检测到无效的OpenID格式: {openId}，将使用NATIVE支付模式");
                }
                else if (string.IsNullOrEmpty(openId))
                {
                    _logger.LogWarning($"未提供OpenID，将使用NATIVE支付模式");
                }

                var request = new WechatPayUnifiedOrderRequest
                {
                    OutTradeNo = orderNo,
                    Body = body,
                    TotalFee = (int)(amount * 100), // 转换为分
                    SpbillCreateIp = clientIp,
                    TradeType = useJsApi ? "JSAPI" : "NATIVE", // 根据OpenID有效性决定使用JSAPI或NATIVE
                    OpenId = useJsApi ? openId : null // 只在JSAPI模式下设置OpenId
                };

                // 记录使用的交易类型和OpenID
                _logger.LogInformation("使用交易类型: {0}，用户OpenID: {1}",
                    request.TradeType, openId);


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
            if (request.TradeType == "JSAPI")
            {
                if (!string.IsNullOrEmpty(request.OpenId) &&
                    (request.OpenId.StartsWith("o") || request.OpenId.StartsWith("wx")) &&
                    !request.OpenId.StartsWith("user_"))
                {
                    parameters.Add("openid", request.OpenId);
                    _logger.LogInformation("添加OpenID参数: {0}", request.OpenId);
                }
                else
                {
                    _logger.LogWarning("检测到无效的OpenID: {0}，将自动切换为NATIVE模式", request.OpenId ?? "空");
                    parameters["trade_type"] = "NATIVE";
                    // 如果已经添加了openid参数，则移除它
                    if (parameters.ContainsKey("openid"))
                    {
                        parameters.Remove("openid");
                        _logger.LogInformation("移除无效的OpenID参数");
                    }
                }
            }
            else
            {
                _logger.LogInformation("不添加OpenID参数，交易类型: {0}", request.TradeType);
            }

            // 生成签名
            parameters.Add("sign", GenerateSign(parameters));

            // 构建XML请求
            var requestXml = new XElement("xml",
                parameters.Select(p => new XElement(p.Key, p.Value))
            );

            // 记录请求XML
            _logger.LogDebug("微信支付统一下单请求XML: {0}", requestXml.ToString());

            // 发送请求
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(requestXml.ToString(), Encoding.UTF8, "application/xml");
            var response = await client.PostAsync("https://api.mch.weixin.qq.com/pay/unifiedorder", content);

            // 解析响应
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("微信支付统一下单响应: {0}", responseContent);

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
