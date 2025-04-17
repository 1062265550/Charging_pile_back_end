using ChargingPile.API.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 充电端口控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargingPortController : ControllerBase
    {
        private readonly ILogger<ChargingPortController> _logger;
        private readonly IDbConnection _dbConnection;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="dbConnection">数据库连接</param>
        public ChargingPortController(
            ILogger<ChargingPortController> logger,
            IDbConnection dbConnection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        /// <summary>
        /// 获取充电端口列表
        /// </summary>
        /// <param name="pileId">充电桩ID，可选</param>
        /// <returns>充电端口列表</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<PortListItem>>>> GetChargingPorts([FromQuery] string? pileId = null)
        {
            try
            {
                _logger.LogInformation("获取充电端口列表，充电桩ID: {PileId}", pileId ?? "全部");

                // 构建SQL查询
                var sql = @"
                    SELECT
                        p.id AS Id, p.pile_id AS PileId, pile.pile_no AS PileNo,
                        p.port_no AS PortNo, p.port_type AS PortType, p.status AS Status,
                        p.current_order_id AS CurrentOrderId, p.total_charging_times AS TotalChargingTimes,
                        p.total_charging_duration AS TotalChargingDuration,
                        p.total_power_consumption AS TotalPowerConsumption
                    FROM charging_ports p
                    JOIN charging_piles pile ON p.pile_id = pile.id";

                // 如果指定了充电桩ID，则添加过滤条件
                var parameters = new DynamicParameters();
                if (!string.IsNullOrEmpty(pileId))
                {
                    sql += " WHERE p.pile_id = @PileId";
                    parameters.Add("@PileId", pileId);
                }

                // 执行查询
                var ports = await _dbConnection.QueryAsync<PortListItem>(sql, parameters);

                return Ok(ApiResponse<List<PortListItem>>.Success(ports.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取充电端口列表失败");
                return BadRequest(ApiResponse<List<PortListItem>>.Fail($"获取充电端口列表失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取充电端口详情
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <returns>充电端口详情</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PortDetailResponse>>> GetPortDetail(string id)
        {
            try
            {
                _logger.LogInformation("获取充电端口详情，ID: {Id}", id);

                // 查询充电端口详情
                var sql = @"
                    SELECT
                        p.id AS Id,
                        p.pile_id AS PileId,
                        pile.pile_no AS PileNo,
                        p.port_no AS PortNo,
                        p.port_type AS PortType,
                        p.status AS Status,
                        p.fault_type AS FaultType,
                        p.is_disabled AS IsDisabled,
                        p.current_order_id AS CurrentOrderId,
                        p.last_order_id AS LastOrderId,
                        p.voltage AS Voltage,
                        p.current_ampere AS CurrentAmpere,
                        p.power AS Power,
                        p.temperature AS Temperature,
                        p.electricity AS Electricity,
                        p.total_charging_times AS TotalChargingTimes,
                        p.total_charging_duration AS TotalChargingDuration,
                        p.total_power_consumption AS TotalPowerConsumption,
                        p.last_check_time AS LastCheckTime,
                        p.update_time AS UpdateTime
                    FROM charging_ports p
                    JOIN charging_piles pile ON p.pile_id = pile.id
                    WHERE p.id = @Id";

                var port = await _dbConnection.QueryFirstOrDefaultAsync<PortDetailResponse>(sql, new { Id = id });

                if (port == null)
                {
                    return NotFound(ApiResponse<PortDetailResponse>.Fail($"未找到ID为{id}的充电端口", 404));
                }

                // 为所有可能为null的属性设置默认值
                port.PileId = port.PileId ?? "";
                port.PileNo = port.PileNo ?? "";
                port.PortNo = port.PortNo ?? "";
                port.PortType = port.PortType ?? 0;
                port.Status = port.Status ?? 0;
                port.FaultType = port.FaultType ?? 0;
                port.IsDisabled = port.IsDisabled ?? false;
                port.CurrentOrderId = port.CurrentOrderId ?? "";
                port.LastOrderId = port.LastOrderId ?? "";
                port.Voltage = port.Voltage ?? 0;
                port.CurrentAmpere = port.CurrentAmpere ?? 0;
                port.Power = port.Power ?? 0;
                port.Temperature = port.Temperature ?? 0;
                port.Electricity = port.Electricity ?? 0;
                port.TotalChargingTimes = port.TotalChargingTimes ?? 0;
                port.TotalChargingDuration = port.TotalChargingDuration ?? 0;
                port.TotalPowerConsumption = port.TotalPowerConsumption ?? 0;
                port.LastCheckTime = port.LastCheckTime ?? DateTime.MinValue;
                port.UpdateTime = port.UpdateTime ?? DateTime.MinValue;

                return Ok(ApiResponse<PortDetailResponse>.Success(port));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取充电端口详情失败，ID: {Id}", id);
                return BadRequest(ApiResponse<PortDetailResponse>.Fail($"获取充电端口详情失败: {ex.Message}"));
            }
        }


    }

    /// <summary>
    /// 充电端口列表项
    /// </summary>
    public class PortListItem
    {
        /// <summary>
        /// 充电端口唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属充电桩ID
        /// </summary>
        public string PileId { get; set; }

        /// <summary>
        /// 所属充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int? PortType { get; set; }

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 当前订单ID
        /// </summary>
        public string CurrentOrderId { get; set; }

        /// <summary>
        /// 总充电次数
        /// </summary>
        public int? TotalChargingTimes { get; set; }

        /// <summary>
        /// 总充电时长(秒)
        /// </summary>
        public int? TotalChargingDuration { get; set; }

        /// <summary>
        /// 总耗电量(kWh)
        /// </summary>
        public decimal? TotalPowerConsumption { get; set; }
    }

    /// <summary>
    /// 充电端口详情响应
    /// </summary>
    public class PortDetailResponse
    {
        /// <summary>
        /// 充电端口唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属充电桩ID
        /// </summary>
        public string PileId { get; set; }

        /// <summary>
        /// 所属充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int? PortType { get; set; }

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 故障类型：0-无故障，1-保险丝熔断，2-继电器粘连
        /// </summary>
        public short? FaultType { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// 当前订单ID
        /// </summary>
        public string CurrentOrderId { get; set; }

        /// <summary>
        /// 最后一个订单ID
        /// </summary>
        public string LastOrderId { get; set; }

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

        /// <summary>
        /// 端口温度
        /// </summary>
        public decimal? Temperature { get; set; }

        /// <summary>
        /// 当前电量(kWh)
        /// </summary>
        public decimal? Electricity { get; set; }

        /// <summary>
        /// 总充电次数
        /// </summary>
        public int? TotalChargingTimes { get; set; }

        /// <summary>
        /// 总充电时长(秒)
        /// </summary>
        public int? TotalChargingDuration { get; set; }

        /// <summary>
        /// 总耗电量(kWh)
        /// </summary>
        public decimal? TotalPowerConsumption { get; set; }

        /// <summary>
        /// 最后检查时间
        /// </summary>
        public DateTime? LastCheckTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 充电端口状态响应
    /// </summary>
    public class PortStatusResponse
    {
        /// <summary>
        /// 充电端口唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int? Status { get; set; }

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

        /// <summary>
        /// 端口温度
        /// </summary>
        public decimal? Temperature { get; set; }

        /// <summary>
        /// 当前电量(kWh)
        /// </summary>
        public decimal? Electricity { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
