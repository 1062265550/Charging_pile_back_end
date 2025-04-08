using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Models;
using ChargingPileAdmin.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 订单服务实现
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ChargingPile> _pileRepository;
        private readonly IRepository<ChargingPort> _portRepository;

        public OrderService(
            IRepository<Order> orderRepository,
            IRepository<User> userRepository,
            IRepository<ChargingPile> pileRepository,
            IRepository<ChargingPort> portRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _pileRepository = pileRepository;
            _portRepository = portRepository;
        }

        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <returns>订单列表</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAsync(
                o => true,
                o => o.User,
                o => o.ChargingPile,
                o => o.ChargingPort);

            // 手动排序结果
            var sortedOrders = orders.OrderByDescending(x => x.StartTime);
            
            return sortedOrders.Select(MapToDto);
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <returns>订单详情</returns>
        public async Task<OrderDto?> GetOrderByIdAsync(string id)
        {
            var order = await _orderRepository.GetByIdAsync(
                id,
                o => o.User,
                o => o.ChargingPile,
                o => o.ChargingPort);

            if (order == null)
            {
                return null;
            }

            return MapToDto(order);
        }

        /// <summary>
        /// 获取订单分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="userId">用户ID，可选</param>
        /// <param name="pileId">充电桩ID，可选</param>
        /// <param name="status">订单状态，可选</param>
        /// <param name="startDate">开始日期，可选</param>
        /// <param name="endDate">结束日期，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>订单分页列表</returns>
        public async Task<PagedResponseDto<OrderDto>> GetOrdersPagedAsync(
            int pageNumber,
            int pageSize,
            int? userId = null,
            string? pileId = null,
            int? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? keyword = null)
        {
            // 构建查询条件
            Expression<Func<Order, bool>> filter = o => true;
            
            // 使用 AND 组合条件而不是替换条件
            List<Expression<Func<Order, bool>>> conditions = new List<Expression<Func<Order, bool>>>();
            
            // 添加所有筛选条件
            if (userId.HasValue)
            {
                conditions.Add(o => o.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(pileId))
            {
                conditions.Add(o => o.PileId == pileId);
            }

            if (status.HasValue)
            {
                conditions.Add(o => o.Status == status.Value);
            }

            if (startDate.HasValue)
            {
                conditions.Add(o => o.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                conditions.Add(o => o.StartTime <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add(o => o.OrderNo.Contains(keyword));
            }
            
            // 组合所有条件
            if (conditions.Count > 0)
            {
                var parameter = Expression.Parameter(typeof(Order), "o");
                Expression combinedExpression = Expression.Constant(true);
                
                foreach (var condition in conditions)
                {
                    // 获取方法体，并将参数替换为当前参数
                    var body = condition.Body;
                    var visitor = new ParameterReplacer(condition.Parameters[0], parameter);
                    body = visitor.Visit(body);
                    
                    // 使用 AND 操作组合条件
                    combinedExpression = Expression.AndAlso(combinedExpression, body);
                }
                
                filter = Expression.Lambda<Func<Order, bool>>(combinedExpression, parameter);
            }

            // 获取分页数据
            var (orders, totalCount, totalPages) = await _orderRepository.GetPagedAsync(
                filter,
                pageNumber,
                pageSize,
                orderBy: q => q.OrderByDescending(x => x.StartTime),
                o => o.User,
                o => o.ChargingPile,
                o => o.ChargingPort
            );

            // 构建返回结果
            var pagedResponse = new PagedResponseDto<OrderDto>
            {
                Items = orders.Select(MapToDto).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return pagedResponse;
        }
        
        /// <summary>
        /// 参数替换访问器，用于组合条件表达式
        /// </summary>
        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }

        /// <summary>
        /// 获取用户订单列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>订单列表</returns>
        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetAsync(
                o => o.UserId == userId,
                o => o.User,
                o => o.ChargingPile,
                o => o.ChargingPort);

            // 手动排序结果
            var sortedOrders = orders.OrderByDescending(x => x.StartTime);
            
            return sortedOrders.Select(MapToDto);
        }

        /// <summary>
        /// 获取充电桩订单列表
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>订单列表</returns>
        public async Task<IEnumerable<OrderDto>> GetOrdersByPileIdAsync(string pileId)
        {
            var orders = await _orderRepository.GetAsync(
                o => o.PileId == pileId,
                o => o.User,
                o => o.ChargingPile,
                o => o.ChargingPort);

            // 手动排序结果
            var sortedOrders = orders.OrderByDescending(x => x.StartTime);
            
            return sortedOrders.Select(MapToDto);
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="dto">创建订单请求</param>
        /// <returns>创建的订单ID</returns>
        public async Task<string> CreateOrderAsync(CreateOrderDto dto)
        {
            // 验证用户是否存在
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new Exception($"用户ID '{dto.UserId}' 不存在");
            }

            // 验证充电桩是否存在
            var pile = await _pileRepository.GetByIdAsync(dto.PileId);
            if (pile == null)
            {
                throw new Exception($"充电桩ID '{dto.PileId}' 不存在");
            }

            // 验证充电端口是否存在
            var port = await _portRepository.GetByIdAsync(dto.PortId);
            if (port == null)
            {
                throw new Exception($"充电端口ID '{dto.PortId}' 不存在");
            }

            // 验证充电端口是否可用
            if (port.Status != 1) // 1表示空闲
            {
                throw new Exception($"充电端口 '{port.PortNo}' 不可用，当前状态: {port.Status}");
            }

            // 创建订单
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                OrderNo = GenerateOrderNo(),
                UserId = dto.UserId,
                PileId = dto.PileId,
                PortId = dto.PortId,
                Status = 1, // 充电中
                BillingMode = dto.BillingMode,
                StartTime = DateTime.Now,
                ChargingMode = dto.ChargingMode,
                UpdateTime = DateTime.Now
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // 更新充电端口状态
            port.Status = 2; // 使用中
            port.CurrentOrderId = order.Id;
            port.UpdateTime = DateTime.Now;

            _portRepository.Update(port);
            await _portRepository.SaveChangesAsync();

            return order.Id;
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="dto">更新订单状态请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new Exception($"订单ID '{id}' 不存在");
            }

            order.Status = dto.Status;
            if (dto.StopReason.HasValue)
            {
                order.StopReason = dto.StopReason.Value;
            }
            order.UpdateTime = DateTime.Now;

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="dto">订单支付请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> PayOrderAsync(string id, OrderPaymentDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new Exception($"订单ID '{id}' 不存在");
            }

            // 检查订单状态是否为待支付
            if (order.Status != 0) // 0表示待支付
            {
                throw new Exception($"订单状态不是待支付，当前状态: {order.Status}");
            }

            // 更新订单支付信息
            order.PaymentStatus = 1; // 已支付
            order.PaymentMethod = dto.PaymentMethod;
            order.PaymentTime = DateTime.Now;
            order.TransactionId = dto.TransactionId;
            order.UpdateTime = DateTime.Now;

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> CancelOrderAsync(string id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new Exception($"订单ID '{id}' 不存在");
            }

            // 检查订单状态是否可以取消
            if (order.Status != 0 && order.Status != 1) // 0表示待支付，1表示充电中
            {
                throw new Exception($"订单状态不可取消，当前状态: {order.Status}");
            }

            // 更新订单状态
            order.Status = 3; // 已取消
            order.EndTime = DateTime.Now;
            order.UpdateTime = DateTime.Now;

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            // 释放充电端口
            if (order.Status == 1) // 如果是充电中状态，需要释放充电端口
            {
                var port = await _portRepository.GetByIdAsync(order.PortId);
                if (port != null)
                {
                    port.Status = 1; // 空闲
                    port.CurrentOrderId = null;
                    port.UpdateTime = DateTime.Now;

                    _portRepository.Update(port);
                    await _portRepository.SaveChangesAsync();
                }
            }

            return true;
        }

        /// <summary>
        /// 完成订单
        /// </summary>
        /// <param name="id">订单ID</param>
        /// <param name="powerConsumption">充电电量(kWh)</param>
        /// <param name="chargingTime">充电时长(秒)</param>
        /// <returns>是否成功</returns>
        public async Task<bool> CompleteOrderAsync(string id, decimal powerConsumption, int chargingTime)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                throw new Exception($"订单ID '{id}' 不存在");
            }

            // 检查订单状态是否为充电中
            if (order.Status != 1) // 1表示充电中
            {
                throw new Exception($"订单状态不是充电中，当前状态: {order.Status}");
            }

            // 计算应收金额（根据实际业务逻辑计算）
            decimal amount = powerConsumption * 0.5m; // 示例：每度电0.5元
            decimal serviceFee = powerConsumption * 0.1m; // 示例：服务费为电费的10%
            decimal totalAmount = amount + serviceFee;

            // 更新订单信息
            order.Status = 2; // 已完成
            order.EndTime = DateTime.Now;
            order.PowerConsumption = powerConsumption;
            order.ChargingTime = chargingTime;
            order.Amount = amount;
            order.ServiceFee = serviceFee;
            order.TotalAmount = totalAmount;
            order.UpdateTime = DateTime.Now;

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            // 释放充电端口
            var port = await _portRepository.GetByIdAsync(order.PortId);
            if (port != null)
            {
                port.Status = 1; // 空闲
                port.CurrentOrderId = null;
                port.LastOrderId = order.Id;
                port.TotalChargingTimes = (port.TotalChargingTimes ?? 0) + 1;
                port.TotalChargingDuration = (port.TotalChargingDuration ?? 0) + chargingTime;
                port.TotalPowerConsumption = (port.TotalPowerConsumption ?? 0) + powerConsumption;
                port.UpdateTime = DateTime.Now;

                _portRepository.Update(port);
                await _portRepository.SaveChangesAsync();
            }

            return true;
        }

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        /// <param name="startDate">开始日期，可选</param>
        /// <param name="endDate">结束日期，可选</param>
        /// <returns>订单统计数据</returns>
        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // 构建查询条件
            Expression<Func<Order, bool>> filter = o => true;
            List<Expression<Func<Order, bool>>> conditions = new List<Expression<Func<Order, bool>>>();

            if (startDate.HasValue)
            {
                conditions.Add(o => o.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                conditions.Add(o => o.StartTime <= endDate.Value);
            }
            
            // 组合所有条件
            if (conditions.Count > 0)
            {
                var parameter = Expression.Parameter(typeof(Order), "o");
                Expression combinedExpression = Expression.Constant(true);
                
                foreach (var condition in conditions)
                {
                    var body = condition.Body;
                    var visitor = new ParameterReplacer(condition.Parameters[0], parameter);
                    body = visitor.Visit(body);
                    combinedExpression = Expression.AndAlso(combinedExpression, body);
                }
                
                filter = Expression.Lambda<Func<Order, bool>>(combinedExpression, parameter);
            }

            // 获取订单数据
            var orders = await _orderRepository.GetAsync(filter);

            // 计算统计数据
            // 状态值定义：0-待支付，1-已支付(等待充电)，2-充电中，3-已完成，4-已取消
            var statistics = new OrderStatisticsDto
            {
                TotalOrders = orders.Count(),
                TotalPowerConsumption = orders.Sum(o => o.PowerConsumption ?? 0),
                TotalChargingTime = orders.Sum(o => o.ChargingTime ?? 0),
                TotalAmount = orders.Sum(o => o.TotalAmount ?? 0),
                PendingPaymentOrders = orders.Count(o => o.Status == 0), // 待支付
                PendingChargingOrders = orders.Count(o => o.Status == 1), // 已支付(等待充电)
                ChargingOrders = orders.Count(o => o.Status == 2), // 充电中
                CompletedOrders = orders.Count(o => o.Status == 3), // 已完成
                CancelledOrders = orders.Count(o => o.Status == 4), // 已取消
                AbnormalOrders = 0 // 暂无异常状态
            };

            return statistics;
        }

        /// <summary>
        /// 将订单实体映射为DTO
        /// </summary>
        /// <param name="order">订单实体</param>
        /// <returns>订单DTO</returns>
        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                UserNickname = order.User?.Nickname,
                PileId = order.PileId,
                PileNo = order.ChargingPile?.PileNo,
                PortId = order.PortId,
                PortNo = order.ChargingPort?.PortNo,
                Status = order.Status,
                BillingMode = order.BillingMode,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                ChargingTime = order.ChargingTime,
                PowerConsumption = order.PowerConsumption,
                Amount = order.Amount,
                ServiceFee = order.ServiceFee,
                TotalAmount = order.TotalAmount,
                PaymentStatus = order.PaymentStatus,
                PaymentTime = order.PaymentTime,
                PaymentMethod = order.PaymentMethod,
                TransactionId = order.TransactionId,
                Power = order.Power,
                ChargingMode = order.ChargingMode,
                StopReason = order.StopReason,
                SharpElectricity = order.SharpElectricity,
                SharpAmount = order.SharpAmount,
                PeakElectricity = order.PeakElectricity,
                PeakAmount = order.PeakAmount,
                FlatElectricity = order.FlatElectricity,
                FlatAmount = order.FlatAmount,
                ValleyElectricity = order.ValleyElectricity,
                ValleyAmount = order.ValleyAmount,
                DeepValleyElectricity = order.DeepValleyElectricity,
                DeepValleyAmount = order.DeepValleyAmount,
                Remark = order.Remark,
                UpdateTime = order.UpdateTime
            };
        }

        /// <summary>
        /// 生成订单编号
        /// </summary>
        /// <returns>订单编号</returns>
        private string GenerateOrderNo()
        {
            return $"O{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
}
