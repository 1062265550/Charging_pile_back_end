using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using ChargingPile.API.Extensions;

namespace ChargingPile.API.Communication
{
    /// <summary>
    /// TCP服务器类
    /// </summary>
    public class TcpServer
    {
        private readonly ILogger<TcpServer> _logger;
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<string, TcpClient> _connectedClients;
        private readonly ConcurrentDictionary<string, string> _deviceImeis; // 存储设备地址和IMEI的映射
        private readonly ConcurrentDictionary<string, DateTimeOffset> _deviceLastActiveTime; // 存储设备IMEI与最后活动时间的映射
        private readonly IDbConnection _dbConnection;
        private readonly AsyncRetryPolicy _retryPolicy;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpServer(ILogger<TcpServer> logger, IConfiguration configuration, IDbConnection dbConnection)
        {
            _logger = logger;

            // 从配置中获取端口
            int port = configuration.GetValue<int>("TcpServer:ListenPort", 8057);

            _listener = new TcpListener(IPAddress.Any, port);
            _cancellationTokenSource = new CancellationTokenSource();
            _connectedClients = new ConcurrentDictionary<string, TcpClient>();
            _deviceImeis = new ConcurrentDictionary<string, string>();
            _deviceLastActiveTime = new ConcurrentDictionary<string, DateTimeOffset>();
            _dbConnection = dbConnection;

            // 创建重试策略
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "执行数据库操作失败，将在 {TimeSpan} 后进行第 {RetryCount} 次重试",
                            timeSpan, retryCount);
                    });
        }

        /// <summary>
        /// 启动TCP服务器
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _listener.Start();
                _logger.LogInformation("TCP服务器已启动，正在监听端口 {Port}", ((IPEndPoint)_listener.LocalEndpoint).Port);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    string clientEndPoint = ((IPEndPoint)client.Client.RemoteEndPoint!).ToString();
                    _logger.LogInformation("收到客户端连接: {ClientEndPoint}", clientEndPoint);

                    // 添加到连接列表
                    _connectedClients.TryAdd(clientEndPoint, client);

                    // 为每个客户端启动一个独立的处理任务
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("TCP服务器接收任务已取消");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TCP服务器异常");
            }
            finally
            {
                _listener.Stop();
                _logger.LogInformation("TCP服务器已停止");
            }
        }

        /// <summary>
        /// 停止TCP服务器
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            foreach (var client in _connectedClients.Values)
            {
                client.Close();
            }
            _connectedClients.Clear();
            _deviceImeis.Clear();
            _deviceLastActiveTime.Clear();
            _listener.Stop();
            _logger.LogInformation("TCP服务器已停止");
        }

        /// <summary>
        /// 获取已连接的设备列表
        /// </summary>
        public List<string> GetConnectedDevices()
        {
            return _deviceImeis.Values.ToList();
        }

        /// <summary>
        /// 获取设备的连接端点
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <returns>设备端点，如果设备未连接则返回null</returns>
        public string? GetDeviceEndPoint(string imei)
        {
            if (string.IsNullOrEmpty(imei))
            {
                return null;
            }

            return _deviceImeis.FirstOrDefault(x => x.Value == imei).Key;
        }

        /// <summary>
        /// 处理客户端连接
        /// </summary>
        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            string clientEndPoint = ((IPEndPoint)client.Client.RemoteEndPoint!).ToString();
            _logger.LogInformation("开始处理客户端连接: {ClientEndPoint}", clientEndPoint);

            try
            {
                NetworkStream stream = client.GetStream();
                var buffer = new byte[4096];

                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    try
                    {
                        // 检查是否有数据可读
                        if (client.Available > 0 || stream.DataAvailable)
                        {
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                            if (bytesRead > 0)
                            {
                                // 复制接收到的数据
                                byte[] receivedData = new byte[bytesRead];
                                Array.Copy(buffer, receivedData, bytesRead);

                                _logger.LogInformation("从客户端 {ClientEndPoint} 接收到 {BytesRead} 字节数据",
                                    clientEndPoint, bytesRead);

                                // 解析并处理数据包
                                var packet = TcpPacket.FromBytes(receivedData);
                                if (packet != null)
                                {
                                    // 记录接收到的数据包信息
                                    _logger.LogInformation("解析数据包: 控制码=0x{ControlCode:X2}, 结果=0x{Result:X2}, IMEI={IMEI}",
                                        packet.ControlCode, packet.Result, packet.IMEI);

                                    // 根据控制码分发处理
                                    switch (packet.ControlCode)
                                    {
                                        case TcpPacket.ControlCodes.DEVICE_LOGIN:
                                            // 处理设备登录请求
                                            await HandleDeviceLoginAsync(client, clientEndPoint, packet);
                                            break;

                                        case TcpPacket.ControlCodes.HEARTBEAT:
                                            // 处理心跳请求
                                            await HandleHeartbeatAsync(client, clientEndPoint, packet);
                                            break;

                                        case TcpPacket.ControlCodes.SUBMIT_CHARGING_END:
                                            // 处理充电结束状态提交
                                            await HandleChargingEndAsync(client, clientEndPoint, packet);
                                            break;

                                        default:
                                            _logger.LogWarning("收到未处理的控制码: 0x{ControlCode:X2}", packet.ControlCode);
                                            break;
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("从客户端 {ClientEndPoint} 收到无效数据包", clientEndPoint);
                                }
                            }
                            else
                            {
                                // 客户端已关闭连接
                                _logger.LogInformation("客户端 {ClientEndPoint} 已断开连接", clientEndPoint);
                                break;
                            }
                        }
                        else
                        {
                            // 等待一段时间，避免CPU使用率过高
                            await Task.Delay(100, cancellationToken);
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogWarning(ex, "客户端 {ClientEndPoint} 连接异常", clientEndPoint);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理客户端 {ClientEndPoint} 数据时发生错误", clientEndPoint);
                        // 继续处理，不中断循环
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理客户端 {ClientEndPoint} 连接时发生错误", clientEndPoint);
            }
            finally
            {
                // 清理连接资源
                if (_connectedClients.TryRemove(clientEndPoint, out _))
                {
                    _logger.LogInformation("已移除客户端连接: {ClientEndPoint}", clientEndPoint);
                }

                // 移除IMEI映射
                if (_deviceImeis.TryGetValue(clientEndPoint, out var imei))
                {
                    _deviceImeis.TryRemove(clientEndPoint, out _);
                    _logger.LogInformation("已移除设备 {IMEI} 的映射", imei);
                }

                // 关闭客户端连接
                try
                {
                    client.Close();
                    _logger.LogInformation("已关闭客户端连接: {ClientEndPoint}", clientEndPoint);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "关闭客户端连接时发生错误: {ClientEndPoint}", clientEndPoint);
                }
            }
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="client">客户端连接</param>
        /// <param name="packet">要发送的数据包</param>
        /// <returns>发送结果</returns>
        public async Task<bool> SendMessageAsync(TcpClient client, TcpPacket packet)
        {
            try
            {
                // 确保客户端连接有效
                if (client == null || !client.Connected)
                {
                    _logger.LogWarning("尝试向断开的客户端发送数据");
                    return false;
                }

                // 将数据包转换为字节数组
                if (packet == null)
                {
                    _logger.LogWarning("数据包为null，无法发送");
                    return false;
                }

                byte[] data = packet.ToBytes();
                if (data == null || data.Length == 0)
                {
                    _logger.LogWarning("数据包转换为字节数组失败");
                    return false;
                }

                // 记录将要发送的数据包内容
                _logger.LogInformation("发送数据包：控制码 0x{ControlCode:X2}, 结果 0x{Result:X2}, IMEI {IMEI}, 数据长度 {DataLength}字节",
                    packet.ControlCode, packet.Result, packet.IMEI ?? "", packet.Data?.Length ?? 0);
                _logger.LogDebug("发送数据包原始内容: {PacketData}", BitConverter.ToString(data));

                // 验证数据包至少有最小长度
                if (data.Length < 7) // 帧头(2)+长度(2)+控制码(1)+结果(1)+校验和(1)
                {
                    _logger.LogError("数据包长度不足：{Length}字节，最小需要7字节", data.Length);
                    return false;
                }

                // 获取客户端网络流
                NetworkStream stream = client.GetStream();

                // 发送数据包
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();

                // 记录校验和信息
                byte checksum = data[data.Length - 1];
                _logger.LogDebug("发送的校验和: 0x{Checksum:X2}", checksum);

                // 根据设备特性文档要求，两个数据包之间需要间隔至少100ms
                // 这里在发送完成后等待100ms，以确保下一个数据包发送时有足够的间隔
                await Task.Delay(100);
                _logger.LogDebug("已等待100ms以确保设备稳定处理数据");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送数据包时发生错误");
                return false;
            }
        }



        /// <summary>
        /// 处理设备登录
        /// </summary>
        private async Task HandleDeviceLoginAsync(TcpClient client, string clientEndPoint, TcpPacket packet)
        {
            try
            {
                // 使用辅助类解析登录数据
                var deviceInfo = TcpPacket.DeviceLoginInfo.FromPacket(packet);
                if (deviceInfo == null)
                {
                    _logger.LogWarning("无法解析设备登录数据");
                    return;
                }

                string imei = deviceInfo.IMEI;
                _logger.LogInformation("设备登录: IMEI={IMEI}, 端口数={PortCount}, 硬件版本={HardwareVersion}, 软件版本={SoftwareVersion}, CCID={CCID}, 协议版本=0x{ProtocolVersion:X2}, 登录原因=0x{LoginReason:X2}",
                    imei, deviceInfo.PortCount, deviceInfo.HardwareVersion, deviceInfo.SoftwareVersion, deviceInfo.CCID, deviceInfo.ProtocolVersion, deviceInfo.LoginReason);

                // 尝试将设备信息保存到数据库
                await _retryPolicy.ExecuteAsync(async () => {
                    try
                    {
                        // 检查设备是否已存在
                        var existingDevice = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT id FROM charging_piles WHERE imei = @imei",
                            new { imei });

                        if (existingDevice == null)
                        {
                            // 从充电站表中随机选择一个充电站ID
                            string stationId = await GetRandomStationIdAsync();

                            // 新设备，添加到数据库
                            await _dbConnection.ExecuteAsync(
                                "INSERT INTO charging_piles (id, station_id, pile_no, pile_type, status, power_rate, " +
                                "imei, total_ports, hardware_version, software_version, ccid, protocol_version, " +
                                "online_status, last_heartbeat_time, update_time) " +
                                "VALUES (@id, @station_id, @pile_no, @pile_type, @status, @power_rate, " +
                                "@imei, @total_ports, @hardware_version, @software_version, @ccid, @protocol_version, " +
                                "@online_status, @last_heartbeat_time, @update_time)",
                                new {
                                    id = Guid.NewGuid().ToString(),
                                    station_id = stationId,
                                    pile_no = $"AUTO_{imei.Substring(Math.Max(0, imei.Length - 6))}", // 使用IMEI的后6位作为编号
                                    pile_type = 2, // 默认为交流慢充
                                    status = 1, // 默认为空闲状态
                                    power_rate = 7.0m, // 默认额定功率7kW
                                    imei = deviceInfo.IMEI,
                                    total_ports = deviceInfo.PortCount,
                                    hardware_version = deviceInfo.HardwareVersion,
                                    software_version = deviceInfo.SoftwareVersion,
                                    ccid = deviceInfo.CCID,
                                    protocol_version = deviceInfo.ProtocolVersion.ToString("X2"),
                                    online_status = 1, // 在线状态
                                    last_heartbeat_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime,
                                    update_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime
                                });

                            _logger.LogInformation("新设备 {IMEI} 已添加到数据库", imei);
                        }
                        else
                        {
                            // 更新现有设备信息
                            await _dbConnection.ExecuteAsync(
                                "UPDATE charging_piles SET total_ports = @total_ports, hardware_version = @hardware_version, " +
                                "software_version = @software_version, ccid = @ccid, protocol_version = @protocol_version, " +
                                "online_status = @online_status, last_heartbeat_time = @last_heartbeat_time, " +
                                "update_time = @update_time WHERE imei = @imei",
                                new {
                                    imei = deviceInfo.IMEI,
                                    total_ports = deviceInfo.PortCount,
                                    hardware_version = deviceInfo.HardwareVersion,
                                    software_version = deviceInfo.SoftwareVersion,
                                    ccid = deviceInfo.CCID,
                                    protocol_version = deviceInfo.ProtocolVersion.ToString("X2"),
                                    online_status = 1, // 在线状态
                                    last_heartbeat_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime,
                                    update_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime
                                });

                            _logger.LogInformation("设备 {IMEI} 信息已更新", imei);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "保存设备信息到数据库时发生错误");
                        throw; // 重新抛出异常，触发重试策略
                    }
                });

                // 检查是否已存在相同IMEI的连接
                var existingEndpoint = _deviceImeis.FirstOrDefault(x => x.Value == imei).Key;
                if (!string.IsNullOrEmpty(existingEndpoint) && existingEndpoint != clientEndPoint)
                {
                    _logger.LogWarning("设备 {IMEI} 已存在连接 {ExistingEndpoint}，将替换为新连接 {ClientEndPoint}",
                        imei, existingEndpoint, clientEndPoint);

                    // 从映射中移除旧连接
                    _deviceImeis.TryRemove(existingEndpoint, out _);

                    // 关闭旧连接
                    if (_connectedClients.TryGetValue(existingEndpoint, out var oldClient))
                    {
                        try
                        {
                            oldClient.Close();
                            _connectedClients.TryRemove(existingEndpoint, out _);
                            _logger.LogInformation("已关闭设备 {IMEI} 的旧连接 {ExistingEndpoint}",
                                imei, existingEndpoint);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "关闭设备 {IMEI} 的旧连接 {ExistingEndpoint} 时发生错误",
                                imei, existingEndpoint);
                        }
                    }
                }

                // 将设备端点与IMEI关联
                _deviceImeis[clientEndPoint] = imei;
                _deviceLastActiveTime[imei] = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));
                _logger.LogInformation("已将设备 {IMEI} 与客户端 {ClientEndPoint} 关联", imei, clientEndPoint);

                // 创建登录响应包
                byte heartbeatInterval = 60; // 心跳间隖60秒，符合文档推荐的60-120秒

                // 验证心跳间隔在协议规定的范围内（10-250秒）
                if (heartbeatInterval < 10)
                {
                    _logger.LogWarning("心跳间隔小于最小值10秒，已调整为10秒");
                    heartbeatInterval = 10;
                }
                else if (heartbeatInterval > 250)
                {
                    _logger.LogWarning("心跳间隔大于最大值250秒，已调整为250秒");
                    heartbeatInterval = 250;
                }

                // 判断是否支持新协议 (协议版本>=0x64)
                byte loginResult = 0x00; // 默认为正常登录成功

                if (deviceInfo.ProtocolVersion >= TcpPacket.PROTOCOL_VERSION)
                {
                    _logger.LogInformation("设备 {IMEI} 支持新协议格式(协议版本:0x{ProtocolVersion:X2})，将返回0xF0使其切换到新协议",
                        imei, deviceInfo.ProtocolVersion);
                    loginResult = 0xF0; // 登录成功且切换到新协议格式
                }

                var response = TcpPacket.CreateLoginResponse(imei, heartbeatInterval, loginResult);

                // 记录将要发送的登录响应数据
                byte[] responseData = response.Data;
                _logger.LogInformation("登录响应数据[{Length}字节]: {Data}, 登录结果:0x{LoginResult:X2}",
                    responseData.Length, BitConverter.ToString(responseData), loginResult);

                // 验证响应数据长度
                if (responseData.Length != 9)
                {
                    _logger.LogError("登录响应数据长度错误: {Length}字节，应为9字节", responseData.Length);
                }

                // 发送响应
                bool sendResult = await SendMessageAsync(client, response);
                if (sendResult)
                {
                    _logger.LogInformation("已成功发送登录响应到设备 {IMEI}", imei);
                }
                else
                {
                    _logger.LogError("发送登录响应到设备 {IMEI} 失败", imei);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理设备登录时发生错误");
            }
        }

        /// <summary>
        /// 处理设备心跳
        /// </summary>
        private async Task HandleHeartbeatAsync(TcpClient client, string clientEndPoint, TcpPacket packet)
        {
            try
            {
                // 使用辅助类解析心跳数据
                var heartbeatInfo = TcpPacket.HeartbeatInfo.FromPacket(packet);
                if (heartbeatInfo == null)
                {
                    _logger.LogWarning("无法解析心跳数据");
                    return;
                }

                // 获取设备IMEI
                string imei = packet.IMEI;
                if (string.IsNullOrEmpty(imei) && _deviceImeis.TryGetValue(clientEndPoint, out var deviceImei))
                {
                    // 如果数据包中没有IMEI，尝试从映射中获取
                    imei = deviceImei;
                }

                if (string.IsNullOrEmpty(imei))
                {
                    _logger.LogWarning("无法确定心跳数据的设备IMEI");
                    return;
                }

                _logger.LogInformation("收到心跳数据：IMEI={IMEI}, 信号强度={SignalStrength}, 温度={Temperature}°C, 端口数={PortCount}",
                    imei, heartbeatInfo.SignalStrength, heartbeatInfo.Temperature, heartbeatInfo.PortCount);

                // 记录每个端口的状态
                for (int i = 0; i < heartbeatInfo.PortStatus.Length; i++)
                {
                    _logger.LogInformation("端口{PortNo}状态: {Status}", i + 1, heartbeatInfo.GetPortStatusText(i));
                }

                // 更新设备最后活跃时间
                _deviceLastActiveTime[imei] = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));

                // 尝试更新数据库中的设备状态
                await _retryPolicy.ExecuteAsync(async () => {
                    try
                    {
                        // 更新设备最后活跃时间
                        await _dbConnection.ExecuteAsync(
                            "UPDATE charging_piles SET last_heartbeat_time = @last_heartbeat_time, update_time = @update_time WHERE imei = @imei",
                            new {
                                imei,
                                last_heartbeat_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime,
                                update_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime
                            });

                        // 更新端口状态
                        for (int i = 0; i < heartbeatInfo.PortStatus.Length; i++)
                        {
                            int portNo = i + 1;
                            byte status = heartbeatInfo.PortStatus[i];

                            // 首先获取充电桩的ID
                            var pileId = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                                "SELECT id FROM charging_piles WHERE imei = @imei",
                                new { imei });

                            if (string.IsNullOrEmpty(pileId))
                            {
                                _logger.LogWarning("无法找到IMEI为 {IMEI} 的充电桩记录", imei);
                                continue;
                            }

                            // 检查端口是否存在
                            var existingPort = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT id FROM charging_ports WHERE pile_id = @pile_id AND port_no = @port_no",
                                new { pile_id = pileId, port_no = portNo.ToString() });

                            if (existingPort == null)
                            {
                                // 新端口，添加到数据库
                                await _dbConnection.ExecuteAsync(
                                    "INSERT INTO charging_ports (id, pile_id, port_no, status, update_time) " +
                                    "VALUES (@id, @pile_id, @port_no, @status, @update_time)",
                                    new {
                                        id = Guid.NewGuid().ToString(),
                                        pile_id = pileId,
                                        port_no = portNo.ToString(),
                                        status = status,
                                        update_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime
                                    });
                            }
                            else
                            {
                                // 更新现有端口状态
                                await _dbConnection.ExecuteAsync(
                                    "UPDATE charging_ports SET status = @status, update_time = @update_time " +
                                    "WHERE pile_id = @pile_id AND port_no = @port_no",
                                    new {
                                        pile_id = pileId,
                                        port_no = portNo.ToString(),
                                        status = status,
                                        update_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime
                                    });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "更新设备心跳数据到数据库时发生错误");
                        throw; // 重新抛出异常，触发重试策略
                    }
                });

                // 创建心跳响应包
                var response = TcpPacket.CreateHeartbeatResponse(imei);

                // 发送响应
                bool sendResult = await SendMessageAsync(client, response);
                if (sendResult)
                {
                    _logger.LogInformation("已成功发送心跳响应到设备 {IMEI}", imei);
                }
                else
                {
                    _logger.LogError("发送心跳响应到设备 {IMEI} 失败", imei);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理设备心跳时发生错误");
            }
        }

        /// <summary>
        /// 处理充电结束状态提交
        /// </summary>
        private async Task HandleChargingEndAsync(TcpClient client, string clientEndPoint, TcpPacket packet)
        {
            // 简单实现，只记录日志
            _logger.LogInformation("收到充电结束状态提交: IMEI={IMEI}", packet.IMEI);

            // 创建简单响应
            var response = new TcpPacket
            {
                ControlCode = packet.ControlCode,
                Result = 0x01, // 成功（根据协议要求）
                IMEI = packet.IMEI,
                Data = new byte[1] { 0x00 } // 预留字节
            };

            // 发送响应
            await SendMessageAsync(client, response);
        }

        /// <summary>
        /// 远程启动充电
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <param name="port">端口号</param>
        /// <param name="orderId">订单号</param>
        /// <param name="startMode">启动方式：1：扫码支付 2：刷卡支付 3：管理员启动</param>
        /// <param name="cardId">卡号（非IC卡启动时为0）</param>
        /// <param name="chargingMode">充电方式：1：充满自停 2：按金额 3：按时间 4：按电量 5：其它</param>
        /// <param name="chargingParam">充电参数（秒/0.01元/0.01度）</param>
        /// <param name="availableAmount">可用金额（0.01元）</param>
        /// <returns>启动结果，包含成功/失败状态和错误信息</returns>
        public async Task<(bool Success, string Message)> StartChargingAsync(
            string imei,
            byte port,
            uint orderId,
            byte startMode = 1,
            uint cardId = 0,
            byte chargingMode = 1,
            uint chargingParam = 0,
            uint availableAmount = 0)
        {
            try
            {
                // 检查设备是否在线
                string clientEndPoint = GetDeviceEndPoint(imei);
                if (string.IsNullOrEmpty(clientEndPoint))
                {
                    return (false, $"设备 {imei} 不在线");
                }

                // 获取客户端连接
                if (!_connectedClients.TryGetValue(clientEndPoint, out var client) || client == null || !client.Connected)
                {
                    return (false, $"设备 {imei} 连接已断开");
                }

                // 创建远程启动充电命令包
                var packet = TcpPacket.CreateStartChargingCommand(
                    imei, port, orderId, startMode, cardId, chargingMode, chargingParam, availableAmount);

                // 发送命令
                _logger.LogInformation("发送远程启动充电命令到设备 {IMEI}, 端口 {Port}, 订单号 {OrderId}",
                    imei, port, orderId);

                bool sendResult = await SendMessageAsync(client, packet);
                if (!sendResult)
                {
                    return (false, $"发送命令到设备 {imei} 失败");
                }

                // 等待设备响应（实际应用中应该使用异步消息队列或回调机制）
                // 这里简化实现，直接返回成功
                return (true, $"已发送远程启动充电命令到设备 {imei}, 端口 {port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "远程启动充电时发生错误: {IMEI}, 端口 {Port}", imei, port);
                return (false, $"远程启动充电时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 从充电站表中随机选择一个充电站ID
        /// </summary>
        /// <returns>充电站ID，如果没有充电站记录则创建一个默认的</returns>
        private async Task<string> GetRandomStationIdAsync()
        {
            try
            {
                // 查询所有充电站ID
                var stationIds = await _dbConnection.QueryAsync<string>("SELECT id FROM charging_stations");

                // 如果有充电站记录，随机选择一个
                if (stationIds.Any())
                {
                    var random = new Random();
                    int index = random.Next(stationIds.Count());
                    return stationIds.ElementAt(index);
                }

                // 如果没有充电站记录，创建一个默认的
                string defaultStationId = Guid.NewGuid().ToString();
                await _dbConnection.ExecuteAsync(
                    "INSERT INTO charging_stations (id, name, status, address, update_time) " +
                    "VALUES (@id, @name, @status, @address, @update_time)",
                    new {
                        id = defaultStationId,
                        name = "默认充电站",
                        status = 1, // 在线状态
                        address = "默认地址",
                        update_time = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime
                    });

                _logger.LogInformation("创建默认充电站，ID: {StationId}", defaultStationId);
                return defaultStationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取随机充电站ID失败");

                // 如果出错，返回一个固定的默认ID
                return "default-station-id";
            }
        }

        /// <summary>
        /// 远程停止充电
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <param name="port">端口号</param>
        /// <param name="orderId">订单号</param>
        /// <returns>停止结果，包含成功/失败状态和错误信息</returns>
        public async Task<(bool Success, string Message)> StopChargingAsync(string imei, byte port, uint orderId)
        {
            try
            {
                // 检查设备是否在线
                string clientEndPoint = GetDeviceEndPoint(imei);
                if (string.IsNullOrEmpty(clientEndPoint))
                {
                    return (false, $"设备 {imei} 不在线");
                }

                // 获取客户端连接
                if (!_connectedClients.TryGetValue(clientEndPoint, out var client) || client == null || !client.Connected)
                {
                    return (false, $"设备 {imei} 连接已断开");
                }

                // 创建远程停止充电命令包
                var packet = TcpPacket.CreateStopChargingCommand(imei, port, orderId);

                // 发送命令
                _logger.LogInformation("发送远程停止充电命令到设备 {IMEI}, 端口 {Port}, 订单号 {OrderId}",
                    imei, port, orderId);

                bool sendResult = await SendMessageAsync(client, packet);
                if (!sendResult)
                {
                    return (false, $"发送命令到设备 {imei} 失败");
                }

                // 等待设备响应（实际应用中应该使用异步消息队列或回调机制）
                // 这里简化实现，直接返回成功
                return (true, $"已发送远程停止充电命令到设备 {imei}, 端口 {port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "远程停止充电时发生错误: {IMEI}, 端口 {Port}", imei, port);
                return (false, $"远程停止充电时发生错误: {ex.Message}");
            }
        }
    }
}
