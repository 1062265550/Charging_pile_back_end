using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ChargingPile.API.Models.Common;
using ChargingPile.API.Models.Payment;
using ChargingPile.API.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 支付控制器
    /// </summary>
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly PaymentService _paymentService;
        private readonly IDbConnection _dbConnection;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PaymentController(
            ILogger<PaymentController> logger,
            PaymentService paymentService,
            IDbConnection dbConnection)
        {
            _logger = logger;
            _paymentService = paymentService;
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        private int GetCurrentUserId()
        {
            // 从 JWT 中获取用户ID
            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogInformation("当前登录用户ID: {UserId}", userId);
                return userId;
            }
            _logger.LogWarning("无法获取用户ID，用户可能未登录或会话已过期");
            return 0;
        }

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="request">支付请求</param>
        /// <returns>支付响应</returns>
        [HttpPost("pay")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PaymentResponse>>> Pay([FromBody] PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("订单支付请求: OrderId={OrderId}, PaymentMethod={PaymentMethod}",
                    request.OrderId, request.PaymentMethod);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<PaymentResponse>.Fail("未授权的访问", 401));
                }

                // 获取客户端IP
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // 创建支付订单
                var response = await _paymentService.CreatePaymentAsync(request.OrderId, request.PaymentMethod, userId, clientIp);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单支付异常: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<PaymentResponse>.Fail($"订单支付异常: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// 查询支付状态
        /// </summary>
        /// <param name="id">支付ID</param>
        /// <returns>支付状态响应</returns>
        [HttpGet("{id}/status")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PaymentStatusResponse>>> GetPaymentStatus(string id)
        {
            try
            {
                _logger.LogInformation("查询支付状态请求: PaymentId={PaymentId}", id);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<PaymentStatusResponse>.Fail("未授权的访问", 401));
                }

                // 查询支付状态
                var response = await _paymentService.GetPaymentStatusAsync(id, userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询支付状态异常: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<PaymentStatusResponse>.Fail($"查询支付状态异常: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// 账户充值
        /// </summary>
        /// <param name="request">充值请求</param>
        /// <returns>充值响应</returns>
        [HttpPost("recharge")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RechargeResponse>>> Recharge([FromBody] RechargeRequest request)
        {
            try
            {
                _logger.LogInformation("账户充值请求: Amount={Amount}, PaymentMethod={PaymentMethod}",
                    request.Amount, request.PaymentMethod);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<RechargeResponse>.Fail("未授权的访问", 401));
                }

                // 获取客户端IP
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // 创建充值订单
                var response = await _paymentService.CreateRechargeAsync(request.Amount, request.PaymentMethod, userId, clientIp);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "账户充值异常: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<RechargeResponse>.Fail($"账户充值异常: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// 微信支付回调
        /// </summary>
        /// <returns>回调响应</returns>
        [HttpPost("wechat/notify")]
        [AllowAnonymous]
        public async Task<IActionResult> WechatPayNotify()
        {
            try
            {
                _logger.LogInformation("收到微信支付回调");

                // 读取请求体
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var notifyXml = await reader.ReadToEndAsync();

                _logger.LogDebug("微信支付回调内容: {NotifyXml}", notifyXml);

                // 处理微信支付回调
                var success = await _paymentService.ProcessWechatPayNotifyAsync(notifyXml);

                // 返回处理结果
                if (success)
                {
                    var successXml = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                    return Content(successXml, "application/xml");
                }
                else
                {
                    var failXml = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[处理失败]]></return_msg></xml>";
                    return Content(failXml, "application/xml");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理微信支付回调异常: {Message}", ex.Message);
                var failXml = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[处理异常]]></return_msg></xml>";
                return Content(failXml, "application/xml");
            }
        }

        /// <summary>
        /// 微信充值回调
        /// </summary>
        /// <returns>回调响应</returns>
        [HttpPost("wechat/recharge/notify")]
        [AllowAnonymous]
        public async Task<IActionResult> WechatRechargeNotify()
        {
            try
            {
                _logger.LogInformation("收到微信充值回调");

                // 读取请求体
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var notifyXml = await reader.ReadToEndAsync();

                _logger.LogDebug("微信充值回调内容: {NotifyXml}", notifyXml);

                // 处理微信充值回调
                var success = await _paymentService.ProcessWechatRechargeNotifyAsync(notifyXml);

                // 返回处理结果
                if (success)
                {
                    var successXml = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                    return Content(successXml, "application/xml");
                }
                else
                {
                    var failXml = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[处理失败]]></return_msg></xml>";
                    return Content(failXml, "application/xml");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理微信充值回调异常: {Message}", ex.Message);
                var failXml = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[处理异常]]></return_msg></xml>";
                return Content(failXml, "application/xml");
            }
        }

        /// <summary>
        /// 微信支付页面
        /// </summary>
        /// <param name="paymentId">支付ID</param>
        /// <returns>支付页面</returns>
        [HttpGet("wechat/pay")]
        [Authorize]
        public async Task<IActionResult> WechatPayPage([FromQuery] string paymentId)
        {
            try
            {
                // 查询支付记录
                var sql = @"
                    SELECT p.js_api_params, p.amount, o.order_no
                    FROM payment_records p
                    JOIN orders o ON p.order_id = o.id
                    WHERE p.id = @PaymentId";

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { PaymentId = paymentId });

                if (result == null)
                {
                    return NotFound("支付记录不存在");
                }

                var jsApiParams = result.js_api_params;
                var amount = result.amount;
                var orderNo = result.order_no;

                if (string.IsNullOrEmpty(jsApiParams))
                {
                    return BadRequest("支付参数不存在");
                }

                // 构建支付页面
                var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>微信支付</title>
                    <script src='https://res.wx.qq.com/open/js/jweixin-1.6.0.js'></script>
                    <style>
                        body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ text-align: center; padding: 20px 0; }}
                        .header h1 {{ color: #333; margin: 0; }}
                        .content {{ background-color: #fff; border-radius: 5px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .order-info {{ margin-bottom: 20px; }}
                        .order-info p {{ margin: 5px 0; color: #666; }}
                        .order-info .amount {{ font-size: 24px; color: #333; font-weight: bold; }}
                        .btn {{ display: block; width: 100%; padding: 12px 0; background-color: #07c160; color: #fff; border: none; border-radius: 5px; font-size: 16px; cursor: pointer; text-align: center; margin-top: 20px; }}
                        .btn:hover {{ background-color: #06ad56; }}
                        .footer {{ text-align: center; margin-top: 20px; color: #999; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>微信支付</h1>
                        </div>
                        <div class='content'>
                            <div class='order-info'>
                                <p>订单编号: {orderNo}</p>
                                <p>支付金额: <span class='amount'>￥{amount}</span></p>
                            </div>
                            <button id='payBtn' class='btn'>立即支付</button>
                        </div>
                        <div class='footer'>
                            <p>充电桩管理系统 &copy; {DateTime.Now.Year}</p>
                        </div>
                    </div>

                    <script>
                        document.getElementById('payBtn').onclick = function() {{
                            var params = {jsApiParams};

                            // 调用微信支付
                            wx.chooseWXPay({{
                                timestamp: params.timeStamp,
                                nonceStr: params.nonceStr,
                                package: params.package,
                                signType: params.signType,
                                paySign: params.paySign,
                                success: function(res) {{
                                    alert('支付成功');
                                    window.location.href = '/api/payment/success?paymentId={paymentId}';
                                }},
                                fail: function(res) {{
                                    alert('支付失败: ' + JSON.stringify(res));
                                }}
                            }});
                        }};
                    </script>
                </body>
                </html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成微信支付页面异常: {Message}", ex.Message);
                return StatusCode(500, $"生成微信支付页面异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 微信充值页面
        /// </summary>
        /// <param name="rechargeId">充值ID</param>
        /// <returns>充值页面</returns>
        [HttpGet("wechat/recharge")]
        [Authorize]
        public async Task<IActionResult> WechatRechargePage([FromQuery] string rechargeId)
        {
            try
            {
                // 查询充值记录
                var sql = @"
                    SELECT js_api_params, amount
                    FROM recharge_records
                    WHERE id = @RechargeId";

                var result = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { RechargeId = rechargeId });

                if (result == null)
                {
                    return NotFound("充值记录不存在");
                }

                var jsApiParams = result.js_api_params;
                var amount = result.amount;

                if (string.IsNullOrEmpty(jsApiParams))
                {
                    return BadRequest("支付参数不存在");
                }

                // 构建充值页面
                var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>微信充值</title>
                    <script src='https://res.wx.qq.com/open/js/jweixin-1.6.0.js'></script>
                    <style>
                        body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ text-align: center; padding: 20px 0; }}
                        .header h1 {{ color: #333; margin: 0; }}
                        .content {{ background-color: #fff; border-radius: 5px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .recharge-info {{ margin-bottom: 20px; }}
                        .recharge-info p {{ margin: 5px 0; color: #666; }}
                        .recharge-info .amount {{ font-size: 24px; color: #333; font-weight: bold; }}
                        .btn {{ display: block; width: 100%; padding: 12px 0; background-color: #07c160; color: #fff; border: none; border-radius: 5px; font-size: 16px; cursor: pointer; text-align: center; margin-top: 20px; }}
                        .btn:hover {{ background-color: #06ad56; }}
                        .footer {{ text-align: center; margin-top: 20px; color: #999; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>账户充值</h1>
                        </div>
                        <div class='content'>
                            <div class='recharge-info'>
                                <p>充值金额: <span class='amount'>￥{amount}</span></p>
                            </div>
                            <button id='payBtn' class='btn'>立即充值</button>
                        </div>
                        <div class='footer'>
                            <p>充电桩管理系统 &copy; {DateTime.Now.Year}</p>
                        </div>
                    </div>

                    <script>
                        document.getElementById('payBtn').onclick = function() {{
                            var params = {jsApiParams};

                            // 调用微信支付
                            wx.chooseWXPay({{
                                timestamp: params.timeStamp,
                                nonceStr: params.nonceStr,
                                package: params.package,
                                signType: params.signType,
                                paySign: params.paySign,
                                success: function(res) {{
                                    alert('充值成功');
                                    window.location.href = '/api/payment/recharge/success?rechargeId={rechargeId}';
                                }},
                                fail: function(res) {{
                                    alert('充值失败: ' + JSON.stringify(res));
                                }}
                            }});
                        }};
                    </script>
                </body>
                </html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成微信充值页面异常: {Message}", ex.Message);
                return StatusCode(500, $"生成微信充值页面异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 支付成功页面
        /// </summary>
        /// <param name="paymentId">支付ID</param>
        /// <returns>支付成功页面</returns>
        [HttpGet("success")]
        public IActionResult PaymentSuccess([FromQuery] string paymentId)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>支付成功</title>
                <style>
                    body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; text-align: center; }}
                    .success-icon {{ font-size: 80px; color: #07c160; margin: 20px 0; }}
                    .content {{ background-color: #fff; border-radius: 5px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                    .title {{ color: #333; margin: 10px 0; }}
                    .message {{ color: #666; margin: 10px 0; }}
                    .btn {{ display: inline-block; padding: 10px 20px; background-color: #07c160; color: #fff; border: none; border-radius: 5px; font-size: 16px; cursor: pointer; text-decoration: none; margin-top: 20px; }}
                    .btn:hover {{ background-color: #06ad56; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='content'>
                        <div class='success-icon'>✓</div>
                        <h1 class='title'>支付成功</h1>
                        <p class='message'>您的订单已支付成功，感谢您的使用！</p>
                        <button onclick='window.close()' class='btn'>关闭页面</button>
                    </div>
                </div>
            </body>
            </html>";

            return Content(html, "text/html");
        }

        /// <summary>
        /// 充值成功页面
        /// </summary>
        /// <param name="rechargeId">充值ID</param>
        /// <returns>充值成功页面</returns>
        [HttpGet("recharge/success")]
        public IActionResult RechargeSuccess([FromQuery] string rechargeId)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>充值成功</title>
                <style>
                    body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; text-align: center; }}
                    .success-icon {{ font-size: 80px; color: #07c160; margin: 20px 0; }}
                    .content {{ background-color: #fff; border-radius: 5px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                    .title {{ color: #333; margin: 10px 0; }}
                    .message {{ color: #666; margin: 10px 0; }}
                    .btn {{ display: inline-block; padding: 10px 20px; background-color: #07c160; color: #fff; border: none; border-radius: 5px; font-size: 16px; cursor: pointer; text-decoration: none; margin-top: 20px; }}
                    .btn:hover {{ background-color: #06ad56; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='content'>
                        <div class='success-icon'>✓</div>
                        <h1 class='title'>充值成功</h1>
                        <p class='message'>您的账户已成功充值，感谢您的使用！</p>
                        <button onclick='window.close()' class='btn'>关闭页面</button>
                    </div>
                </div>
            </body>
            </html>";

            return Content(html, "text/html");
        }

        /// <summary>
        /// 微信支付测试页面
        /// </summary>
        /// <returns>测试页面</returns>
        [HttpGet("wechat/test")]
        [AllowAnonymous]
        public IActionResult WechatPayTest()
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>微信支付测试</title>
                <style>
                    body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f5f5; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ text-align: center; padding: 20px 0; }}
                    .header h1 {{ color: #333; margin: 0; }}
                    .content {{ background-color: #fff; border-radius: 5px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                    .form-group {{ margin-bottom: 15px; }}
                    .form-group label {{ display: block; margin-bottom: 5px; color: #666; }}
                    .form-group input {{ width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }}
                    .btn {{ display: block; width: 100%; padding: 10px 0; background-color: #07c160; color: #fff; border: none; border-radius: 5px; font-size: 16px; cursor: pointer; text-align: center; margin-top: 20px; }}
                    .btn:hover {{ background-color: #06ad56; }}
                    .footer {{ text-align: center; margin-top: 20px; color: #999; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>微信支付测试</h1>
                    </div>
                    <div class='content'>
                        <h2>订单支付测试</h2>
                        <form action='/api/payment/pay' method='post'>
                            <div class='form-group'>
                                <label for='orderId'>订单ID</label>
                                <input type='text' id='orderId' name='orderId' placeholder='输入订单ID' required>
                            </div>
                            <button type='submit' class='btn'>发起支付</button>
                        </form>

                        <h2 style='margin-top: 30px;'>账户充值测试</h2>
                        <form action='/api/payment/recharge' method='post'>
                            <div class='form-group'>
                                <label for='amount'>充值金额</label>
                                <input type='number' id='amount' name='amount' placeholder='输入充值金额' required>
                            </div>
                            <button type='submit' class='btn'>发起充值</button>
                        </form>
                    </div>
                    <div class='footer'>
                        <p>充电桩管理系统 &copy; {DateTime.Now.Year}</p>
                    </div>
                </div>
            </body>
            </html>";

            return Content(html, "text/html");
        }
    }
}
