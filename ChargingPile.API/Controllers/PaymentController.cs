using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ChargingPile.API.Models.Common;
using ChargingPile.API.Models.Payment;
using ChargingPile.API.Services;
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

        /// <summary>
        /// 构造函数
        /// </summary>
        public PaymentController(
            ILogger<PaymentController> logger,
            PaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
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
        public IActionResult WechatPayPage([FromQuery] string paymentId)
        {
            // 实际项目中，这里应该返回一个包含JSAPI支付参数的HTML页面
            // 这里简化处理，返回一个提示信息
            return Content($"<html><body><h1>微信支付</h1><p>支付ID: {paymentId}</p><p>请在微信小程序中完成支付</p></body></html>", "text/html");
        }

        /// <summary>
        /// 微信充值页面
        /// </summary>
        /// <param name="rechargeId">充值ID</param>
        /// <returns>充值页面</returns>
        [HttpGet("wechat/recharge")]
        [Authorize]
        public IActionResult WechatRechargePage([FromQuery] string rechargeId)
        {
            // 实际项目中，这里应该返回一个包含JSAPI支付参数的HTML页面
            // 这里简化处理，返回一个提示信息
            return Content($"<html><body><h1>微信充值</h1><p>充值ID: {rechargeId}</p><p>请在微信小程序中完成充值</p></body></html>", "text/html");
        }
    }
}
