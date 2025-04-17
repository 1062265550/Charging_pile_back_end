using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChargingPileAdmin.Controllers
{
    /// <summary>
    /// 订单管理 - 提供充电桩订单相关的增删改查操作
    /// </summary>
    /// <remarks>
    /// 本控制器负责充电订单的所有管理功能，包括：
    /// - 订单信息的查询（按ID、按用户、按充电桩、分页查询等）
    /// - 订单状态管理（创建、支付、取消、完成等）
    /// - 订单统计数据分析
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// 获取所有订单信息
        /// </summary>
        /// <remarks>
        /// 返回系统中所有订单的完整列表，包含订单基本信息、用户信息、充电桩信息等数据
        /// 
        /// 使用场景：
        /// - 管理后台展示所有订单记录
        /// - 数据分析和统计
        /// 
        /// 注意：当订单数量较多时，建议使用分页接口获取数据
        /// </remarks>
        /// <returns>订单列表</returns>
        /// <response code="200">成功返回订单列表</response>
        /// <response code="500">服务器错误</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取订单列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定ID的订单详细信息
        /// </summary>
        /// <remarks>
        /// 根据订单ID获取单个订单的详细信息，包括：
        /// - 基本信息（订单号、创建时间等）
        /// - 用户信息（用户ID、用户名等）
        /// - 充电桩信息（充电桩ID、名称等）
        /// - 充电信息（电量、时长、费用等）
        /// - 支付信息（支付状态、支付时间等）
        /// 
        /// 使用场景：
        /// - 查看订单详情页面
        /// - 订单管理和跟踪
        /// </remarks>
        /// <param name="id">订单ID（唯一标识符）</param>
        /// <returns>订单详情</returns>
        /// <response code="200">成功返回订单详情</response>
        /// <response code="400">ID参数错误</response>
        /// <response code="404">未找到指定ID的订单</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("订单ID不能为空");
                }
                
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound($"订单ID '{id}' 不存在");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取订单详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 分页获取订单列表（支持多条件筛选）
        /// </summary>
        /// <remarks>
        /// 提供带分页功能的订单查询接口，支持多种筛选条件：
        /// - 按用户ID筛选
        /// - 按充电桩ID筛选
        /// - 按订单状态筛选（未支付/已支付/充电中/已完成/已取消等）
        /// - 按日期范围筛选
        /// - 按关键字搜索（订单号等）
        /// 
        /// 使用场景：
        /// - 管理后台的订单列表页
        /// - 用户个人中心的订单列表
        /// - 带筛选条件的数据查询
        /// 
        /// 订单状态说明：
        /// - 0：待支付
        /// - 1：已支付（等待充电）
        /// - 2：充电中
        /// - 3：已完成
        /// - 4：已取消
        /// </remarks>
        /// <param name="pageNumber">页码，从1开始</param>
        /// <param name="pageSize">每页记录数，默认10条</param>
        /// <param name="userId">用户ID，可选，用于筛选特定用户的订单</param>
        /// <param name="pileId">充电桩ID，可选，用于筛选特定充电桩的订单</param>
        /// <param name="status">订单状态，可选，用于筛选特定状态的订单</param>
        /// <param name="startDate">开始日期，可选，用于筛选特定日期范围的订单</param>
        /// <param name="endDate">结束日期，可选，用于筛选特定日期范围的订单</param>
        /// <param name="keyword">关键字，可选，用于搜索订单号等</param>
        /// <returns>订单分页数据</returns>
        /// <response code="200">成功返回分页数据</response>
        /// <response code="400">分页参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResponseDto<OrderDto>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetOrdersPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? userId = null,
            [FromQuery] string? pileId = null,
            [FromQuery] int? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? keyword = null)
        {
            try
            {
                if (pageNumber < 1)
                {
                    return BadRequest("页码必须大于等于1");
                }

                if (pageSize < 1)
                {
                    return BadRequest("每页记录数必须大于等于1");
                }
                
                var orders = await _orderService.GetOrdersPagedAsync(
                    pageNumber, pageSize, userId, pileId, status, startDate, endDate, keyword);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取订单分页列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定用户的所有订单
        /// </summary>
        /// <remarks>
        /// 根据用户ID查询该用户的所有订单记录，按创建时间倒序排列
        /// 
        /// 使用场景：
        /// - 用户个人中心查看历史订单
        /// - 客服查询用户订单历史
        /// - 数据分析和用户行为分析
        /// </remarks>
        /// <param name="userId">用户ID（唯一标识符）</param>
        /// <returns>订单列表</returns>
        /// <response code="200">成功返回订单列表</response>
        /// <response code="400">用户ID参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("byUser/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetOrdersByUserId(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest("用户ID必须大于0");
                }
                
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户订单列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定充电桩的所有订单
        /// </summary>
        /// <remarks>
        /// 根据充电桩ID查询该充电桩的所有订单记录，按创建时间倒序排列
        /// 
        /// 使用场景：
        /// - 充电桩管理查看历史订单记录
        /// - 充电桩使用情况分析
        /// - 故障排查和运维管理
        /// </remarks>
        /// <param name="pileId">充电桩ID（唯一标识符）</param>
        /// <returns>订单列表</returns>
        /// <response code="200">成功返回订单列表</response>
        /// <response code="400">充电桩ID参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("byPile/{pileId}")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetOrdersByPileId(string pileId)
        {
            try
            {
                if (string.IsNullOrEmpty(pileId))
                {
                    return BadRequest("充电桩ID不能为空");
                }
                
                var orders = await _orderService.GetOrdersByPileIdAsync(pileId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取充电桩订单列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建新的充电订单
        /// </summary>
        /// <remarks>
        /// 创建一个新的充电订单，需要提供用户ID、充电桩ID、充电端口ID等信息
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "userId": 1001,
        ///   "pileId": "PILE001",
        ///   "portId": "PORT001",
        ///   "chargingMode": 1,
        ///   "chargingAmount": 20.5,
        ///   "estimatedTime": 60
        /// }
        /// ```
        /// 
        /// 充电模式说明：
        /// - 0：按金额充电
        /// - 1：按电量充电
        /// - 2：按时间充电
        /// - 3：充满为止
        /// 
        /// 使用场景：
        /// - 用户在APP中创建新的充电订单
        /// - 管理员代客户创建充电订单
        /// </remarks>
        /// <param name="dto">创建订单的数据传输对象</param>
        /// <returns>创建的订单ID</returns>
        /// <response code="201">订单创建成功，返回新创建的订单ID</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="500">服务器错误</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var orderId = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { id = orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"创建订单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <remarks>
        /// 根据订单ID更新订单的状态，可用于各类订单状态流转场景
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "status": 2,
        ///   "remark": "用户已开始充电"
        /// }
        /// ```
        /// 
        /// 订单状态说明：
        /// - 0：待支付
        /// - 1：已支付（等待充电）
        /// - 2：充电中
        /// - 3：已完成
        /// - 4：已取消
        /// 
        /// 使用场景：
        /// - 管理员更新订单状态
        /// - 系统自动更新订单状态
        /// - 充电桩上报状态变更时同步更新订单状态
        /// </remarks>
        /// <param name="id">订单ID</param>
        /// <param name="dto">订单状态更新的数据传输对象</param>
        /// <returns>更新结果</returns>
        /// <response code="204">订单状态更新成功</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="404">未找到指定ID的订单</response>
        /// <response code="500">服务器错误</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("订单ID不能为空");
                }
                
                var result = await _orderService.UpdateOrderStatusAsync(id, dto);
                if (!result)
                {
                    return NotFound($"订单ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"更新订单状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <remarks>
        /// 处理订单的支付请求，更新订单状态为已支付，并记录支付信息
        /// 
        /// 请求示例:
        /// ```json
        /// {
        ///   "paymentMethod": 1,
        ///   "paymentAmount": 50.00,
        ///   "transactionId": "PAY123456789",
        ///   "remark": "微信支付"
        /// }
        /// ```
        /// 
        /// 支付方式说明：
        /// - 0：余额支付
        /// - 1：微信支付
        /// - 2：支付宝支付
        /// - 3：银行卡支付
        /// - 4：其他方式
        /// 
        /// 使用场景：
        /// - 用户在APP中完成订单支付
        /// - 管理员代客户完成订单支付
        /// - 线下支付后管理员手动更新支付状态
        /// </remarks>
        /// <param name="id">订单ID</param>
        /// <param name="dto">订单支付的数据传输对象</param>
        /// <returns>支付结果</returns>
        /// <response code="204">订单支付成功</response>
        /// <response code="400">请求数据验证失败</response>
        /// <response code="404">未找到指定ID的订单</response>
        /// <response code="500">服务器错误</response>
        [HttpPost("{id}/pay")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> PayOrder(string id, [FromBody] OrderPaymentDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("订单ID不能为空");
                }
                
                var result = await _orderService.PayOrderAsync(id, dto);
                if (!result)
                {
                    return NotFound($"订单ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"订单支付失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <remarks>
        /// 取消指定的订单，更新订单状态为已取消
        /// 
        /// 订单取消说明：
        /// - 未支付订单可直接取消
        /// - 已支付但未开始充电的订单取消后，将自动退款
        /// - 充电中的订单不允许取消，需要先停止充电
        /// - 已完成的订单不允许取消
        /// 
        /// 使用场景：
        /// - 用户在APP中取消订单
        /// - 管理员代客户取消订单
        /// - 系统自动取消长时间未支付的订单
        /// </remarks>
        /// <param name="id">订单ID</param>
        /// <returns>取消结果</returns>
        /// <response code="204">订单取消成功</response>
        /// <response code="400">请求参数错误</response>
        /// <response code="404">未找到指定ID的订单</response>
        /// <response code="500">服务器错误</response>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CancelOrder(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("订单ID不能为空");
                }
                
                var result = await _orderService.CancelOrderAsync(id);
                if (!result)
                {
                    return NotFound($"订单ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"取消订单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 完成订单
        /// </summary>
        /// <remarks>
        /// 将订单标记为已完成状态，并记录实际充电电量和充电时长
        /// 
        /// 订单完成触发场景：
        /// - 用户主动结束充电
        /// - 充电达到预设条件自动结束
        /// - 充电桩上报充电完成事件
        /// 
        /// 完成订单后的处理：
        /// - 计算最终费用
        /// - 生成电子发票（如需）
        /// - 释放充电端口资源
        /// - 推送完成通知给用户
        /// </remarks>
        /// <param name="id">订单ID</param>
        /// <param name="powerConsumption">充电电量(kWh)，实际消耗的电量</param>
        /// <param name="chargingTime">充电时长(秒)，实际充电的时间</param>
        /// <returns>完成结果</returns>
        /// <response code="204">订单完成成功</response>
        /// <response code="400">请求参数错误</response>
        /// <response code="404">未找到指定ID的订单</response>
        /// <response code="500">服务器错误</response>
        [HttpPost("{id}/complete")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CompleteOrder(
            string id, 
            [FromQuery] decimal powerConsumption, 
            [FromQuery] int chargingTime)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("订单ID不能为空");
                }
                
                if (powerConsumption < 0)
                {
                    return BadRequest("充电电量不能为负数");
                }
                
                if (chargingTime < 0)
                {
                    return BadRequest("充电时长不能为负数");
                }
                
                var result = await _orderService.CompleteOrderAsync(id, powerConsumption, chargingTime);
                if (!result)
                {
                    return NotFound($"订单ID '{id}' 不存在");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"完成订单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取订单统计数据
        /// </summary>
        /// <remarks>
        /// 获取系统中订单的统计分析数据，支持按时间范围筛选
        /// 
        /// 返回的统计数据包括：
        /// - 订单总数
        /// - 订单总金额
        /// - 总充电电量
        /// - 总充电时长
        /// - 各状态订单数量分布
        /// - 按日期的订单趋势
        /// 
        /// 使用场景：
        /// - 管理后台的数据看板和报表
        /// - 经营分析和决策支持
        /// - 系统运营状况监控
        /// </remarks>
        /// <param name="startDate">开始日期，可选，默认为最近30天</param>
        /// <param name="endDate">结束日期，可选，默认为当前日期</param>
        /// <returns>订单统计数据</returns>
        /// <response code="200">成功返回统计数据</response>
        /// <response code="400">日期参数错误</response>
        /// <response code="500">服务器错误</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(OrderStatisticsDto), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetOrderStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest("开始日期不能大于结束日期");
                }
                
                var statistics = await _orderService.GetOrderStatisticsAsync(startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取订单统计数据失败: {ex.Message}");
            }
        }
    }
}
