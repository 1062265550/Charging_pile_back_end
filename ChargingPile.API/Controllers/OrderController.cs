using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using ChargingPile.API.Models.Common;
using Dapper;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 订单控制器
    /// </summary>
    [ApiController]
    [Route("api")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IDbConnection _dbConnection;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="dbConnection">数据库连接</param>
        public OrderController(
            ILogger<OrderController> logger,
            IDbConnection dbConnection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        /// <summary>
        /// 创建充电订单
        /// </summary>
        /// <param name="request">创建订单请求</param>
        /// <returns>订单信息</returns>
        [HttpPost("order/create")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CreateOrderResponse>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("创建充电订单请求: PileId={PileId}, PortId={PortId}, ChargingMode={ChargingMode}",
                    request.PileId, request.PortId, request.ChargingMode);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<CreateOrderResponse>.Fail("未授权的访问", 401));
                }

                // 验证充电桩和充电端口是否存在
                var pileExists = await CheckPileExistsAsync(request.PileId);
                if (!pileExists)
                {
                    return BadRequest(ApiResponse<CreateOrderResponse>.Fail("充电桩不存在"));
                }

                var portExists = await CheckPortExistsAsync(request.PortId);
                if (!portExists)
                {
                    return BadRequest(ApiResponse<CreateOrderResponse>.Fail("充电端口不存在"));
                }

                // 验证充电端口是否可用
                var portStatus = await GetPortStatusAsync(request.PortId);
                if (portStatus != 1) // 1表示空闲
                {
                    string statusMessage = portStatus switch
                    {
                        0 => "离线",
                        2 => "使用中",
                        3 => "故障",
                        _ => "状态异常"
                    };
                    return BadRequest(ApiResponse<CreateOrderResponse>.Fail($"充电端口不可用，当前状态: {statusMessage}"));
                }

                // 使用事务确保数据一致性
                if (_dbConnection.State != System.Data.ConnectionState.Open)
                    _dbConnection.Open();

                using (var transaction = _dbConnection.BeginTransaction())
                {
                    try
                    {
                        // 生成订单号
                        string orderNo = GenerateOrderNo();
                        string orderId = Guid.NewGuid().ToString();

                        // 根据充电模式设置订单金额
                        decimal orderAmount = 0;
                        if (request.ChargingMode == 2) // 按金额充电
                        {
                            orderAmount = request.ChargingParameter;
                        }
                        else if (request.ChargingMode == 3) // 按时间充电
                        {
                            // 这里可以根据时间计算预估金额，或者使用最小金额
                            orderAmount = Math.Max(0.01m, request.ChargingParameter * 0.01m); // 假设每分钟0.01元
                        }
                        else if (request.ChargingMode == 4) // 按电量充电
                        {
                            // 这里可以根据电量计算预估金额，或者使用最小金额
                            orderAmount = Math.Max(0.01m, request.ChargingParameter * 0.5m); // 假设每度电0.5元
                        }
                        else
                        {
                            // 其他充电模式，设置一个最小金额
                            orderAmount = 0.01m; // 最小1分钱
                        }

                        _logger.LogInformation("订单金额设置: 充电模式={0}, 充电参数={1}, 订单金额={2}",
                            request.ChargingMode, request.ChargingParameter, orderAmount);

                        // 创建订单
                        var sql = @"
                            INSERT INTO orders (
                                id, order_no, user_id, pile_id, port_id, amount, total_amount,
                                start_time, power_consumption, charging_time, power,
                                status, payment_status, charging_mode, update_time, payment_method
                            ) VALUES (
                                @Id, @OrderNo, @UserId, @PileId, @PortId, @Amount, @TotalAmount,
                                @StartTime, 0, 0, 0,
                                0, 0, @ChargingMode, @UpdateTime, @PaymentMethod
                            )";

                        var orderResult = await _dbConnection.ExecuteAsync(sql, new
                        {
                            Id = orderId,
                            OrderNo = orderNo,
                            UserId = userId,
                            PileId = request.PileId,
                            PortId = request.PortId,
                            Amount = orderAmount,
                            TotalAmount = orderAmount, // 设置总金额等于订单金额
                            StartTime = DateTime.Now,
                            ChargingMode = request.ChargingMode,
                            UpdateTime = DateTime.Now,
                            PaymentMethod = request.PaymentMethod
                        }, transaction);

                        _logger.LogInformation(string.Format("订单创建结果: {0}, 订单ID: {1}", orderResult > 0 ? "成功" : "失败", orderId));

                        // 更新充电端口状态
                        var portUpdateResult = await UpdatePortStatusAsync(request.PortId, 2, orderId, transaction); // 2表示使用中
                        if (!portUpdateResult)
                        {
                            Console.WriteLine(string.Format("充电端口状态更新失败: PortId={0}", request.PortId));
                            throw new Exception("充电端口状态更新失败");
                        }

                        // 更新充电桩状态
                        var pileUpdateResult = await UpdatePileStatusAsync(request.PileId, 2, transaction); // 2表示使用中
                        if (!pileUpdateResult)
                        {
                            Console.WriteLine(string.Format("充电桩状态更新失败: PileId={0}", request.PileId));
                            throw new Exception("充电桩状态更新失败");
                        }

                        // 获取充电桩和充电站信息
                        var pileInfo = await GetPileInfoAsync(request.PileId, transaction);

                        // 提交事务
                        transaction.Commit();

                        // 构建响应
                        var response = new CreateOrderResponse
                        {
                            OrderId = orderId,
                            OrderNo = orderNo,
                            Status = 0, // 0表示创建
                            PaymentStatus = 0, // 0表示未支付
                            PaymentMethod = request.PaymentMethod,
                            Amount = orderAmount,
                            StartTime = DateTime.Now,
                            PaymentUrl = GeneratePaymentUrl(orderId, request.PaymentMethod)
                        };

                        return Ok(ApiResponse<CreateOrderResponse>.Success(response));
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        _logger.LogError(ex, "创建充电订单失败");
                        return BadRequest(ApiResponse<CreateOrderResponse>.Fail($"创建充电订单失败: {ex.Message}"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建充电订单失败");
                return StatusCode(500, ApiResponse<CreateOrderResponse>.Fail($"创建充电订单失败: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <returns>订单详情</returns>
        [HttpGet("order/{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<OrderDetailResponse>>> GetOrderById(string id)
        {
            try
            {
                _logger.LogInformation("获取订单详情，ID: {Id}", id);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<OrderDetailResponse>.Fail("未授权的访问", 401));
                }

                // 查询订单详情
                var sql = @"
                    SELECT o.id AS Id, o.order_no AS OrderNo, o.user_id AS UserId, o.pile_id AS PileId,
                           p.pile_no AS PileNo, o.port_id AS PortId, port.port_no AS PortNo,
                           s.name AS StationName, o.amount AS Amount, o.start_time AS StartTime,
                           o.end_time AS EndTime, o.power_consumption AS PowerConsumption,
                           o.charging_time AS ChargingTime, o.power AS Power, o.status AS Status,
                           o.payment_status AS PaymentStatus, o.payment_method AS PaymentMethod,
                           o.payment_time AS PaymentTime, o.service_fee AS ServiceFee,
                           o.total_amount AS TotalAmount, o.transaction_id AS TransactionId,
                           o.charging_mode AS ChargingMode, o.stop_reason AS StopReason,
                           o.billing_mode AS BillingMode, o.sharp_electricity AS SharpElectricity,
                           o.sharp_amount AS SharpAmount, o.peak_electricity AS PeakElectricity,
                           o.peak_amount AS PeakAmount, o.flat_electricity AS FlatElectricity,
                           o.flat_amount AS FlatAmount, o.valley_electricity AS ValleyElectricity,
                           o.valley_amount AS ValleyAmount, o.deep_valley_electricity AS DeepValleyElectricity,
                           o.deep_valley_amount AS DeepValleyAmount, o.remark AS Remark,
                           o.update_time AS UpdateTime
                    FROM orders o
                    JOIN charging_piles p ON o.pile_id = p.id
                    JOIN charging_ports port ON o.port_id = port.id
                    JOIN charging_stations s ON p.station_id = s.id
                    WHERE o.id = @Id AND o.user_id = @UserId";

                var order = await _dbConnection.QueryFirstOrDefaultAsync<OrderDetailResponse>(sql, new { Id = id, UserId = userId });

                if (order == null)
                {
                    return NotFound(ApiResponse<OrderDetailResponse>.Fail($"未找到ID为{id}的订单", 404));
                }

                return Ok(ApiResponse<OrderDetailResponse>.Success(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败，ID: {Id}", id);
                return BadRequest(ApiResponse<OrderDetailResponse>.Fail($"获取订单详情失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取订单实时状态
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <returns>订单实时状态</returns>
        [HttpGet("order/{id}/status")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<OrderStatusResponse>>> GetOrderStatus(string id)
        {
            try
            {
                _logger.LogInformation("获取订单实时状态，ID: {Id}", id);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<OrderStatusResponse>.Fail("未授权的访问", 401));
                }

                // 查询订单基本信息
                var orderSql = @"
                    SELECT o.id AS Id, o.status AS Status, o.payment_status AS PaymentStatus,
                           o.start_time AS StartTime, o.end_time AS EndTime, o.power_consumption AS PowerConsumption,
                           o.charging_time AS ChargingTime, o.power AS Power, o.amount AS Amount,
                           p.voltage AS Voltage, p.current_ampere AS CurrentAmpere
                    FROM orders o
                    LEFT JOIN charging_ports p ON o.port_id = p.id
                    WHERE o.id = @Id AND o.user_id = @UserId";

                var order = await _dbConnection.QueryFirstOrDefaultAsync<OrderStatusResponse>(orderSql, new { Id = id, UserId = userId });

                if (order == null)
                {
                    return NotFound(ApiResponse<OrderStatusResponse>.Fail($"未找到ID为{id}的订单", 404));
                }

                // 设置当前时间
                order.CurrentTime = DateTime.Now;

                // 如果订单状态为进行中，查询充电端口实时状态并更新数据
                if (order.Status == 1) // 1表示进行中
                {
                    var portSql = @"
                        SELECT p.voltage AS Voltage, p.current_ampere AS CurrentAmpere, p.power AS Power, p.electricity AS PowerConsumption
                        FROM charging_ports p
                        JOIN orders o ON p.id = o.port_id
                        WHERE o.id = @Id";

                    var portStatus = await _dbConnection.QueryFirstOrDefaultAsync(portSql, new { Id = id });

                    if (portStatus != null)
                    {
                        // 计算已充电时间
                        var chargingTime = (int)(DateTime.Now - order.StartTime).TotalSeconds;
                        order.ChargingTime = chargingTime;

                        // 更新实时数据
                        if (portStatus.PowerConsumption > 0)
                        {
                            order.PowerConsumption = portStatus.PowerConsumption;
                            // 计算当前金额
                            order.Amount = CalculateAmount(portStatus.PowerConsumption);
                        }

                        order.Voltage = portStatus.Voltage;
                        order.CurrentAmpere = portStatus.CurrentAmpere;
                        order.Power = portStatus.Power;
                    }
                }

                return Ok(ApiResponse<OrderStatusResponse>.Success(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单实时状态失败，ID: {Id}", id);
                return BadRequest(ApiResponse<OrderStatusResponse>.Fail($"获取订单实时状态失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 停止充电
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <returns>操作结果</returns>
        [HttpPost("order/{id}/stop")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<StopChargingResponse>>> StopCharging(string id)
        {
            try
            {
                _logger.LogInformation("停止充电请求，ID: {Id}", id);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<StopChargingResponse>.Fail("未授权的访问", 401));
                }

                // 使用事务确保数据一致性
                if (_dbConnection.State != System.Data.ConnectionState.Open)
                    _dbConnection.Open();

                using (var transaction = _dbConnection.BeginTransaction())
                {
                    try
                    {
                        // 查询订单信息
                        var orderSql = @"
                            SELECT o.id AS Id, o.status AS Status, o.pile_id AS PileId, o.port_id AS PortId,
                                   o.power_consumption AS PowerConsumption, o.amount AS Amount, o.payment_status AS PaymentStatus,
                                   p.electricity AS Electricity, p.power AS Power, p.total_charging_times AS TotalChargingTimes
                            FROM orders o
                            JOIN charging_ports p ON o.port_id = p.id
                            WHERE o.id = @Id AND o.user_id = @UserId";

                        var orderInfo = await _dbConnection.QueryFirstOrDefaultAsync(orderSql, new { Id = id, UserId = userId }, transaction);

                        if (orderInfo == null)
                        {
                            return NotFound(ApiResponse<StopChargingResponse>.Fail($"未找到ID为{id}的订单", 404));
                        }

                        // 检查订单状态是否为进行中
                        if (orderInfo.Status != 1) // 1表示进行中
                        {
                            return BadRequest(ApiResponse<StopChargingResponse>.Fail("只能停止进行中的充电订单"));
                        }

                        // 计算充电时间和金额
                        var endTime = DateTime.Now;
                        var chargingTime = await CalculateChargingTimeAsync(id, transaction);

                        // 使用订单表中已有的值，如果存在的话
                        decimal powerConsumption = 0;
                        decimal amount = 0;
                        int paymentStatus = 0;

                        // 如果订单表中已有值，则使用订单表中的值
                        if (orderInfo.PowerConsumption > 0)
                        {
                            powerConsumption = Convert.ToDecimal(orderInfo.PowerConsumption);
                            amount = Convert.ToDecimal(orderInfo.Amount);
                            paymentStatus = orderInfo.PaymentStatus;
                        }
                        else
                        {
                            // 如果订单表中没有值，则使用充电端口表中的值计算
                            powerConsumption = Convert.ToDecimal(orderInfo.Electricity);
                            amount = CalculateAmount(powerConsumption);
                        }

                        // 更新订单状态
                        var updateOrderSql = @"
                            UPDATE orders
                            SET status = 2, -- 2表示已完成
                                end_time = @EndTime,
                                charging_time = @ChargingTime,
                                power_consumption = @PowerConsumption,
                                amount = @Amount,
                                total_amount = @TotalAmount, -- 更新总金额
                                stop_reason = 3, -- 3表示手动停止
                                update_time = @UpdateTime
                            WHERE id = @Id";

                        var orderUpdateResult = await _dbConnection.ExecuteAsync(updateOrderSql, new
                        {
                            Id = id,
                            EndTime = endTime,
                            ChargingTime = chargingTime,
                            PowerConsumption = powerConsumption,
                            Amount = amount,
                            TotalAmount = amount, // 设置总金额等于订单金额
                            UpdateTime = DateTime.Now
                        }, transaction);

                        _logger.LogInformation(string.Format("订单状态更新结果: {0}, 影响行数: {1}",
                            orderUpdateResult > 0 ? "成功" : "失败", orderUpdateResult));

                        // 更新充电端口完整信息
                        var portUpdateResult = await UpdatePortCompleteInfoAsync(
                            orderInfo.PortId,
                            1, // 1表示空闲
                            id, // 当前订单ID作为最后一个订单ID
                            chargingTime,
                            powerConsumption,
                            transaction
                        );

                        if (!portUpdateResult)
                        {
                            Console.WriteLine(string.Format("充电端口状态更新失败: PortId={0}", orderInfo.PortId));
                            throw new Exception("充电端口状态更新失败");
                        }

                        // 更新充电桩状态
                        var pileUpdateResult = await UpdatePileStatusAsync(orderInfo.PileId, 1, transaction); // 1表示空闲

                        if (!pileUpdateResult)
                        {
                            Console.WriteLine(string.Format("充电桩状态更新失败: PileId={0}", orderInfo.PileId));
                            throw new Exception("充电桩状态更新失败");
                        }

                        // 提交事务
                        transaction.Commit();

                        // 构建响应
                        var response = new StopChargingResponse
                        {
                            Id = id,
                            Status = 2, // 2表示已完成
                            EndTime = endTime,
                            ChargingTime = chargingTime,
                            PowerConsumption = powerConsumption,
                            Amount = amount,
                            PaymentStatus = paymentStatus, // 保留原有的支付状态
                            PaymentUrl = GeneratePaymentUrl(id, 1) // 1表示微信支付，这里只是示例
                        };

                        return Ok(ApiResponse<StopChargingResponse>.Success(response));
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        transaction.Rollback();
                        _logger.LogError(ex, "停止充电失败，ID: {Id}", id);
                        return BadRequest(ApiResponse<StopChargingResponse>.Fail($"停止充电失败: {ex.Message}"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止充电失败，ID: {Id}", id);
                return StatusCode(500, ApiResponse<StopChargingResponse>.Fail($"停止充电失败: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="pageNum">页码，默认1</param>
        /// <param name="pageSize">每页数量，默认10</param>
        /// <param name="status">订单状态，可选</param>
        /// <param name="startDate">开始日期，可选，格式：YYYY-MM-DD</param>
        /// <param name="endDate">结束日期，可选，格式：YYYY-MM-DD</param>
        /// <returns>订单列表</returns>
        [HttpGet("orders")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagedResponse<OrderListItem>>>> GetOrders(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? status = null,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null)
        {
            try
            {
                _logger.LogInformation("获取订单列表，页码: {PageNum}, 每页数量: {PageSize}, 状态: {Status}",
                    pageNum, pageSize, status);

                // 获取当前用户ID
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(ApiResponse<PagedResponse<OrderListItem>>.Fail("未授权的访问", 401));
                }

                // 构建查询条件
                var whereClause = "WHERE o.user_id = @UserId";
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                if (status.HasValue)
                {
                    whereClause += " AND o.status = @Status";
                    parameters.Add("@Status", status.Value);
                }

                if (!string.IsNullOrEmpty(startDate))
                {
                    whereClause += " AND o.start_time >= @StartDate";
                    parameters.Add("@StartDate", DateTime.Parse(startDate));
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    whereClause += " AND o.start_time <= @EndDate";
                    parameters.Add("@EndDate", DateTime.Parse(endDate).AddDays(1).AddSeconds(-1)); // 包含当天
                }

                // 查询总数
                var countSql = $"SELECT COUNT(*) FROM orders o {whereClause}";
                var total = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);

                // 计算分页参数
                var offset = (pageNum - 1) * pageSize;
                parameters.Add("@Offset", offset);
                parameters.Add("@PageSize", pageSize);

                // 查询订单列表
                var sql = $@"
                    SELECT o.id AS Id, o.order_no AS OrderNo, s.name AS StationName, p.pile_no AS PileNo, port.port_no AS PortNo,
                           o.amount AS Amount, o.start_time AS StartTime, o.end_time AS EndTime, o.power_consumption AS PowerConsumption,
                           o.charging_time AS ChargingTime, o.status AS Status, o.payment_status AS PaymentStatus
                    FROM orders o
                    JOIN charging_piles p ON o.pile_id = p.id
                    JOIN charging_ports port ON o.port_id = port.id
                    JOIN charging_stations s ON p.station_id = s.id
                    {whereClause}
                    ORDER BY o.start_time DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                var orders = await _dbConnection.QueryAsync<OrderListItem>(sql, parameters);

                // 构建分页响应
                var response = new PagedResponse<OrderListItem>
                {
                    Total = total,
                    List = orders.ToList(),
                    PageNum = pageNum,
                    PageSize = pageSize,
                    Pages = (int)Math.Ceiling((double)total / pageSize)
                };

                return Ok(ApiResponse<PagedResponse<OrderListItem>>.Success(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return BadRequest(ApiResponse<PagedResponse<OrderListItem>>.Fail($"获取订单列表失败: {ex.Message}"));
            }
        }

        #region 辅助方法

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetCurrentUserId()
        {
            // 从 JWT 中获取用户ID
            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("用户未登录或会话已过期");
            }
            _logger.LogInformation("当前登录用户ID: {UserId}", userId);
            return userId;
        }

        /// <summary>
        /// 检查充电桩是否存在
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>是否存在</returns>
        private async Task<bool> CheckPileExistsAsync(string pileId)
        {
            var sql = "SELECT COUNT(1) FROM charging_piles WHERE id = @PileId";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { PileId = pileId });
            return count > 0;
        }

        /// <summary>
        /// 检查充电端口是否存在
        /// </summary>
        /// <param name="portId">充电端口ID</param>
        /// <returns>是否存在</returns>
        private async Task<bool> CheckPortExistsAsync(string portId)
        {
            var sql = "SELECT COUNT(1) FROM charging_ports WHERE id = @PortId";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { PortId = portId });
            return count > 0;
        }

        /// <summary>
        /// 获取充电端口状态
        /// </summary>
        /// <param name="portId">充电端口ID</param>
        /// <returns>端口状态</returns>
        private async Task<int> GetPortStatusAsync(string portId)
        {
            var sql = "SELECT status FROM charging_ports WHERE id = @PortId";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { PortId = portId });
        }

        /// <summary>
        /// 更新充电端口状态
        /// </summary>
        /// <param name="portId">充电端口ID</param>
        /// <param name="status">新状态</param>
        /// <param name="orderId">订单ID，可为空</param>
        /// <param name="transaction">事务对象，可为空</param>
        /// <returns>是否成功</returns>
        private async Task<bool> UpdatePortStatusAsync(string portId, int status, string orderId, IDbTransaction transaction = null)
        {
            var sql = "UPDATE charging_ports SET status = @Status, current_order_id = @OrderId, update_time = @UpdateTime WHERE id = @PortId";
            var result = await _dbConnection.ExecuteAsync(sql, new
            {
                PortId = portId,
                Status = status,
                OrderId = orderId,
                UpdateTime = DateTime.Now
            }, transaction);
            return result > 0;
        }

        /// <summary>
        /// 更新充电端口完整信息（停止充电时使用）
        /// </summary>
        /// <param name="portId">充电端口ID</param>
        /// <param name="status">新状态</param>
        /// <param name="lastOrderId">最后一个订单ID</param>
        /// <param name="chargingTime">充电时长(秒)</param>
        /// <param name="powerConsumption">耗电量(kWh)</param>
        /// <param name="transaction">事务对象，可为空</param>
        /// <returns>是否成功</returns>
        private async Task<bool> UpdatePortCompleteInfoAsync(string portId, int status, string lastOrderId, int chargingTime, decimal powerConsumption, IDbTransaction transaction = null)
        {
            var sql = @"
                UPDATE charging_ports
                SET status = @Status,
                    current_order_id = NULL,
                    last_order_id = @LastOrderId,
                    total_charging_times = total_charging_times + 1,
                    total_charging_duration = total_charging_duration + @ChargingTime,
                    total_power_consumption = total_power_consumption + @PowerConsumption,
                    update_time = @UpdateTime
                WHERE id = @PortId";

            var result = await _dbConnection.ExecuteAsync(sql, new
            {
                PortId = portId,
                Status = status,
                LastOrderId = lastOrderId,
                ChargingTime = chargingTime,
                PowerConsumption = powerConsumption,
                UpdateTime = DateTime.Now
            }, transaction);

            _logger.LogInformation(string.Format("更新充电端口完整信息: PortId={0}, Status={1}, LastOrderId={2}, 影响行数={3}",
                portId, status, lastOrderId, result));

            return result > 0;
        }

        /// <summary>
        /// 更新充电桩状态
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <param name="status">新状态</param>
        /// <param name="transaction">事务对象，可为空</param>
        /// <returns>是否成功</returns>
        private async Task<bool> UpdatePileStatusAsync(string pileId, int status, IDbTransaction transaction = null)
        {
            var sql = "UPDATE charging_piles SET status = @Status, update_time = @UpdateTime WHERE id = @PileId";
            var result = await _dbConnection.ExecuteAsync(sql, new
            {
                PileId = pileId,
                Status = status,
                UpdateTime = DateTime.Now
            }, transaction);

            _logger.LogInformation(string.Format("更新充电桩状态: PileId={0}, Status={1}, 影响行数={2}",
                pileId, status, result));

            return result > 0;
        }

        /// <summary>
        /// 获取充电桩信息
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <param name="transaction">事务对象，可为空</param>
        /// <returns>充电桩信息</returns>
        private async Task<dynamic> GetPileInfoAsync(string pileId, IDbTransaction transaction = null)
        {
            var sql = @"
                SELECT p.id, p.pile_no, p.pile_type, p.power_rate, s.id AS station_id, s.name AS station_name
                FROM charging_piles p
                JOIN charging_stations s ON p.station_id = s.id
                WHERE p.id = @PileId";
            return await _dbConnection.QueryFirstOrDefaultAsync(sql, new { PileId = pileId }, transaction);
        }

        /// <summary>
        /// 生成订单号
        /// </summary>
        /// <returns>订单号</returns>
        private string GenerateOrderNo()
        {
            // 生成格式：YYYYMMDDHHMMSS + 4位随机数
            return DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999).ToString();
        }

        /// <summary>
        /// 生成支付URL
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentMethod">支付方式</param>
        /// <returns>支付URL</returns>
        private string GeneratePaymentUrl(string orderId, int paymentMethod)
        {
            // 实际应调用支付系统生成支付URL
            // 这里返回模拟数据
            string paymentType = paymentMethod switch
            {
                1 => "wechat",
                2 => "alipay",
                3 => "balance",
                _ => "wechat"
            };
            return $"/api/payment/pay?orderId={orderId}&type={paymentType}";
        }

        /// <summary>
        /// 计算充电时间
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="transaction">事务对象，可为空</param>
        /// <returns>充电时间（秒）</returns>
        private async Task<int> CalculateChargingTimeAsync(string orderId, IDbTransaction transaction = null)
        {
            var sql = "SELECT DATEDIFF(SECOND, start_time, GETDATE()) FROM orders WHERE id = @OrderId";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { OrderId = orderId }, transaction);
        }

        /// <summary>
        /// 计算金额
        /// </summary>
        /// <param name="powerConsumption">耗电量</param>
        /// <returns>金额</returns>
        private decimal CalculateAmount(decimal powerConsumption)
        {
            // 实际应该根据计费规则计算
            // 这里简化为固定费率
            const decimal rate = 0.8m; // 0.8元/kWh
            return Math.Round(powerConsumption * rate, 2);
        }

        #endregion
    }

    #region 请求和响应模型

    /// <summary>
    /// 创建订单请求
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// 充电桩ID
        /// </summary>
        [Required(ErrorMessage = "充电桩ID不能为空")]
        public string PileId { get; set; }

        /// <summary>
        /// 充电端口ID
        /// </summary>
        [Required(ErrorMessage = "充电端口ID不能为空")]
        public string PortId { get; set; }

        /// <summary>
        /// 充电模式：1-充满自停，2-按金额，3-按时间，4-按电量，5-其他
        /// </summary>
        [Required(ErrorMessage = "充电模式不能为空")]
        [Range(1, 5, ErrorMessage = "充电模式必须在1-5之间")]
        public int ChargingMode { get; set; }

        /// <summary>
        /// 充电参数：金额(元)/时间(分钟)/电量(kWh)
        /// </summary>
        public decimal ChargingParameter { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-余额
        /// </summary>
        [Required(ErrorMessage = "支付方式不能为空")]
        [Range(1, 3, ErrorMessage = "支付方式必须在1-3之间")]
        public int PaymentMethod { get; set; }
    }

    /// <summary>
    /// 创建订单响应
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单状态：0-创建，1-进行中，2-已完成，3-已取消，4-异常
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 支付状态：0-未支付，1-已支付，2-已退款
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public int PaymentMethod { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 支付链接
        /// </summary>
        public string PaymentUrl { get; set; }
    }

    /// <summary>
    /// 订单详情响应
    /// </summary>
    public class OrderDetailResponse
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 充电桩ID
        /// </summary>
        public string PileId { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 充电端口ID
        /// </summary>
        public string PortId { get; set; }

        /// <summary>
        /// 充电端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 充电站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 订单金额(元)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 耗电量(kWh)
        /// </summary>
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// 充电时长(秒)
        /// </summary>
        public int ChargingTime { get; set; }

        /// <summary>
        /// 充电功率(kW)
        /// </summary>
        public decimal Power { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public int? PaymentMethod { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        [JsonPropertyName("paymentTime")]
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 服务费(元)
        /// </summary>
        public decimal? ServiceFee { get; set; }

        /// <summary>
        /// 总金额(元)
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 交易流水号
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// 充电模式
        /// </summary>
        public int ChargingMode { get; set; }

        /// <summary>
        /// 停止原因
        /// </summary>
        public short? StopReason { get; set; }

        /// <summary>
        /// 计费模式
        /// </summary>
        public short? BillingMode { get; set; }

        /// <summary>
        /// 尖峰时段电量(kWh)
        /// </summary>
        public decimal? SharpElectricity { get; set; }

        /// <summary>
        /// 尖峰时段金额(元)
        /// </summary>
        public decimal? SharpAmount { get; set; }

        /// <summary>
        /// 峰时段电量(kWh)
        /// </summary>
        public decimal? PeakElectricity { get; set; }

        /// <summary>
        /// 峰时段金额(元)
        /// </summary>
        public decimal? PeakAmount { get; set; }

        /// <summary>
        /// 平时段电量(kWh)
        /// </summary>
        public decimal? FlatElectricity { get; set; }

        /// <summary>
        /// 平时段金额(元)
        /// </summary>
        public decimal? FlatAmount { get; set; }

        /// <summary>
        /// 谷时段电量(kWh)
        /// </summary>
        public decimal? ValleyElectricity { get; set; }

        /// <summary>
        /// 谷时段金额(元)
        /// </summary>
        public decimal? ValleyAmount { get; set; }

        /// <summary>
        /// 深谷时段电量(kWh)
        /// </summary>
        public decimal? DeepValleyElectricity { get; set; }

        /// <summary>
        /// 深谷时段金额(元)
        /// </summary>
        public decimal? DeepValleyAmount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 订单状态响应
    /// </summary>
    public class OrderStatusResponse
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime CurrentTime { get; set; }

        /// <summary>
        /// 已充电时间(秒)
        /// </summary>
        public int ChargingTime { get; set; }

        /// <summary>
        /// 已充电量(kWh)
        /// </summary>
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// 当前金额(元)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 当前电压(V)
        /// </summary>
        public decimal? Voltage { get; set; }

        /// <summary>
        /// 当前电流(A)
        /// </summary>
        public decimal? CurrentAmpere { get; set; }

        /// <summary>
        /// 当前功率(kW)
        /// </summary>
        public decimal? Power { get; set; }
    }

    /// <summary>
    /// 停止充电响应
    /// </summary>
    public class StopChargingResponse
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 充电时长(秒)
        /// </summary>
        public int ChargingTime { get; set; }

        /// <summary>
        /// 耗电量(kWh)
        /// </summary>
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// 金额(元)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// 支付链接
        /// </summary>
        public string PaymentUrl { get; set; }
    }

    /// <summary>
    /// 订单列表项
    /// </summary>
    public class OrderListItem
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 充电站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 充电端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 金额(元)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 耗电量(kWh)
        /// </summary>
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// 充电时长(秒)
        /// </summary>
        public int ChargingTime { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public int PaymentStatus { get; set; }
    }

    #endregion
}
