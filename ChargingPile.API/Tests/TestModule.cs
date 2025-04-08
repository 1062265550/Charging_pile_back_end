using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChargingPile.API.Tests.Tools;

namespace ChargingPile.API.Tests
{
    /// <summary>
    /// 测试模块入口类 - 简化版
    /// </summary>
    public class TestModule
    {
        private readonly ILogger<TestModule> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string[] _args;

        public TestModule(ILogger<TestModule> logger, IServiceProvider serviceProvider, string[] args)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _args = args;
        }

        /// <summary>
        /// 运行测试模块
        /// </summary>
        public async Task RunAsync()
        {
            _logger.LogInformation("测试模块已禁用");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 判断是否包含指定参数
        /// </summary>
        private bool HasArgument(string name)
        {
            return Array.IndexOf(_args, name) >= 0;
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        private T GetArgumentValue<T>(string name, T defaultValue)
        {
            int index = Array.IndexOf(_args, name);
            if (index < 0 || index >= _args.Length - 1)
                return defaultValue;

            string value = _args[index + 1];
            try
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)int.Parse(value);
                    
                if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                    return (T)(object)double.Parse(value);
                    
                return (T)(object)value;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private void ShowHelp()
        {
            Console.WriteLine("充电桩测试工具 - 帮助");
            Console.WriteLine("======================================");
            Console.WriteLine("用法: dotnet run -- [选项]");
            Console.WriteLine();
            Console.WriteLine("选项:");
            Console.WriteLine("  --help, -h                显示帮助信息");
            Console.WriteLine("  --server-ip <IP>          服务器IP地址，默认为127.0.0.1");
            Console.WriteLine("  --server-port <PORT>      服务器端口，默认为8057");
            Console.WriteLine("  --imei <IMEI>             设备IMEI，默认为861197062934387");
        }
    }
}
