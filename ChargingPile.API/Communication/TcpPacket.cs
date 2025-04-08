using System;
using System.Text;

namespace ChargingPile.API.Communication
{
    /// <summary>
    /// TCP数据包格式 - 简化版本
    /// </summary>
    public class TcpPacket
    {
        // 起始标识，固定值 0x5AA5
        public const ushort START_FLAG = 0x5AA5;
        // 协议版本
        public const byte PROTOCOL_VERSION = 0x64; // 新协议版本从0x64开始

        // 包长度（从CMD到SUM，包含CMD和SUM）
        public ushort Length { get; set; }
        // 命令字节
        public byte ControlCode { get; set; }
        // 结果字节（0x01成功，0x00失败，0xFF无络）
        public byte Result { get; set; }
        // 设备编号IMEI（15字节ASCII码）
        public string IMEI { get; set; } = string.Empty;
        // 数据区
        public byte[] Data { get; set; } = Array.Empty<byte>();
        // 校验和
        public byte CheckSum { get; set; }

        /// <summary>
        /// 控制码定义
        /// </summary>
        public static class ControlCodes
        {
            /// <summary>
            /// 设备登录
            /// </summary>
            public const byte DEVICE_LOGIN = 0x81;

            /// <summary>
            /// 心跳数据
            /// </summary>
            public const byte HEARTBEAT = 0x82;
            
            /// <summary>
            /// 远程启动充电
            /// </summary>
            public const byte START_CHARGING = 0x83;

            /// <summary>
            /// 远程停止充电
            /// </summary>
            public const byte STOP_CHARGING = 0x84;
            
            /// <summary>
            /// 提交充电结束状态
            /// </summary>
            public const byte SUBMIT_CHARGING_END = 0x85;
            
            /// <summary>
            /// 主动上报IMEI身份
            /// </summary>
            public const byte REPORT_IMEI = 0xC0;
        }

        /// <summary>
        /// 将数据包转换为字节数组 - 简化版本，仅供编译
        /// </summary>
        public byte[] ToBytes()
        {
            // 简化实现，仅用于让代码编译通过
            byte[] placeholder = new byte[10];
            return placeholder;
        }

        /// <summary>
        /// 从字节数组解析数据包 - 简化版本，仅供编译
        /// </summary>
        public static TcpPacket FromBytes(byte[] data)
        {
            // 简化实现，仅用于让代码编译通过
            return new TcpPacket();
        }
    }
}
