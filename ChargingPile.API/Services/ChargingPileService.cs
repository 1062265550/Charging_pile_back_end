using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Communication;
using System.Text;
using System.Data;

namespace ChargingPile.API.Services
{
    /// <summary>
    /// 充电桩服务类 - 简化版
    /// </summary>
    public class ChargingPileService
    {
        private readonly ILogger<ChargingPileService> _logger;
        private readonly TcpServer _tcpServer;
        private readonly IDbConnection _dbConnection;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChargingPileService(
            ILogger<ChargingPileService> logger,
            TcpServer tcpServer,
            IDbConnection dbConnection)
        {
            _logger = logger;
            _tcpServer = tcpServer;
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// 获取充电桩IMEI
        /// </summary>
        /// <returns>结果对象，包含是否成功、IMEI和错误消息</returns>
        public async Task<(bool Success, string IMEI, string? ErrorMessage)> GetChargingPileImeiAsync(string deviceIdentifier)
        {
            try
            {
                _logger.LogInformation("尝试获取设备IMEI: {DeviceIdentifier}", deviceIdentifier);
                await Task.Delay(5); // 模拟网络延迟
                return (true, deviceIdentifier, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取设备IMEI失败: {DeviceIdentifier}", deviceIdentifier);
                return (false, string.Empty, "获取设备IMEI失败，请检查设备ID或编号是否正确");
            }
        }

        /// <summary>
        /// 统一的命令执行方法，用于处理异常和IMEI查询
        /// </summary>
        private async Task<(bool Success, string Message)> ExecuteCommandAsync(
            string deviceIdentifier,
            Func<string, Task<(bool Success, string Message)>> commandFunc)
        {
            try
            {
                // 首先获取IMEI
                var (success, imei, errorMessage) = await GetChargingPileImeiAsync(deviceIdentifier);
                if (!success)
                {
                    return (false, errorMessage ?? "未知错误");
                }

                _logger.LogInformation("准备执行命令: 设备IMEI={IMEI}", imei);
                return (true, $"设备 {imei} 命令已发送（模拟）");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行命令失败: {DeviceIdentifier}", deviceIdentifier);
                return (false, "执行命令时发生错误，请稍后重试");
            }
        }

        /// <summary>
        /// 远程启动充电
        /// </summary>
        /// <returns>操作结果，包含是否成功和消息</returns>
        public async Task<(bool Success, string Message)> StartChargingAsync(
            string deviceIdentifier,
            byte port,
            uint orderId,
            byte startMode = 0x01,
            uint cardId = 0,
            byte chargingMode = 0x01,
            uint chargingParam = 0,
            decimal availableAmount = 0)
        {
            return await ExecuteCommandAsync(deviceIdentifier, async (imei) => {
                _logger.LogInformation(
                    "发送远程启动命令: IMEI={IMEI}, 端口={Port}, 订单={OrderId}, 启动方式={StartMode}, 充电方式={ChargingMode}",
                    imei, port, orderId, startMode, chargingMode);

                // 将decimal类型的金额转换为uint（以0.01元为单位）
                uint availableAmountInt = (uint)(availableAmount * 100);

                // 调用TCP服务器的远程启动充电方法
                return await _tcpServer.StartChargingAsync(
                    imei, port, orderId, startMode, cardId, chargingMode, chargingParam, availableAmountInt);
            });
        }

        /// <summary>
        /// 远程停止充电
        /// </summary>
        /// <returns>操作结果，包含是否成功和消息</returns>
        public async Task<(bool Success, string Message)> StopChargingAsync(string deviceIdentifier, byte port, uint orderId)
        {
            return await ExecuteCommandAsync(deviceIdentifier, async (imei) => {
                _logger.LogInformation("发送远程停止命令: IMEI={IMEI}, 端口={Port}, 订单={OrderId}", imei, port, orderId);

                // 调用TCP服务器的远程停止充电方法
                return await _tcpServer.StopChargingAsync(imei, port, orderId);
            });
        }

        /// <summary>
        /// 查询充电状态
        /// </summary>
        /// <returns>操作结果，包含是否成功和消息</returns>
        public async Task<(bool Success, string Message)> QueryChargingStatusAsync(string deviceIdentifier, byte port)
        {
            return await ExecuteCommandAsync(deviceIdentifier, async (imei) => {
                _logger.LogInformation("构造查询状态命令: IMEI={IMEI}, 端口={Port}", imei, port);
                await Task.Delay(10); // 模拟网络延迟
                return (true, "查询状态命令已模拟发送");
            });
        }

        /// <summary>
        /// 配置充电桩参数
        /// </summary>
        /// <returns>操作结果，包含是否成功和消息</returns>
        public async Task<(bool Success, string Message)> ConfigureChargingPileAsync(string deviceIdentifier, byte[] configData)
        {
            return await ExecuteCommandAsync(deviceIdentifier, async (imei) => {
                _logger.LogInformation("构造配置命令: IMEI={IMEI}, 配置数据长度={ConfigLength}", imei, configData.Length);
                await Task.Delay(10); // 模拟网络延迟
                return (true, "配置命令已模拟发送");
            });
        }
    }
}