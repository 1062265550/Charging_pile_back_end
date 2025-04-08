using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Communication;
using ChargingPile.API.Tests.Tools;

namespace ChargingPile.API.Tests
{
    /// <summary>
    /// 增强版充电桩测试客户端 - 空实现
    /// </summary>
    public class EnhancedTestClient
    {
        private readonly ILogger<EnhancedTestClient> _logger;

        public EnhancedTestClient(
            ILogger<EnhancedTestClient> logger,
            ILogger<ProtocolValidator> validatorLogger, 
            string serverIp, 
            int serverPort, 
            string imei)
        {
            _logger = logger;
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public async Task ConnectAsync()
        {
            _logger.LogInformation("连接方法已禁用");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            _logger.LogInformation("断开连接方法已禁用");
        }

        /// <summary>
        /// 发送登录请求
        /// </summary>
        public async Task SendLoginRequestAsync()
        {
            _logger.LogInformation("发送登录请求方法已禁用");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 发送心跳数据
        /// </summary>
        public async Task SendHeartbeatAsync()
        {
            _logger.LogInformation("发送心跳数据方法已禁用");
            await Task.CompletedTask;
        }
    }
}
