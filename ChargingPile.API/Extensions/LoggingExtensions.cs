using Microsoft.Extensions.Logging;
using System;

namespace ChargingPile.API.Extensions
{
    /// <summary>
    /// 日志扩展方法，解决动态参数调用问题
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// 记录信息日志，解决动态参数调用问题
        /// </summary>
        public static void SafeLogInformation(this ILogger logger, string message, params object[] args)
        {
            // 确保参数不为null
            if (args == null || args.Length == 0)
            {
                logger.LogInformation(message);
                return;
            }

            // 直接调用LogInformation，但使用显式的参数传递方式
            switch (args.Length)
            {
                case 1:
                    logger.LogInformation(message, args[0]);
                    break;
                case 2:
                    logger.LogInformation(message, args[0], args[1]);
                    break;
                case 3:
                    logger.LogInformation(message, args[0], args[1], args[2]);
                    break;
                case 4:
                    logger.LogInformation(message, args[0], args[1], args[2], args[3]);
                    break;
                case 5:
                    logger.LogInformation(message, args[0], args[1], args[2], args[3], args[4]);
                    break;
                default:
                    // 对于超过5个参数的情况，使用一个固定的消息
                    logger.LogInformation("无法显示完整日志，参数过多: " + message);
                    break;
            }
        }
    }
} 