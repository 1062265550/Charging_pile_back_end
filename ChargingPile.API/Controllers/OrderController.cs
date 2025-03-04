using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Data;
using ChargingPile.API.Models.Entities;
using ChargingPile.API.Models.DTOs;
using ChargingPile.API.Services;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 订单管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly RateCalculationService _rateCalculationService;

        public OrderController(
            ApplicationDbContext context,
            ILogger<OrderController> logger,
            RateCalculationService rateCalculationService)
        {
            _context = context;
            _logger = logger;
            _rateCalculationService = rateCalculationService;
        }

        /// <summary>
        /// 创建新订单
        /// </summary>
        /// <param name="dto">订单创建信息</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     POST /api/Order
        ///     {
        ///         "userId": 1,
        ///         "stationId": "550e8400-e29b-41d4-a716-446655440000"
        ///     }
        /// </remarks>
        /// <response code="201">返回新创建的订单</response>
        /// <response code="400">请求数据无效</response>
        /// <response code="404">用户或充电站不存在</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDTO), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            _logger.LogInformation($"尝试创建订单: 用户ID={dto.UserId}, 充电站ID={dto.StationId}, 充电口ID={dto.PortId}");

            try
            {
                // 验证用户是否存在
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"用户ID {dto.UserId} 不存在");
                    return NotFound(new ProblemDetails
                    {
                        Title = "用户不存在",
                        Detail = $"ID为 {dto.UserId} 的用户不存在",
                        Status = 404
                    });
                }

                // 验证充电站是否存在
                var station = await _context.ChargingStations.FindAsync(dto.StationId);
                if (station == null)
                {
                    _logger.LogWarning($"充电站ID {dto.StationId} 不存在");
                    return NotFound(new ProblemDetails
                    {
                        Title = "充电站不存在",
                        Detail = $"ID为 {dto.StationId} 的充电站不存在",
                        Status = 404
                    });
                }

                // 验证充电口是否存在
                _logger.LogInformation($"查询充电口: ID={dto.PortId}");
                var port = await _context.ChargingPorts
                    .Include(p => p.ChargingPile)
                    .FirstOrDefaultAsync(p => p.Id == dto.PortId);

                if (port == null)
                {
                    _logger.LogWarning($"充电口ID {dto.PortId} 不存在");
                    return NotFound(new ProblemDetails
                    {
                        Title = "充电口不存在",
                        Detail = $"ID为 {dto.PortId} 的充电口不存在",
                        Status = 404
                    });
                }

                _logger.LogInformation($"找到充电口: ID={port.Id}, PileId={port.PileId}, Status={port.Status}");

                if (port.ChargingPile == null)
                {
                    _logger.LogWarning($"充电口 {dto.PortId} 的充电桩信息不存在");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "充电口信息不完整",
                        Detail = "无法获取充电口对应的充电桩信息",
                        Status = 400
                    });
                }

                _logger.LogInformation($"充电桩信息: ID={port.ChargingPile.Id}, StationId={port.ChargingPile.StationId}, PileNo={port.ChargingPile.PileNo}");

                // 验证充电口是否属于指定的充电站
                if (port.ChargingPile.StationId != dto.StationId)
                {
                    _logger.LogWarning($"充电口 {dto.PortId} 不属于充电站 {dto.StationId}");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "充电口不属于指定充电站",
                        Detail = "所选充电口不属于指定的充电站",
                        Status = 400
                    });
                }

                // 检查充电口是否可用
                if (port.Status != 0)
                {
                    _logger.LogWarning($"充电口 {dto.PortId} 当前不可用，状态为 {port.Status}");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "充电口不可用",
                        Detail = "所选充电口当前不可用，请选择其他充电口",
                        Status = 400
                    });
                }

                var currentTime = DateTime.Now;
                
                // 创建新订单
                var order = new Order
                {
                    OrderNo = Guid.NewGuid().ToString(),
                    UserId = dto.UserId,
                    StationId = dto.StationId,
                    PortId = dto.PortId,
                    StartTime = currentTime,
                    Status = 0, // 进行中
                    PaymentStatus = 0, // 未支付
                    PowerConsumption = 0, // 初始充电量为0
                    CreateTime = currentTime
                };

                _logger.LogInformation($"创建订单: OrderNo={order.OrderNo}");

                // 添加一个标志变量，防止重复执行
                bool orderCreated = false;

                // 保存订单
                _context.Orders.Add(order);
                _logger.LogInformation($"正在保存订单到数据库...");
                
                // 使用执行策略包装事务操作
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    // 使用事务确保数据一致性
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // 只有当订单未创建时才执行创建逻辑
                            if (!orderCreated)
                            {
                                // 先保存订单
                                await _context.SaveChangesAsync();
                                orderCreated = true;
                                _logger.LogInformation($"订单创建成功: ID={order.Id}, 订单号={order.OrderNo}");
                            }

                            // 更新充电口状态
                            _logger.LogInformation($"正在更新充电口状态: ID={port.Id}, 当前状态={port.Status}, 当前订单ID={port.CurrentOrderId}");
                            port.Status = 1; // 使用中
                            port.CurrentOrderId = order.Id;
                            port.TotalChargingTimes++;
                            
                            // 保存充电口状态更新
                            _logger.LogInformation($"正在保存充电口状态更新...");
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"充电口状态已更新: ID={port.Id}, 新状态={port.Status}, 新CurrentOrderId={port.CurrentOrderId}");
                            
                            // 提交事务
                            await transaction.CommitAsync();
                            _logger.LogInformation("事务已提交，订单创建和充电口状态更新成功");
                        }
                        catch (Exception ex)
                        {
                            // 回滚事务
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "创建订单或更新充电口状态时发生错误，事务已回滚: {Message}", ex.Message);
                            if (ex.InnerException != null)
                            {
                                _logger.LogError("内部异常: {Message}", ex.InnerException.Message);
                            }
                            throw; // 重新抛出异常，让外层catch捕获
                        }
                    }
                });

                // 重新加载订单以获取关联实体
                await _context.Entry(order).Reference(o => o.User).LoadAsync();
                await _context.Entry(order).Reference(o => o.ChargingStation).LoadAsync();
                await _context.Entry(order).Reference(o => o.ChargingPort).LoadAsync();

                // 构建返回DTO
                var orderDto = new OrderDTO
                {
                    Id = order.Id,
                    OrderNo = order.OrderNo,
                    UserId = order.UserId,
                    UserName = order.User?.Nickname ?? string.Empty,
                    StationId = order.StationId,
                    StationName = order.ChargingStation?.Name ?? string.Empty,
                    PortId = order.PortId,
                    PortNo = order.ChargingPort?.PortNo ?? string.Empty,
                    PileNo = order.ChargingPort?.ChargingPile?.PileNo ?? string.Empty,
                    StartTime = order.StartTime,
                    EndTime = order.EndTime,
                    Amount = order.Amount,
                    PowerConsumption = order.PowerConsumption,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus,
                    CreateTime = order.CreateTime,
                    UpdateTime = order.UpdateTime
                };

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单时发生错误: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("内部异常: {Message}", ex.InnerException.Message);
                }
                return StatusCode(500, new ProblemDetails
                {
                    Title = "创建订单失败",
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     GET /api/Order/5
        /// </remarks>
        /// <response code="200">返回订单详情</response>
        /// <response code="404">订单不存在</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDTO), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetOrder(int id)
        {
            _logger.LogInformation($"获取订单详情: ID={id}");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ChargingStation)
                .Include(o => o.ChargingPort)
                .Include("ChargingPort.ChargingPile")
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning($"订单ID {id} 不存在");
                return NotFound(new ProblemDetails
                {
                    Title = "订单不存在",
                    Detail = $"ID为 {id} 的订单不存在",
                    Status = 404
                });
            }

            var orderDto = new OrderDTO
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                UserName = order.User?.Nickname ?? string.Empty,
                StationId = order.StationId,
                StationName = order.ChargingStation?.Name ?? string.Empty,
                PortId = order.PortId,
                PortNo = order.ChargingPort?.PortNo ?? string.Empty,
                PileNo = order.ChargingPort?.ChargingPile?.PileNo ?? string.Empty,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                Amount = order.Amount,
                PowerConsumption = order.PowerConsumption,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                CreateTime = order.CreateTime,
                UpdateTime = order.UpdateTime
            };

            return Ok(orderDto);
        }

        /// <summary>
        /// 根据订单号获取订单详情
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     GET /api/Order/no/550e8400-e29b-41d4-a716-446655440000
        /// </remarks>
        /// <response code="200">返回订单详情</response>
        /// <response code="404">订单不存在</response>
        [HttpGet("no/{orderNo}")]
        [ProducesResponseType(typeof(OrderDTO), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetOrderByOrderNo(string orderNo)
        {
            _logger.LogInformation($"根据订单号获取订单详情: OrderNo={orderNo}");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ChargingStation)
                .FirstOrDefaultAsync(o => o.OrderNo == orderNo);

            if (order == null)
            {
                _logger.LogWarning($"订单号 {orderNo} 不存在");
                return NotFound(new ProblemDetails
                {
                    Title = "订单不存在",
                    Detail = $"订单号为 {orderNo} 的订单不存在",
                    Status = 404
                });
            }

            var orderDto = new OrderDTO
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                UserName = order.User?.Nickname ?? string.Empty,
                StationId = order.StationId,
                StationName = order.ChargingStation?.Name ?? string.Empty,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                Amount = order.Amount,
                PowerConsumption = order.PowerConsumption,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                CreateTime = order.CreateTime,
                UpdateTime = order.UpdateTime
            };

            return Ok(orderDto);
        }

        /// <summary>
        /// 获取用户的订单列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="status">订单状态（可选）</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     GET /api/Order/user/1?status=0
        /// </remarks>
        /// <response code="200">返回用户的订单列表</response>
        /// <response code="404">用户不存在</response>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<OrderDTO>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetUserOrders(int userId, [FromQuery] int? status = null)
        {
            _logger.LogInformation($"获取用户订单列表: 用户ID={userId}, 状态={status}");

            // 验证用户是否存在
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                _logger.LogWarning($"用户ID {userId} 不存在");
                return NotFound(new ProblemDetails
                {
                    Title = "用户不存在",
                    Detail = $"ID为 {userId} 的用户不存在",
                    Status = 404
                });
            }

            // 查询订单
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.ChargingStation)
                .Where(o => o.UserId == userId);

            // 如果指定了状态，则按状态筛选
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // 按创建时间降序排序
            var orders = await query.OrderByDescending(o => o.CreateTime).ToListAsync();

            // 转换为DTO
            var orderDtos = orders.Select(order => new OrderDTO
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                UserName = order.User?.Nickname ?? string.Empty,
                StationId = order.StationId,
                StationName = order.ChargingStation?.Name ?? string.Empty,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                Amount = order.Amount,
                PowerConsumption = order.PowerConsumption,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                CreateTime = order.CreateTime,
                UpdateTime = order.UpdateTime
            }).ToList();

            return Ok(orderDtos);
        }

        /// <summary>
        /// 获取订单当前状态（包括实时充电量和费用）
        /// </summary>
        [HttpGet("{id}/current-status")]
        [ProducesResponseType(typeof(OrderStatusDTO), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetOrderCurrentStatus(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ChargingPort)
                    .ThenInclude(p => p.ChargingPile)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "订单不存在",
                    Detail = $"ID为 {id} 的订单不存在",
                    Status = 404
                });
            }

            var (consumption, amount) = order.Status == 0 
                ? _rateCalculationService.CalculateCurrentChargingFee(order.StartTime, order.ChargingPort.ChargingPile.PowerRate)
                : _rateCalculationService.CalculateChargingFee(order.StartTime, order.EndTime ?? DateTime.Now, order.ChargingPort.ChargingPile.PowerRate);

            var status = new OrderStatusDTO
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                Status = order.Status,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                CurrentPowerConsumption = consumption,
                CurrentAmount = amount
            };

            return Ok(status);
        }

        /// <summary>
        /// 更新订单信息
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="dto">更新的订单信息</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     PUT /api/Order/5
        ///     {
        ///         "amount": 50.00,
        ///         "status": 1,
        ///         "paymentStatus": 1
        ///     }
        /// </remarks>
        /// <response code="200">返回更新后的订单</response>
        /// <response code="400">请求数据无效</response>
        /// <response code="404">订单不存在</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(OrderDTO), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDTO dto)
        {
            _logger.LogInformation($"更新订单: ID={id}");

            // 添加一个标志变量，防止重复更新充电口
            bool portUpdated = false;

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.ChargingStation)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning($"订单ID {id} 不存在");
                return NotFound(new ProblemDetails
                {
                    Title = "订单不存在",
                    Detail = $"ID为 {id} 的订单不存在",
                    Status = 404
                });
            }

            var currentStatus = order.Status;

            // 更新订单状态
            if (dto.Status.HasValue && dto.Status.Value != currentStatus)
            {
                order.Status = dto.Status.Value;
            }

            // 更新支付状态
            if (dto.PaymentStatus.HasValue && dto.PaymentStatus.Value != order.PaymentStatus)
            {
                order.PaymentStatus = dto.PaymentStatus.Value;
            }

            order.UpdateTime = DateTime.Now;

            try
            {
                // 使用执行策略包装事务操作
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    // 开始事务
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // 如果订单状态从"进行中"变为"已完成"或"已取消"，且充电口尚未更新
                        if (dto.Status.HasValue && currentStatus == 0 && (dto.Status.Value == 1 || dto.Status.Value == 2) && !portUpdated)
                        {
                            var endTime = DateTime.Now;
                            order.EndTime = endTime;

                            var port = await _context.ChargingPorts
                                .Include(p => p.ChargingPile)
                                .FirstOrDefaultAsync(p => p.Id == order.PortId);

                            if (port != null && port.ChargingPile != null)
                            {
                                // 使用费率计算服务计算最终费用
                                var (consumption, amount) = _rateCalculationService.CalculateChargingFee(
                                    order.StartTime,
                                    endTime,
                                    port.ChargingPile.PowerRate);

                                // 更新订单的充电量和金额
                                order.PowerConsumption = consumption;
                                order.Amount = amount;
                                _logger.LogInformation($"订单费用已自动计算: ID={order.Id}, 充电量={consumption:F2}度, 金额={amount:F2}元");

                                // 更新充电口状态
                                port.Status = 0; // 空闲
                                port.CurrentOrderId = null;
                                port.LastOrderId = order.Id;

                                // 更新充电口的统计数据
                                var duration = (int)(endTime - order.StartTime).TotalMinutes;
                                port.TotalChargingDuration += duration;
                                port.TotalPowerConsumption += consumption;
                                port.LastCheckTime = endTime;

                                _logger.LogInformation($"充电口统计数据已更新: ID={port.Id}, " +
                                    $"总充电时长={port.TotalChargingDuration}分钟, " +
                                    $"总耗电量={port.TotalPowerConsumption:F2}度");

                                portUpdated = true;
                            }
                        }

                        // 保存更改
                        await _context.SaveChangesAsync();
                        
                        // 提交事务
                        await transaction.CommitAsync();
                        _logger.LogInformation($"订单 {id} 更新成功，事务已提交");
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, $"更新订单 {id} 时发生错误，事务已回滚");
                        throw;
                    }
                });

                var orderDto = new OrderDTO
                {
                    Id = order.Id,
                    OrderNo = order.OrderNo,
                    UserId = order.UserId,
                    UserName = order.User?.Nickname ?? string.Empty,
                    StationId = order.StationId,
                    StationName = order.ChargingStation?.Name ?? string.Empty,
                    PortId = order.PortId,
                    PortNo = order.ChargingPort?.PortNo ?? string.Empty,
                    PileNo = order.ChargingPort?.ChargingPile?.PileNo ?? string.Empty,
                    StartTime = order.StartTime,
                    EndTime = order.EndTime,
                    Amount = order.Amount,
                    PowerConsumption = order.PowerConsumption,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus,
                    CreateTime = order.CreateTime,
                    UpdateTime = order.UpdateTime
                };

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新订单 {id} 时发生错误");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "更新订单失败",
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     DELETE /api/Order/5
        /// </remarks>
        /// <response code="204">订单删除成功</response>
        /// <response code="400">无法删除进行中的订单</response>
        /// <response code="404">订单不存在</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            _logger.LogInformation($"尝试删除订单: ID={id}");

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning($"订单ID {id} 不存在");
                return NotFound(new ProblemDetails
                {
                    Title = "订单不存在",
                    Detail = $"ID为 {id} 的订单不存在",
                    Status = 404
                });
            }

            // 不允许删除进行中的订单
            if (order.Status == 0)
            {
                _logger.LogWarning($"无法删除进行中的订单: ID={id}");
                return BadRequest(new ProblemDetails
                {
                    Title = "无法删除订单",
                    Detail = "无法删除进行中的订单，请先完成或取消订单",
                    Status = 400
                });
            }

            try
            {
                // 检查是否有充电口引用了该订单
                var referencingPorts = await _context.ChargingPorts
                    .Where(p => p.CurrentOrderId == id || p.LastOrderId == id)
                    .ToListAsync();
                
                if (referencingPorts.Any())
                {
                    _logger.LogInformation($"发现 {referencingPorts.Count} 个充电口引用了订单 {id}，正在解除引用");
                    
                    foreach (var port in referencingPorts)
                    {
                        if (port.CurrentOrderId == id)
                        {
                            _logger.LogInformation($"解除充电口 {port.Id} 的当前订单引用");
                            port.CurrentOrderId = null;
                        }
                        
                        if (port.LastOrderId == id)
                        {
                            _logger.LogInformation($"解除充电口 {port.Id} 的最后订单引用");
                            port.LastOrderId = null;
                        }
                    }
                    
                    // 先保存充电口的更改
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("所有充电口引用已解除");
                }
                
                // 使用执行策略包装事务操作
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    // 使用事务删除订单
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            _context.Orders.Remove(order);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            _logger.LogInformation($"订单 {id} 删除成功");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, $"删除订单 {id} 时发生错误，事务已回滚");
                            throw;
                        }
                    }
                });
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除订单 {id} 时发生错误");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "删除订单失败",
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }
    }

    public class OrderStatusDTO
    {
        public int Id { get; set; }
        public required string OrderNo { get; set; }
        public int Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal CurrentPowerConsumption { get; set; }
        public decimal CurrentAmount { get; set; }
    }
} 