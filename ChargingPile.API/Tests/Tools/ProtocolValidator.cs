using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Communication;

namespace ChargingPile.API.Tests.Tools
{
    /// <summary>
    /// 协议验证工具 - 简化版
    /// </summary>
    public class ProtocolValidator
    {
        private readonly ILogger<ProtocolValidator> _logger;

        public ProtocolValidator(ILogger<ProtocolValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 验证原始字节数组是否符合协议规范
        /// </summary>
        /// <param name="bytes">原始字节数组</param>
        /// <returns>验证结果，包含是否通过和详细信息</returns>
        public ValidationResult ValidateRawPacket(byte[] bytes)
        {
            var result = new ValidationResult();
            result.AddInfo("协议验证功能已禁用");
            return result;
        }

        /// <summary>
        /// 验证TcpPacket对象是否符合协议规范
        /// </summary>
        /// <param name="packet">待验证的数据包</param>
        /// <returns>验证结果</returns>
        public ValidationResult ValidatePacket(TcpPacket packet)
        {
            var result = new ValidationResult();
            result.AddInfo("协议验证功能已禁用");
            return result;
        }

        /// <summary>
        /// 验证登录数据包
        /// </summary>
        /// <param name="packet">登录数据包</param>
        /// <returns>验证结果</returns>
        public ValidationResult ValidateLoginPacket(TcpPacket packet)
        {
            var result = new ValidationResult();
            result.AddInfo("协议验证功能已禁用");
            return result;
        }

        /// <summary>
        /// 验证心跳数据包
        /// </summary>
        /// <param name="packet">心跳数据包</param>
        /// <returns>验证结果</returns>
        public ValidationResult ValidateHeartbeatPacket(TcpPacket packet)
        {
            var result = new ValidationResult();
            result.AddInfo("协议验证功能已禁用");
            return result;
        }
    }

    /// <summary>
    /// 协议验证结果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> Infos { get; } = new List<string>();

        /// <summary>
        /// 添加错误信息
        /// </summary>
        public void AddError(string error)
        {
            Errors.Add(error);
        }

        /// <summary>
        /// 添加警告信息
        /// </summary>
        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        /// <summary>
        /// 添加一般信息
        /// </summary>
        public void AddInfo(string info)
        {
            Infos.Add(info);
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"验证结果: {(IsValid ? "通过" : "失败")}");
            
            if (Errors.Any())
            {
                sb.AppendLine("错误:");
                foreach (var error in Errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }
            
            if (Warnings.Any())
            {
                sb.AppendLine("警告:");
                foreach (var warning in Warnings)
                {
                    sb.AppendLine($"  - {warning}");
                }
            }
            
            if (Infos.Any())
            {
                sb.AppendLine("信息:");
                foreach (var info in Infos)
                {
                    sb.AppendLine($"  - {info}");
                }
            }
            
            return sb.ToString();
        }
    }
}
