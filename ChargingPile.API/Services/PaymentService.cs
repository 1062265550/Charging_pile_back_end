using System;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using ChargingPile.API.Models.Common;
using ChargingPile.API.Models.Payment;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LoggerExtensions;

namespace ChargingPile.API.Services
{
    /// <summary>
    /// 支付服务
    /// </summary>
    public class PaymentService
    {
        private readonly IDbConnection _dbConnection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly WechatPayService _wechatPayService;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PaymentService(
            IDbConnection dbConnection,
            IConfiguration configuration,
            ILogger<PaymentService> logger,
            WechatPayService wechatPayService)
        {
            _dbConnection = dbConnection;
            _configuration = configuration;
            _logger = logger;
            _wechatPayService = wechatPayService;
        }

        /// <summary>
        /// 创建支付订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentMethod">支付方式</param>
        /// <param name="userId">用户ID</param>
        /// <param name="clientIp">客户端IP</param>
        /// <returns>支付响应</returns>
        public async Task<ApiResponse<PaymentResponse>> CreatePaymentAsync(
            string orderId,
            int paymentMethod,
            int userId,
            string clientIp)
        {
            try
            {
                _logger.LogInformation("创建支付订单: 订单ID={OrderId}, 支付方式={PaymentMethod}, 用户ID={UserId}",
                    orderId, paymentMethod, userId);

                // 查询订单信息
                var orderSql = @"
                    SELECT o.id AS Id, o.order_no AS OrderNo, o.amount AS Amount, o.user_id AS UserId,
                           o.status AS Status, o.payment_status AS PaymentStatus
                    FROM orders o
                    WHERE o.id = @OrderId AND o.user_id = @UserId";

                var order = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(orderSql, new { OrderId = orderId, UserId = userId });

                if (order == null)
                {
                    return ApiResponse<PaymentResponse>.Fail("订单不存在");
                }

                if (order.PaymentStatus == 1)
                {
                    return ApiResponse<PaymentResponse>.Fail("订单已支付");
                }

                // 生成支付ID
                var paymentId = Guid.NewGuid().ToString();

                // 创建支付记录
                var createPaymentSql = @"
                    INSERT INTO payment_records (
                        id, order_id, user_id, payment_method, amount,
                        payment_status, create_time, update_time
                    ) VALUES (
                        @Id, @OrderId, @UserId, @PaymentMethod, @Amount,
                        0, @CreateTime, @UpdateTime
                    )";

                await _dbConnection.ExecuteAsync(createPaymentSql, new
                {
                    Id = paymentId,
                    OrderId = orderId,
                    UserId = userId,
                    PaymentMethod = paymentMethod,
                    Amount = order.Amount,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                });

                // 构建支付响应
                var response = new PaymentResponse
                {
                    PaymentId = paymentId,
                    OrderId = orderId,
                    PaymentStatus = 0, // 未支付
                    PaymentMethod = paymentMethod,
                    Amount = order.Amount
                };

                // 根据支付方式处理
                switch (paymentMethod)
                {
                    case 1: // 微信支付
                        await ProcessWechatPayAsync(response, order.OrderNo, userId, clientIp);
                        break;
                    case 2: // 支付宝支付
                        // 暂不实现
                        response.PaymentUrl = $"/api/payment/alipay?paymentId={paymentId}";
                        break;
                    case 3: // 余额支付
                        await ProcessBalancePayAsync(response, userId);
                        break;
                    default:
                        return ApiResponse<PaymentResponse>.Fail("不支持的支付方式");
                }

                return ApiResponse<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建支付订单异常: {Message}", ex.Message);
                return ApiResponse<PaymentResponse>.Fail($"创建支付订单异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理微信支付
        /// </summary>
        private async Task ProcessWechatPayAsync(PaymentResponse response, string orderNo, int userId, string clientIp)
        {
            try
            {
                // 查询用户OpenId
                var userSql = "SELECT open_id FROM users WHERE id = @UserId";
                var openId = await _dbConnection.ExecuteScalarAsync<string>(userSql, new { UserId = userId });

                _logger.LogInformation("从数据库获取的用户OpenID: {OpenId}", openId);

                if (string.IsNullOrEmpty(openId))
                {
                    _logger.LogWarning("用户OpenId不存在，将使用NATIVE支付模式");
                }

                // 确保OpenID不是以user_开头的格式
                if (!string.IsNullOrEmpty(openId) && openId.StartsWith("user_"))
                {
                    _logger.LogWarning("检测到无效的OpenID格式: {OpenId}，尝试从数据库获取真实OpenID", openId);

                    // 尝试获取真实的OpenID（如果存在）
                    var realOpenIdSql = "SELECT open_id FROM users WHERE id = @UserId AND open_id NOT LIKE 'user_%' AND open_id LIKE 'o%'";
                    var realOpenId = await _dbConnection.ExecuteScalarAsync<string>(realOpenIdSql, new { UserId = userId });

                    if (!string.IsNullOrEmpty(realOpenId))
                    {
                        _logger.LogInformation("找到真实的OpenID: {OpenId}", realOpenId);
                        openId = realOpenId;
                    }
                    else
                    {
                        _logger.LogWarning("无法找到真实的OpenID，将使用NATIVE支付模式");
                    }
                }

                // 检查OpenID是否有效（微信OpenID通常以o开头）
                bool isValidOpenId = !string.IsNullOrEmpty(openId) &&
                                    (openId.StartsWith("o") || openId.StartsWith("wx")) &&
                                    !openId.StartsWith("user_");

                if (!isValidOpenId)
                {
                    _logger.LogWarning("用户 {UserId} 没有有效的OpenID，将使用NATIVE支付模式", userId);
                }

                // 创建微信支付订单
                var (success, errorMessage, jsApiParameters, qrCodeUrl) = await _wechatPayService.CreateWechatPayOrderAsync(
                    orderNo,
                    response.Amount,
                    $"充电桩订单-{orderNo}",
                    openId,
                    clientIp);

                if (!success)
                {
                    throw new InvalidOperationException($"创建微信支付订单失败: {errorMessage}");
                }

                // 更新支付记录
                var updateSql = @"
                    UPDATE payment_records
                    SET payment_status = 1, update_time = @UpdateTime
                    WHERE id = @PaymentId";

                await _dbConnection.ExecuteAsync(updateSql, new
                {
                    PaymentId = response.PaymentId,
                    UpdateTime = DateTime.Now
                });

                // 设置支付链接和参数
                response.PaymentStatus = 1; // 支付中
                response.PaymentUrl = $"/api/payment/wechat/pay?paymentId={response.PaymentId}";
                response.QrCodeUrl = qrCodeUrl;
                response.JsApiParameters = jsApiParameters; // 添加JSAPI参数

                // 记录支付信息
                if (jsApiParameters != null)
                {
                    _logger.LogInformation("生成微信JSAPI支付参数: {0}",
                        System.Text.Json.JsonSerializer.Serialize(jsApiParameters));
                }

                if (!string.IsNullOrEmpty(qrCodeUrl))
                {
                    _logger.LogInformation("生成微信扫码支付URL: {0}", qrCodeUrl);
                }

                // 保存JSAPI参数
                var saveJsApiSql = @"
                    UPDATE payment_records
                    SET js_api_params = @JsApiParams
                    WHERE id = @PaymentId";

                await _dbConnection.ExecuteAsync(saveJsApiSql, new
                {
                    PaymentId = response.PaymentId,
                    JsApiParams = System.Text.Json.JsonSerializer.Serialize(jsApiParameters)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理微信支付异常: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 处理余额支付
        /// </summary>
        private async Task ProcessBalancePayAsync(PaymentResponse response, int userId)
        {
            try
            {
                // 查询用户余额
                var userSql = "SELECT balance FROM users WHERE id = @UserId";
                var balance = await _dbConnection.ExecuteScalarAsync<decimal>(userSql, new { UserId = userId });

                if (balance < response.Amount)
                {
                    throw new InvalidOperationException("账户余额不足");
                }

                // 开始事务
                if (_dbConnection.State != ConnectionState.Open)
                {
                    _dbConnection.Open();
                }

                using var transaction = _dbConnection.BeginTransaction();

                try
                {
                    // 扣减用户余额
                    var updateBalanceSql = @"
                        UPDATE users
                        SET balance = balance - @Amount, update_time = @UpdateTime
                        WHERE id = @UserId";

                    await _dbConnection.ExecuteAsync(updateBalanceSql, new
                    {
                        UserId = userId,
                        Amount = response.Amount,
                        UpdateTime = DateTime.Now
                    }, transaction);

                    // 更新支付记录
                    var updatePaymentSql = @"
                        UPDATE payment_records
                        SET payment_status = 2, payment_time = @PaymentTime, update_time = @UpdateTime
                        WHERE id = @PaymentId";

                    await _dbConnection.ExecuteAsync(updatePaymentSql, new
                    {
                        PaymentId = response.PaymentId,
                        PaymentTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    }, transaction);

                    // 更新订单支付状态
                    var updateOrderSql = @"
                        UPDATE orders
                        SET payment_status = 1, payment_method = @PaymentMethod, payment_time = @PaymentTime, update_time = @UpdateTime
                        WHERE id = @OrderId";

                    await _dbConnection.ExecuteAsync(updateOrderSql, new
                    {
                        OrderId = response.OrderId,
                        PaymentMethod = response.PaymentMethod,
                        PaymentTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    }, transaction);

                    // 添加余额变动记录
                    var addBalanceRecordSql = @"
                        INSERT INTO balance_records (
                            user_id, amount, balance, type, related_id,
                            description, create_time
                        ) VALUES (
                            @UserId, @Amount, @Balance, @Type, @RelatedId,
                            @Description, @CreateTime
                        )";

                    await _dbConnection.ExecuteAsync(addBalanceRecordSql, new
                    {
                        UserId = userId,
                        Amount = -response.Amount, // 负数表示支出
                        Balance = balance - response.Amount,
                        Type = 2, // 2表示订单支付
                        RelatedId = response.OrderId,
                        Description = $"订单支付-{response.OrderId}",
                        CreateTime = DateTime.Now
                    }, transaction);

                    // 提交事务
                    transaction.Commit();

                    // 设置支付状态
                    response.PaymentStatus = 2; // 支付成功
                }
                catch (Exception)
                {
                    // 回滚事务
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理余额支付异常: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 查询支付状态
        /// </summary>
        /// <param name="paymentId">支付ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>支付状态响应</returns>
        public async Task<ApiResponse<PaymentStatusResponse>> GetPaymentStatusAsync(string paymentId, int userId)
        {
            try
            {
                var sql = @"
                    SELECT p.id AS PaymentId, p.order_id AS OrderId, p.payment_status AS PaymentStatus,
                           p.payment_method AS PaymentMethod, p.amount AS Amount, p.payment_time AS PaymentTime,
                           p.transaction_id AS TransactionId
                    FROM payment_records p
                    WHERE p.id = @PaymentId AND p.user_id = @UserId";

                var payment = await _dbConnection.QueryFirstOrDefaultAsync<PaymentStatusResponse>(sql, new { PaymentId = paymentId, UserId = userId });

                if (payment == null)
                {
                    return ApiResponse<PaymentStatusResponse>.Fail("支付记录不存在");
                }

                return ApiResponse<PaymentStatusResponse>.Success(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询支付状态异常: {Message}", ex.Message);
                return ApiResponse<PaymentStatusResponse>.Fail($"查询支付状态异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理微信支付回调
        /// </summary>
        /// <param name="notifyXml">回调XML</param>
        /// <returns>处理结果</returns>
        public async Task<bool> ProcessWechatPayNotifyAsync(string notifyXml)
        {
            try
            {
                // 处理微信支付回调
                var (success, orderNo, amount, transactionId) = await _wechatPayService.ProcessNotifyAsync(notifyXml);

                if (!success || string.IsNullOrEmpty(orderNo))
                {
                    return false;
                }

                // 查询订单信息
                var orderSql = @"
                    SELECT o.id AS Id, o.payment_status AS PaymentStatus
                    FROM orders o
                    WHERE o.order_no = @OrderNo";

                var order = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(orderSql, new { OrderNo = orderNo });

                if (order == null)
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信支付回调处理失败: 订单不存在, 订单号={0}", (object)orderNo);
                    return false;
                }

                if (order.PaymentStatus == 1)
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "微信支付回调处理: 订单已支付, 订单号={0}", (object)orderNo);
                    return true;
                }

                // 查询支付记录
                var paymentSql = @"
                    SELECT p.id AS Id, p.payment_status AS PaymentStatus
                    FROM payment_records p
                    WHERE p.order_id = @OrderId";

                var payment = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(paymentSql, new { OrderId = order.Id });

                if (payment == null)
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信支付回调处理失败: 支付记录不存在, 订单ID={0}", (object)order.Id);
                    return false;
                }

                // 开始事务
                if (_dbConnection.State != ConnectionState.Open)
                {
                    _dbConnection.Open();
                }

                using var transaction = _dbConnection.BeginTransaction();

                try
                {
                    // 更新支付记录
                    var updatePaymentSql = @"
                        UPDATE payment_records
                        SET payment_status = 2, payment_time = @PaymentTime, transaction_id = @TransactionId, update_time = @UpdateTime
                        WHERE id = @PaymentId";

                    await _dbConnection.ExecuteAsync(updatePaymentSql, new
                    {
                        PaymentId = payment.Id,
                        PaymentTime = DateTime.Now,
                        TransactionId = transactionId,
                        UpdateTime = DateTime.Now
                    }, transaction);

                    // 更新订单支付状态
                    var updateOrderSql = @"
                        UPDATE orders
                        SET payment_status = 1, payment_time = @PaymentTime, transaction_id = @TransactionId, update_time = @UpdateTime
                        WHERE id = @OrderId";

                    await _dbConnection.ExecuteAsync(updateOrderSql, new
                    {
                        OrderId = order.Id,
                        PaymentTime = DateTime.Now,
                        TransactionId = transactionId,
                        UpdateTime = DateTime.Now
                    }, transaction);

                    // 提交事务
                    transaction.Commit();

                    Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "微信支付回调处理成功: 订单号={0}, 交易号={1}", orderNo, transactionId);
                    return true;
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    transaction.Rollback();
                    _logger.LogError(ex, "微信支付回调处理异常: {0}", ex.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理微信支付回调异常: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 创建账户充值
        /// </summary>
        /// <param name="amount">充值金额</param>
        /// <param name="paymentMethod">支付方式</param>
        /// <param name="userId">用户ID</param>
        /// <param name="clientIp">客户端IP</param>
        /// <returns>充值响应</returns>
        public async Task<ApiResponse<RechargeResponse>> CreateRechargeAsync(
            decimal amount,
            int paymentMethod,
            int userId,
            string clientIp)
        {
            try
            {
                _logger.LogInformation("创建账户充值: 金额={Amount}, 支付方式={PaymentMethod}, 用户ID={UserId}",
                    amount, paymentMethod, userId);

                // 生成充值ID
                var rechargeId = Guid.NewGuid().ToString();

                // 创建充值记录
                var createRechargeSql = @"
                    INSERT INTO recharge_records (
                        id, user_id, amount, payment_method, status,
                        create_time, update_time
                    ) VALUES (
                        @Id, @UserId, @Amount, @PaymentMethod, 0,
                        @CreateTime, @UpdateTime
                    )";

                await _dbConnection.ExecuteAsync(createRechargeSql, new
                {
                    Id = rechargeId,
                    UserId = userId,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                });

                // 构建充值响应
                var response = new RechargeResponse
                {
                    RechargeId = rechargeId,
                    Amount = amount,
                    PaymentMethod = paymentMethod
                };

                // 根据支付方式处理
                switch (paymentMethod)
                {
                    case 1: // 微信支付
                        await ProcessWechatRechargeAsync(response, userId, clientIp);
                        break;
                    case 2: // 支付宝支付
                        // 暂不实现
                        response.PaymentUrl = $"/api/payment/alipay/recharge?rechargeId={rechargeId}";
                        break;
                    default:
                        return ApiResponse<RechargeResponse>.Fail("不支持的支付方式");
                }

                return ApiResponse<RechargeResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建账户充值异常: {Message}", ex.Message);
                return ApiResponse<RechargeResponse>.Fail($"创建账户充值异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理微信充值
        /// </summary>
        private async Task ProcessWechatRechargeAsync(RechargeResponse response, int userId, string clientIp)
        {
            try
            {
                // 查询用户OpenId
                var userSql = "SELECT open_id FROM users WHERE id = @UserId";
                var openId = await _dbConnection.ExecuteScalarAsync<string>(userSql, new { UserId = userId });

                if (string.IsNullOrEmpty(openId))
                {
                    throw new InvalidOperationException("用户OpenId不存在");
                }

                // 创建微信支付订单
                var (success, errorMessage, jsApiParameters, qrCodeUrl) = await _wechatPayService.CreateWechatPayOrderAsync(
                    $"R{response.RechargeId}",
                    response.Amount,
                    $"账户充值-{response.Amount}元",
                    openId,
                    clientIp);

                if (!success)
                {
                    throw new InvalidOperationException($"创建微信支付订单失败: {errorMessage}");
                }

                // 更新充值记录
                var updateSql = @"
                    UPDATE recharge_records
                    SET status = 1, update_time = @UpdateTime, js_api_params = @JsApiParams
                    WHERE id = @RechargeId";

                await _dbConnection.ExecuteAsync(updateSql, new
                {
                    RechargeId = response.RechargeId,
                    UpdateTime = DateTime.Now,
                    JsApiParams = System.Text.Json.JsonSerializer.Serialize(jsApiParameters)
                });

                // 设置支付链接和二维码URL
                response.PaymentUrl = $"/api/payment/wechat/recharge?rechargeId={response.RechargeId}";
                response.QrCodeUrl = qrCodeUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理微信充值异常: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 处理微信充值回调
        /// </summary>
        /// <param name="notifyXml">回调XML</param>
        /// <returns>处理结果</returns>
        public async Task<bool> ProcessWechatRechargeNotifyAsync(string notifyXml)
        {
            try
            {
                // 处理微信支付回调
                var (success, orderNo, amount, transactionId) = await _wechatPayService.ProcessNotifyAsync(notifyXml);

                if (!success || string.IsNullOrEmpty(orderNo))
                {
                    return false;
                }

                // 检查是否是充值订单
                if (!orderNo.StartsWith("R"))
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信充值回调处理失败: 非充值订单, 订单号={0}", (object)orderNo);
                    return false;
                }

                // 提取充值ID
                var rechargeId = orderNo.Substring(1);

                // 查询充值记录
                var rechargeSql = @"
                    SELECT r.id AS Id, r.user_id AS UserId, r.amount AS Amount, r.status AS Status
                    FROM recharge_records r
                    WHERE r.id = @RechargeId";

                var recharge = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(rechargeSql, new { RechargeId = rechargeId });

                if (recharge == null)
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogWarning(_logger, "微信充值回调处理失败: 充值记录不存在, 充值ID={0}", (object)rechargeId);
                    return false;
                }

                if (recharge.Status == 2)
                {
                    Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "微信充值回调处理: 充值已完成, 充值ID={0}", (object)rechargeId);
                    return true;
                }

                // 查询用户余额
                var userSql = "SELECT balance FROM users WHERE id = @UserId";
                var balance = await _dbConnection.ExecuteScalarAsync<decimal>(userSql, new { UserId = recharge.UserId });

                // 开始事务
                if (_dbConnection.State != ConnectionState.Open)
                {
                    _dbConnection.Open();
                }

                using var transaction = _dbConnection.BeginTransaction();

                try
                {
                    // 更新充值记录
                    var updateRechargeSql = @"
                        UPDATE recharge_records
                        SET status = 2, complete_time = @CompleteTime, transaction_id = @TransactionId, update_time = @UpdateTime
                        WHERE id = @RechargeId";

                    await _dbConnection.ExecuteAsync(updateRechargeSql, new
                    {
                        RechargeId = rechargeId,
                        CompleteTime = DateTime.Now,
                        TransactionId = transactionId,
                        UpdateTime = DateTime.Now
                    }, transaction);

                    // 更新用户余额
                    var updateBalanceSql = @"
                        UPDATE users
                        SET balance = balance + @Amount, update_time = @UpdateTime
                        WHERE id = @UserId";

                    await _dbConnection.ExecuteAsync(updateBalanceSql, new
                    {
                        UserId = recharge.UserId,
                        Amount = recharge.Amount,
                        UpdateTime = DateTime.Now
                    }, transaction);

                    // 添加余额变动记录
                    var addBalanceRecordSql = @"
                        INSERT INTO balance_records (
                            user_id, amount, balance, type, related_id,
                            description, create_time
                        ) VALUES (
                            @UserId, @Amount, @Balance, @Type, @RelatedId,
                            @Description, @CreateTime
                        )";

                    await _dbConnection.ExecuteAsync(addBalanceRecordSql, new
                    {
                        UserId = recharge.UserId,
                        Amount = recharge.Amount, // 正数表示收入
                        Balance = balance + recharge.Amount,
                        Type = 1, // 1表示充值
                        RelatedId = rechargeId,
                        Description = $"账户充值-{recharge.Amount}元",
                        CreateTime = DateTime.Now
                    }, transaction);

                    // 提交事务
                    transaction.Commit();

                    Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(_logger, "微信充值回调处理成功: 充值ID={0}, 金额={1}, 交易号={2}",
                        rechargeId, recharge.Amount, transactionId);
                    return true;
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    transaction.Rollback();
                    _logger.LogError(ex, "微信充值回调处理异常: {0}", ex.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Microsoft.Extensions.Logging.LoggerExtensions.LogError(_logger, ex, "处理微信充值回调异常: {0}", ex.Message);
                return false;
            }
        }
    }
}
