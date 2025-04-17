using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Services;
using System.ComponentModel.DataAnnotations;
using System;
using System.Data;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 充电桩控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargingPileController : ControllerBase
    {
        private readonly ILogger<ChargingPileController> _logger;
        private readonly ChargingPileService _chargingPileService;
        private readonly IDbConnection _dbConnection;

        public ChargingPileController(
            ILogger<ChargingPileController> logger,
            ChargingPileService chargingPileService,
            IDbConnection dbConnection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chargingPileService = chargingPileService ?? throw new ArgumentNullException(nameof(chargingPileService));
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        /// <summary>
        /// 启动充电
        /// </summary>
        /// <param name="deviceAddress">设备地址</param>
        /// <param name="request">启动充电请求</param>
        /// <returns>操作结果</returns>
        /// <response code="200">启动充电命令已发送</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">设备未连接</response>
        [HttpPost("{deviceAddress}/start")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StartCharging(
            [FromRoute][Required] string deviceAddress,
            [FromBody] StartChargingRequest request)
        {
            try
            {
                _logger.LogInformation("收到启动充电请求：设备 {DeviceAddress}, 订单 {OrderId}", deviceAddress, request.OrderId);

                var result = await _chargingPileService.StartChargingAsync(
                    deviceAddress,          // pileId
                    request.Port,           // 端口号
                    request.OrderId,        // 订单号
                    request.StartMode,      // 启动方式
                    request.CardId,         // 卡号
                    request.ChargingMode,   // 充电方式
                    request.ChargingParam,  // 充电参数
                    request.AvailableAmount // 可用金额
                );

                if (result.Success)
                {
                    return Ok(new { message = "启动充电命令已发送" });
                }
                else
                {
                    // 根据错误信息判断返回状态码
                    if (result.Message.Contains("未找到") || result.Message.Contains("未连接") || result.Message.Contains("离线"))
                    {
                        return NotFound(new { error = result.Message });
                    }
                    else if (result.Message.Contains("已存在") || result.Message.Contains("正在充电中") || result.Message.Contains("请使用新的订单号"))
                    {
                        return BadRequest(new { error = result.Message });
                    }
                    else
                    {
                        return StatusCode(500, new { error = result.Message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "启动充电请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动充电操作失败");
                return StatusCode(500, new { error = "启动充电操作失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 停止充电
        /// </summary>
        /// <param name="deviceAddress">设备地址</param>
        /// <param name="request">停止充电请求</param>
        /// <returns>操作结果</returns>
        /// <response code="200">停止充电命令已发送</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">设备未连接</response>
        [HttpPost("{deviceAddress}/stop")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StopCharging(
            [FromRoute][Required] string deviceAddress,
            [FromBody] StopChargingRequest request)
        {
            try
            {
                _logger.LogInformation("收到停止充电请求：设备 {DeviceAddress}, 订单 {OrderId}", deviceAddress, request.OrderId);

                var result = await _chargingPileService.StopChargingAsync(
                    deviceAddress,   // pileId
                    request.Port,    // 端口号
                    request.OrderId  // 订单号
                );

                if (result.Success)
                {
                    return Ok(new { message = "停止充电命令已发送" });
                }
                else if (result.Message.Contains("未连接"))
                {
                    return NotFound(new { error = result.Message });
                }
                else
                {
                    return BadRequest(new { error = result.Message });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "停止充电请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理停止充电请求时发生异常");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 查询充电状态
        /// </summary>
        /// <param name="deviceAddress">设备地址</param>
        /// <param name="port">充电端口号</param>
        /// <returns>操作结果</returns>
        /// <response code="200">查询命令已发送</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">设备未连接</response>
        [HttpGet("{deviceAddress}/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetChargingStatus(
            [FromRoute][Required] string deviceAddress,
            [FromQuery] byte port = 1)
        {
            try
            {
                _logger.LogDebug("收到查询充电状态请求：设备 {DeviceAddress}, 端口 {Port}", deviceAddress, port);

                var result = await _chargingPileService.QueryChargingStatusAsync(deviceAddress, port);

                if (result.Success)
                {
                    return Ok(new { message = "查询充电状态命令已发送" });
                }
                else
                {
                    // 根据错误信息判断返回状态码
                    if (result.Message.Contains("未找到") || result.Message.Contains("未连接") || result.Message.Contains("离线"))
                    {
                        return NotFound(new { error = result.Message });
                    }
                    else
                    {
                        return BadRequest(new { error = result.Message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "查询充电状态请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询充电状态请求处理异常");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 配置充电桩
        /// </summary>
        [HttpPost("{deviceAddress}/config")]
        public async Task<IActionResult> ConfigureChargingPile(
            string deviceAddress,
            [FromBody] byte[] config)
        {
            try
            {
                _logger.LogInformation("收到配置充电桩请求：设备 {DeviceAddress}", deviceAddress);

                var result = await _chargingPileService.ConfigureChargingPileAsync(deviceAddress, config);

                if (result.Success)
                {
                    return Ok(new { message = "配置命令已发送" });
                }
                else
                {
                    // 根据错误信息判断返回状态码
                    if (result.Message.Contains("未找到") || result.Message.Contains("未连接") || result.Message.Contains("离线"))
                    {
                        return NotFound(new { error = result.Message });
                    }
                    else
                    {
                        return BadRequest(new { error = result.Message });
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "配置充电桩请求参数无效");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置充电桩请求处理异常");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 获取充电桩详情
        /// </summary>
        /// <param name="id">充电桩ID</param>
        /// <returns>充电桩详情</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PileDetailResponse>>> GetPileDetail(string id)
        {
            try
            {
                _logger.LogInformation("获取充电桩详情，ID: {Id}", id);

                // 查询充电桩详情
                var sql = @"
                    SELECT
                        p.id AS Id,
                        p.station_id AS StationId,
                        s.name AS StationName,
                        p.pile_no AS PileNo,
                        p.pile_type AS PileType,
                        p.status AS Status,
                        p.power_rate AS PowerRate,
                        p.manufacturer AS Manufacturer,
                        p.total_ports AS TotalPorts,
                        p.online_status AS OnlineStatus,
                        p.signal_strength AS SignalStrength,
                        p.temperature AS Temperature,
                        p.last_heartbeat_time AS LastHeartbeatTime,
                        p.update_time AS UpdateTime
                    FROM charging_piles p
                    JOIN charging_stations s ON p.station_id = s.id
                    WHERE p.id = @Id";

                var pile = await _dbConnection.QueryFirstOrDefaultAsync<PileDetailResponse>(sql, new { Id = id });

                if (pile == null)
                {
                    return NotFound(ApiResponse<PileDetailResponse>.Fail($"未找到ID为{id}的充电桩", 404));
                }

                // 查询充电桩下的所有端口
                var portSql = @"
                    SELECT
                        id AS Id,
                        port_no AS PortNo,
                        port_type AS PortType,
                        status AS Status,
                        voltage AS Voltage,
                        current_ampere AS CurrentAmpere,
                        power AS Power,
                        temperature AS Temperature,
                        electricity AS Electricity,
                        is_disabled AS IsDisabled
                    FROM charging_ports
                    WHERE pile_id = @PileId";

                var ports = await _dbConnection.QueryAsync<PortInfo>(portSql, new { PileId = id });
                pile.Ports = ports.ToList();

                return Ok(ApiResponse<PileDetailResponse>.Success(pile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取充电桩详情失败，ID: {Id}", id);
                return BadRequest(ApiResponse<PileDetailResponse>.Fail($"获取充电桩详情失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 获取充电站内所有充电桩列表
        /// </summary>
        /// <param name="stationId">充电站ID</param>
        /// <param name="status">充电桩状态，可选，不传表示查询所有状态</param>
        /// <returns>充电桩列表</returns>
        [HttpGet("station/{stationId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<StationPileResponse>>>> GetPilesByStationId(string stationId, [FromQuery] int? status = null)
        {
            try
            {
                _logger.LogInformation("获取充电站内充电桩列表，充电站ID: {StationId}, 状态过滤: {Status}", stationId, status);

                // 构建查询SQL
                var sql = @"
                    SELECT
                        p.id AS Id,
                        p.station_id AS StationId,
                        p.pile_no AS PileNo,
                        p.pile_type AS PileType,
                        p.status AS Status,
                        p.power_rate AS PowerRate,
                        p.manufacturer AS Manufacturer,
                        p.imei AS Imei,
                        p.total_ports AS TotalPorts,
                        p.floating_power AS FloatingPower,
                        p.auto_stop_enabled AS AutoStopEnabled,
                        p.plugin_wait_time AS PluginWaitTime,
                        p.removal_wait_time AS RemovalWaitTime,
                        p.supports_new_protocol AS SupportsNewProtocol,
                        p.protocol_version AS ProtocolVersion,
                        p.software_version AS SoftwareVersion,
                        p.hardware_version AS HardwareVersion,
                        p.ccid AS Ccid,
                        p.voltage AS Voltage,
                        p.ampere_value AS AmpereValue,
                        p.online_status AS OnlineStatus,
                        p.signal_strength AS SignalStrength,
                        p.temperature AS Temperature,
                        p.last_heartbeat_time AS LastHeartbeatTime,
                        p.installation_date AS InstallationDate,
                        p.last_maintenance_date AS LastMaintenanceDate,
                        p.description AS Description,
                        p.update_time AS UpdateTime,
                        (SELECT COUNT(*) FROM charging_ports WHERE pile_id = p.id AND status = 1) AS AvailablePorts
                    FROM charging_piles p
                    WHERE p.station_id = @StationId";

                // 添加状态过滤条件
                if (status.HasValue)
                {
                    sql += " AND p.status = @Status";
                }

                // 执行查询
                var parameters = new { StationId = stationId, Status = status };
                var piles = await _dbConnection.QueryAsync<StationPileResponse>(sql, parameters);

                // 如果没有找到充电桩，返回空列表
                if (!piles.Any())
                {
                    return Ok(ApiResponse<List<StationPileResponse>>.Success(new List<StationPileResponse>(), $"充电站{stationId}没有充电桩"));
                }

                // 查询每个充电桩的端口信息
                foreach (var pile in piles)
                {
                    var portSql = @"
                        SELECT
                            id AS Id,
                            port_no AS PortNo,
                            port_type AS PortType,
                            status AS Status,
                            is_disabled AS IsDisabled
                        FROM charging_ports
                        WHERE pile_id = @PileId";

                    var ports = await _dbConnection.QueryAsync<StationPortInfo>(portSql, new { PileId = pile.Id });
                    pile.Ports = ports.ToList();
                }

                return Ok(ApiResponse<List<StationPileResponse>>.Success(piles.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取充电站内充电桩列表失败，充电站ID: {StationId}", stationId);
                return BadRequest(ApiResponse<List<StationPileResponse>>.Fail($"获取充电站内充电桩列表失败: {ex.Message}"));
            }
        }

        /// <summary>
        /// 扫码获取充电桩信息
        /// </summary>
        /// <param name="code">二维码内容，通常包含充电桩标识</param>
        /// <returns>充电桩信息</returns>
        [HttpGet("qrcode/{code}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<QrCodePileResponse>>> GetPileByQrCode(string code)
        {
            try
            {
                _logger.LogInformation("扫码获取充电桩信息，二维码内容: {Code}", code);

                // 解析二维码内容，获取充电桩标识
                // 这里假设二维码内容就是充电桩的ID或编号
                // 实际应用中可能需要更复杂的解析逻辑
                string pileIdentifier = code.Trim();

                // 首先尝试通过ID查询
                var sql = @"
                    SELECT
                        p.id AS Id,
                        p.station_id AS StationId,
                        s.name AS StationName,
                        p.pile_no AS PileNo,
                        p.pile_type AS PileType,
                        p.status AS Status,
                        p.power_rate AS PowerRate
                    FROM charging_piles p
                    JOIN charging_stations s ON p.station_id = s.id
                    WHERE p.id = @Identifier OR p.pile_no = @Identifier";

                var pile = await _dbConnection.QueryFirstOrDefaultAsync<QrCodePileResponse>(sql, new { Identifier = pileIdentifier });

                if (pile == null)
                {
                    return NotFound(ApiResponse<QrCodePileResponse>.Fail($"未找到标识为{pileIdentifier}的充电桩", 404));
                }

                // 查询充电桩下的所有端口
                var portSql = @"
                    SELECT
                        id AS Id,
                        port_no AS PortNo,
                        port_type AS PortType,
                        status AS Status,
                        is_disabled AS IsDisabled
                    FROM charging_ports
                    WHERE pile_id = @PileId";

                var ports = await _dbConnection.QueryAsync<QrCodePortInfo>(portSql, new { PileId = pile.Id });
                pile.Ports = ports.ToList();

                return Ok(ApiResponse<QrCodePileResponse>.Success(pile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "扫码获取充电桩信息失败，二维码内容: {Code}", code);
                return BadRequest(ApiResponse<QrCodePileResponse>.Fail($"扫码获取充电桩信息失败: {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// 启动充电请求
    /// </summary>
    public class StartChargingRequest
    {
        /// <summary>
        /// 端口号 (1-N)
        /// </summary>
        [Required(ErrorMessage = "端口号不能为空")]
        [Range(1, 255, ErrorMessage = "端口号必须在1-255之间")]
        public byte Port { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required(ErrorMessage = "订单号不能为空")]
        public uint OrderId { get; set; }

        /// <summary>
        /// 启动方式 (1:扫码支付 2:刷卡支付 3:管理员启动)
        /// </summary>
        [Required(ErrorMessage = "启动方式不能为空")]
        [Range(1, 3, ErrorMessage = "启动方式必须是1-3之间的值")]
        public byte StartMode { get; set; }

        /// <summary>
        /// 卡号 (非IC卡启动时为0)
        /// </summary>
        [Required(ErrorMessage = "卡号不能为空")]
        public uint CardId { get; set; }

        /// <summary>
        /// 充电方式 (1:充满自停 2:按金额 3:按时间 4:按电量 5:其它)
        /// </summary>
        [Required(ErrorMessage = "充电方式不能为空")]
        [Range(1, 5, ErrorMessage = "充电方式必须是1-5之间的值")]
        public byte ChargingMode { get; set; }

        /// <summary>
        /// 充电参数 (秒/0.01元/0.01度)
        /// </summary>
        [Required(ErrorMessage = "充电参数不能为空")]
        public uint ChargingParam { get; set; }

        /// <summary>
        /// 可用金额 (0.01元)
        /// </summary>
        [Required(ErrorMessage = "可用金额不能为空")]
        public uint AvailableAmount { get; set; }
    }

    /// <summary>
    /// 停止充电请求
    /// </summary>
    public class StopChargingRequest
    {
        /// <summary>
        /// 端口号 (1-N)
        /// </summary>
        [Required(ErrorMessage = "端口号不能为空")]
        [Range(1, 255, ErrorMessage = "端口号必须在1-255之间")]
        public byte Port { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Required(ErrorMessage = "订单号不能为空")]
        public uint OrderId { get; set; }
    }

    /// <summary>
    /// API响应包装类
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 状态码：0-成功，非0-失败
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Code = 0,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        public static ApiResponse<T> Fail(string message, int code = 400)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = default
            };
        }
    }

    /// <summary>
    /// 充电桩详情响应
    /// </summary>
    public class PileDetailResponse
    {
        /// <summary>
        /// 充电桩唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属充电站ID
        /// </summary>
        public string StationId { get; set; }

        /// <summary>
        /// 所属充电站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 充电桩类型：1-直流快充，2-交流慢充
        /// </summary>
        public int PileType { get; set; }

        /// <summary>
        /// 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 额定功率(kW)
        /// </summary>
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 总端口数
        /// </summary>
        public int TotalPorts { get; set; }

        /// <summary>
        /// 在线状态：0-离线，1-在线
        /// </summary>
        public int OnlineStatus { get; set; }

        /// <summary>
        /// 信号强度
        /// </summary>
        public int SignalStrength { get; set; }

        /// <summary>
        /// 设备温度
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTime LastHeartbeatTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 充电桩端口列表
        /// </summary>
        public List<PortInfo> Ports { get; set; } = new List<PortInfo>();
    }

    /// <summary>
    /// 端口信息
    /// </summary>
    public class PortInfo
    {
        /// <summary>
        /// 端口ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int PortType { get; set; }

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 当前电压(V)
        /// </summary>
        public decimal Voltage { get; set; }

        /// <summary>
        /// 当前电流(A)
        /// </summary>
        public decimal CurrentAmpere { get; set; }

        /// <summary>
        /// 当前功率(kW)
        /// </summary>
        public decimal Power { get; set; }

        /// <summary>
        /// 端口温度
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// 当前电量(kWh)
        /// </summary>
        public decimal Electricity { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsDisabled { get; set; }
    }

    /// <summary>
    /// 扫码获取充电桩信息响应
    /// </summary>
    public class QrCodePileResponse
    {
        /// <summary>
        /// 充电桩唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属充电站ID
        /// </summary>
        public string StationId { get; set; }

        /// <summary>
        /// 所属充电站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 充电桩类型：1-直流快充，2-交流慢充
        /// </summary>
        public int PileType { get; set; }

        /// <summary>
        /// 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 额定功率(kW)
        /// </summary>
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 充电桩端口列表
        /// </summary>
        public List<QrCodePortInfo> Ports { get; set; } = new List<QrCodePortInfo>();
    }

    /// <summary>
    /// 扫码获取的端口信息
    /// </summary>
    public class QrCodePortInfo
    {
        /// <summary>
        /// 端口ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int PortType { get; set; }

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsDisabled { get; set; }
    }

    /// <summary>
    /// 充电站内充电桩列表响应
    /// </summary>
    public class StationPileResponse
    {
        /// <summary>
        /// 充电桩唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 所属充电站ID
        /// </summary>
        public string StationId { get; set; }

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string PileNo { get; set; }

        /// <summary>
        /// 充电桩类型：1-直流快充，2-交流慢充
        /// </summary>
        public int PileType { get; set; }

        /// <summary>
        /// 充电桩状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 额定功率(kW)
        /// </summary>
        public decimal PowerRate { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 设备IMEI号
        /// </summary>
        public string Imei { get; set; }

        /// <summary>
        /// 总端口数
        /// </summary>
        public int TotalPorts { get; set; }

        /// <summary>
        /// 浮充参考功率(W)
        /// </summary>
        public decimal? FloatingPower { get; set; }

        /// <summary>
        /// 充满自停开关状态
        /// </summary>
        public bool? AutoStopEnabled { get; set; }

        /// <summary>
        /// 充电器插入等待时间(秒)
        /// </summary>
        public int? PluginWaitTime { get; set; }

        /// <summary>
        /// 充电器移除等待时间(秒)
        /// </summary>
        public int? RemovalWaitTime { get; set; }

        /// <summary>
        /// 是否支持新协议格式(带IMEI)
        /// </summary>
        public bool? SupportsNewProtocol { get; set; }

        /// <summary>
        /// 协议版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// CCID号
        /// </summary>
        public string Ccid { get; set; }

        /// <summary>
        /// 额定电压
        /// </summary>
        public string Voltage { get; set; }

        /// <summary>
        /// 额定电流
        /// </summary>
        public string AmpereValue { get; set; }

        /// <summary>
        /// 在线状态：0-离线，1-在线
        /// </summary>
        public int? OnlineStatus { get; set; }

        /// <summary>
        /// 信号强度
        /// </summary>
        public int? SignalStrength { get; set; }

        /// <summary>
        /// 设备温度
        /// </summary>
        public decimal? Temperature { get; set; }

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTime? LastHeartbeatTime { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        public DateTime? InstallationDate { get; set; }

        /// <summary>
        /// 最后维护日期
        /// </summary>
        public DateTime? LastMaintenanceDate { get; set; }

        /// <summary>
        /// 充电桩描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 可用端口数
        /// </summary>
        public int AvailablePorts { get; set; }

        /// <summary>
        /// 充电桩端口列表
        /// </summary>
        public List<StationPortInfo> Ports { get; set; } = new List<StationPortInfo>();
    }

    /// <summary>
    /// 充电站内充电桩端口信息
    /// </summary>
    public class StationPortInfo
    {
        /// <summary>
        /// 端口ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 端口编号
        /// </summary>
        public string PortNo { get; set; }

        /// <summary>
        /// 端口类型：1-国标，2-欧标，3-美标
        /// </summary>
        public int PortType { get; set; }

        /// <summary>
        /// 端口状态：0-离线，1-空闲，2-使用中，3-故障
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}