using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ChargingPile.API.Communication
{
    /// <summary>
    /// TCP数据包格式
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
            /// 本地启动上报
            /// </summary>
            public const byte LOCAL_START_REPORT = 0x86;

            /// <summary>
            /// 在线卡信息
            /// </summary>
            public const byte ONLINE_CARD_INFO = 0x87;

            /// <summary>
            /// 查询端口数据
            /// </summary>
            public const byte QUERY_PORT_DATA = 0x88;

            /// <summary>
            /// 主动上报IMEI身份
            /// </summary>
            public const byte REPORT_IMEI = 0xC0;
        }

        /// <summary>
        /// 创建一个新的数据包
        /// </summary>
        /// <param name="controlCode">控制码</param>
        /// <param name="result">结果码</param>
        /// <param name="imei">设备IMEI</param>
        /// <param name="data">数据区</param>
        /// <returns>创建的数据包</returns>
        public static TcpPacket Create(byte controlCode, byte result, string imei, byte[] data)
        {
            var packet = new TcpPacket
            {
                ControlCode = controlCode,
                Result = result,
                IMEI = imei,
                Data = data ?? Array.Empty<byte>()
            };

            // 计算数据长度
            int dataLength = packet.Data.Length;
            if (!string.IsNullOrEmpty(imei))
            {
                dataLength += 15; // IMEI长度固定为15字节
            }

            // 总长度 = 控制码(1) + 结果码(1) + IMEI(0或15) + 数据区(n) + 校验和(1)
            packet.Length = (ushort)(dataLength + 3); // 3 = 控制码(1) + 结果码(1) + 校验和(1)

            return packet;
        }

        /// <summary>
        /// 将数据包转换为字节数组
        /// </summary>
        /// <returns>字节数组格式的数据包</returns>
        public byte[] ToBytes()
        {
            // 计算数据包总长度
            // 帧头(2) + 长度(2) + 控制码(1) + 结果码(1) + IMEI(0或15) + 数据区(n) + 校验和(1)
            int totalLength = 7; // 2 + 2 + 1 + 1 + 1

            if (!string.IsNullOrEmpty(IMEI))
            {
                totalLength += 15; // IMEI长度固定为15字节
            }

            if (Data != null)
            {
                totalLength += Data.Length;
            }

            // 创建字节数组
            byte[] packet = new byte[totalLength];

            // 写入帧头
            packet[0] = (byte)(START_FLAG & 0xFF); // 低字节
            packet[1] = (byte)((START_FLAG >> 8) & 0xFF); // 高字节

            // 写入长度
            packet[2] = (byte)(Length & 0xFF); // 低字节
            packet[3] = (byte)((Length >> 8) & 0xFF); // 高字节

            // 写入控制码和结果码
            packet[4] = ControlCode;
            packet[5] = Result;

            int offset = 6;

            // 写入IMEI（如果有）
            if (!string.IsNullOrEmpty(IMEI))
            {
                byte[] imeiBytes = Encoding.ASCII.GetBytes(IMEI);
                Array.Copy(imeiBytes, 0, packet, offset, Math.Min(imeiBytes.Length, 15));
                offset += 15;
            }

            // 写入数据区（如果有）
            if (Data != null && Data.Length > 0)
            {
                Array.Copy(Data, 0, packet, offset, Data.Length);
                offset += Data.Length;
            }

            // 计算并写入校验和
            byte checksum = CalculateChecksum();
            packet[offset] = checksum;

            return packet;
        }

        /// <summary>
        /// 计算数据包校验和
        /// </summary>
        /// <returns>计算出的校验和字节</returns>
        public byte CalculateChecksum()
        {
            // 校验和计算：从LEN到DATA结束所有字节相加得到的值取低8位
            int sum = Length & 0xFF; // 长度低字节
            sum += (Length >> 8) & 0xFF; // 长度高字节
            sum += ControlCode;
            sum += Result;

            // 加上IMEI（如果有）
            if (!string.IsNullOrEmpty(IMEI))
            {
                byte[] imeiBytes = Encoding.ASCII.GetBytes(IMEI);
                for (int i = 0; i < Math.Min(imeiBytes.Length, 15); i++)
                {
                    sum += imeiBytes[i];
                }
            }

            // 加上数据区（如果有）
            if (Data != null)
            {
                foreach (byte b in Data)
                {
                    sum += b;
                }
            }

            // 取低8位
            return (byte)(sum & 0xFF);
        }

        /// <summary>
        /// 从字节数组解析数据包
        /// </summary>
        /// <param name="data">要解析的字节数组</param>
        /// <returns>解析出的数据包，如果解析失败则返回null</returns>
        public static TcpPacket FromBytes(byte[] data)
        {
            // 验证数据长度至少包含基本结构
            if (data == null || data.Length < 7) // 帧头(2) + 长度(2) + 控制码(1) + 结果码(1) + 校验和(1)
            {
                return null;
            }

            // 验证帧头
            ushort startFlag = (ushort)((data[1] << 8) | data[0]);
            if (startFlag != START_FLAG)
            {
                return null;
            }

            // 解析长度
            ushort length = (ushort)((data[3] << 8) | data[2]);

            // 验证数据包完整性
            if (data.Length < length + 4) // +4是因为帧头和长度字段不包含在length中
            {
                return null;
            }

            // 创建数据包对象
            var packet = new TcpPacket
            {
                Length = length,
                ControlCode = data[4],
                Result = data[5]
            };

            int offset = 6;

            // 解析IMEI（如果有）
            // 根据协议，只有新版本协议（0x81命令回复0xF0后）才会在数据包中包含IMEI
            // 或者是0xC0命令（主动上报IMEI）
            bool hasImei = packet.ControlCode != ControlCodes.DEVICE_LOGIN && length > 3; // 3 = 控制码(1) + 结果码(1) + 校验和(1)

            if (hasImei && data.Length >= offset + 15) // 确保有足够的数据包含IMEI
            {
                packet.IMEI = Encoding.ASCII.GetString(data, offset, 15);
                offset += 15;
            }

            // 解析数据区
            int dataLength = length - (hasImei ? 18 : 3); // 18 = 控制码(1) + 结果码(1) + IMEI(15) + 校验和(1)
            if (dataLength > 0 && data.Length >= offset + dataLength)
            {
                packet.Data = new byte[dataLength];
                Array.Copy(data, offset, packet.Data, 0, dataLength);
                offset += dataLength;
            }
            else
            {
                packet.Data = Array.Empty<byte>();
            }

            // 获取校验和
            if (data.Length > offset)
            {
                packet.CheckSum = data[offset];

                // 验证校验和
                byte calculatedChecksum = packet.CalculateChecksum();
                if (calculatedChecksum != packet.CheckSum)
                {
                    // 校验和不匹配，但仍然返回数据包，由调用者决定如何处理
                    // 可以在日志中记录校验和不匹配的情况
                }
            }

            return packet;
        }

        #region 设备登录相关

        /// <summary>
        /// 设备登录信息类
        /// </summary>
        public class DeviceLoginInfo
        {
            /// <summary>
            /// 设备IMEI
            /// </summary>
            public string IMEI { get; set; } = string.Empty;

            /// <summary>
            /// 设备端口数
            /// </summary>
            public byte PortCount { get; set; }

            /// <summary>
            /// 硬件版本
            /// </summary>
            public string HardwareVersion { get; set; } = string.Empty;

            /// <summary>
            /// 软件版本
            /// </summary>
            public string SoftwareVersion { get; set; } = string.Empty;

            /// <summary>
            /// CCID号
            /// </summary>
            public string CCID { get; set; } = string.Empty;

            /// <summary>
            /// 协议版本
            /// </summary>
            public byte ProtocolVersion { get; set; }

            /// <summary>
            /// 登录原因
            /// </summary>
            public byte LoginReason { get; set; }

            /// <summary>
            /// 从数据包解析设备登录信息
            /// </summary>
            /// <param name="packet">数据包</param>
            /// <returns>设备登录信息</returns>
            public static DeviceLoginInfo FromPacket(TcpPacket packet)
            {
                if (packet == null || packet.ControlCode != ControlCodes.DEVICE_LOGIN || packet.Data == null || packet.Data.Length < 70)
                {
                    return null;
                }

                var info = new DeviceLoginInfo();

                // 解析IMEI（15字节ASCII码）
                info.IMEI = Encoding.ASCII.GetString(packet.Data, 0, 15);

                // 解析端口数
                info.PortCount = packet.Data[15];

                // 解析硬件版本（16字节ASCII码）
                info.HardwareVersion = Encoding.ASCII.GetString(packet.Data, 16, 16).TrimEnd('\0');

                // 解析软件版本（16字节ASCII码）
                info.SoftwareVersion = Encoding.ASCII.GetString(packet.Data, 32, 16).TrimEnd('\0');

                // 解析CCID（20字节ASCII码）
                info.CCID = Encoding.ASCII.GetString(packet.Data, 48, 20).TrimEnd('\0');

                // 解析协议版本（信号值）
                info.ProtocolVersion = packet.Data[68];

                // 解析登录原因
                if (packet.Data.Length > 69)
                {
                    info.LoginReason = packet.Data[69];
                }

                return info;
            }
        }

        /// <summary>
        /// 创建设备登录响应包
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <param name="heartbeatInterval">心跳间隔（10-250秒）</param>
        /// <param name="loginResult">登录结果（0x00正常登录成功，0xF0登录成功并切换到新协议格式）</param>
        /// <returns>登录响应数据包</returns>
        public static TcpPacket CreateLoginResponse(string imei, byte heartbeatInterval, byte loginResult = 0x00)
        {
            // 创建响应数据
            byte[] responseData = new byte[9]; // 7字节时间（预留） + 1字节心跳间隔 + 1字节登录结果

            // 时间字段预留，全部填0
            for (int i = 0; i < 7; i++)
            {
                responseData[i] = 0x00;
            }

            // 设置心跳间隔
            responseData[7] = heartbeatInterval;

            // 设置登录结果
            responseData[8] = loginResult;

            // 创建响应包
            // 注意：登录响应不带IMEI，因为设备还不知道是否要切换到新协议
            // 根据协议要求，Result字段应该设置为0x01（成功）
            return Create(ControlCodes.DEVICE_LOGIN, 0x01, null, responseData);
        }

        #endregion

        #region 心跳数据相关

        /// <summary>
        /// 心跳数据信息类
        /// </summary>
        public class HeartbeatInfo
        {
            /// <summary>
            /// 信号强度（0-32）
            /// </summary>
            public byte SignalStrength { get; set; }

            /// <summary>
            /// 设备温度
            /// </summary>
            public byte Temperature { get; set; }

            /// <summary>
            /// 总端口数
            /// </summary>
            public byte PortCount { get; set; }

            /// <summary>
            /// 端口状态数组
            /// </summary>
            public byte[] PortStatus { get; set; } = Array.Empty<byte>();

            /// <summary>
            /// 获取端口状态文本描述
            /// </summary>
            /// <param name="portIndex">端口索引（0开始）</param>
            /// <returns>状态描述</returns>
            public string GetPortStatusText(int portIndex)
            {
                if (portIndex < 0 || portIndex >= PortStatus.Length)
                {
                    return "无效端口";
                }

                switch (PortStatus[portIndex])
                {
                    case 0x00: return "空闲";
                    case 0x01: return "使用中";
                    case 0x02: return "保险丝熔断";
                    case 0x03: return "继电器粘连";
                    case 0x04: return "端口禁用";
                    default: return $"未知状态(0x{PortStatus[portIndex]:X2})";
                }
            }

            /// <summary>
            /// 从数据包解析心跳信息
            /// </summary>
            /// <param name="packet">数据包</param>
            /// <returns>心跳信息</returns>
            public static HeartbeatInfo FromPacket(TcpPacket packet)
            {
                if (packet == null || packet.ControlCode != ControlCodes.HEARTBEAT)
                {
                    return null;
                }

                var info = new HeartbeatInfo();
                int offset = 0;

                // 如果有IMEI，数据区第一个字节是信号强度
                // 如果没有IMEI，则需要从数据区解析
                if (string.IsNullOrEmpty(packet.IMEI) && packet.Data != null && packet.Data.Length >= 3)
                {
                    info.SignalStrength = packet.Data[offset++];
                    info.Temperature = packet.Data[offset++];
                    info.PortCount = packet.Data[offset++];

                    // 解析端口状态
                    int portStatusLength = packet.Data.Length - offset;
                    info.PortStatus = new byte[portStatusLength];
                    Array.Copy(packet.Data, offset, info.PortStatus, 0, portStatusLength);
                }
                else if (!string.IsNullOrEmpty(packet.IMEI) && packet.Data != null && packet.Data.Length >= 3)
                {
                    // 新协议格式，数据区包含信号强度、温度、端口数和端口状态
                    info.SignalStrength = packet.Data[offset++];
                    info.Temperature = packet.Data[offset++];
                    info.PortCount = packet.Data[offset++];

                    // 解析端口状态
                    int portStatusLength = packet.Data.Length - offset;
                    info.PortStatus = new byte[portStatusLength];
                    Array.Copy(packet.Data, offset, info.PortStatus, 0, portStatusLength);
                }

                return info;
            }
        }

        /// <summary>
        /// 创建心跳响应包
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <returns>心跳响应数据包</returns>
        public static TcpPacket CreateHeartbeatResponse(string imei)
        {
            // 心跳响应只需要一个预留字节
            byte[] responseData = new byte[1] { 0x00 };

            // 创建响应包
            // 根据协议要求，Result字段应该设置为0x01（成功）
            return Create(ControlCodes.HEARTBEAT, 0x01, imei, responseData);
        }

        #endregion

        #region 远程启动充电相关

        /// <summary>
        /// 创建远程启动充电命令包
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <param name="port">端口号</param>
        /// <param name="orderId">订单号</param>
        /// <param name="startMode">启动方式：1：扫码支付 2：刷卡支付 3：管理员启动</param>
        /// <param name="cardId">卡号（非IC卡启动时为0）</param>
        /// <param name="chargingMode">充电方式：1：充满自停 2：按金额 3：按时间 4：按电量 5：其它</param>
        /// <param name="chargingParam">充电参数（秒/0.01元/0.01度）</param>
        /// <param name="availableAmount">可用金额（0.01元）</param>
        /// <returns>远程启动充电命令包</returns>
        public static TcpPacket CreateStartChargingCommand(
            string imei,
            byte port,
            uint orderId,
            byte startMode = 1,
            uint cardId = 0,
            byte chargingMode = 1,
            uint chargingParam = 0,
            uint availableAmount = 0)
        {
            // 创建数据区
            byte[] data = new byte[19]; // 端口(1) + 订单号(4) + 启动方式(1) + 卡号(4) + 充电方式(1) + 充电参数(4) + 可用金额(4)

            // 端口号
            data[0] = port;

            // 订单号（低字节在前）
            data[1] = (byte)(orderId & 0xFF);
            data[2] = (byte)((orderId >> 8) & 0xFF);
            data[3] = (byte)((orderId >> 16) & 0xFF);
            data[4] = (byte)((orderId >> 24) & 0xFF);

            // 启动方式
            data[5] = startMode;

            // 卡号（低字节在前）
            data[6] = (byte)(cardId & 0xFF);
            data[7] = (byte)((cardId >> 8) & 0xFF);
            data[8] = (byte)((cardId >> 16) & 0xFF);
            data[9] = (byte)((cardId >> 24) & 0xFF);

            // 充电方式
            data[10] = chargingMode;

            // 充电参数（低字节在前）
            data[11] = (byte)(chargingParam & 0xFF);
            data[12] = (byte)((chargingParam >> 8) & 0xFF);
            data[13] = (byte)((chargingParam >> 16) & 0xFF);
            data[14] = (byte)((chargingParam >> 24) & 0xFF);

            // 可用金额（低字节在前）
            data[15] = (byte)(availableAmount & 0xFF);
            data[16] = (byte)((availableAmount >> 8) & 0xFF);
            data[17] = (byte)((availableAmount >> 16) & 0xFF);
            data[18] = (byte)((availableAmount >> 24) & 0xFF);

            // 创建数据包
            // 根据协议要求，Result字段应该设置为0x01（成功）
            return Create(ControlCodes.START_CHARGING, 0x01, imei, data);
        }

        /// <summary>
        /// 创建远程启动充电响应包
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <param name="port">端口号</param>
        /// <param name="orderId">订单号</param>
        /// <param name="startMode">启动方式</param>
        /// <param name="startResult">启动结果</param>
        /// <returns>响应数据包</returns>
        public static TcpPacket CreateStartChargingResponse(string imei, byte port, uint orderId, byte startMode, byte startResult)
        {
            // 创建数据区
            byte[] data = new byte[7]; // 端口(1) + 订单号(4) + 启动方式(1) + 启动结果(1)

            // 端口号
            data[0] = port;

            // 订单号（低字节在前）
            data[1] = (byte)(orderId & 0xFF);
            data[2] = (byte)((orderId >> 8) & 0xFF);
            data[3] = (byte)((orderId >> 16) & 0xFF);
            data[4] = (byte)((orderId >> 24) & 0xFF);

            // 启动方式
            data[5] = startMode;

            // 启动结果
            data[6] = startResult;

            // 创建数据包
            // 根据协议要求，Result字段应该设置为0x01（成功）
            return Create(ControlCodes.START_CHARGING, 0x01, imei, data);
        }

        #endregion

        #region 远程停止充电相关

        /// <summary>
        /// 创建远程停止充电命令包
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <param name="port">端口号</param>
        /// <param name="orderId">订单号</param>
        /// <returns>远程停止充电命令包</returns>
        public static TcpPacket CreateStopChargingCommand(string imei, byte port, uint orderId)
        {
            // 创建数据区
            byte[] data = new byte[5]; // 端口(1) + 订单号(4)

            // 端口号
            data[0] = port;

            // 订单号（低字节在前）
            data[1] = (byte)(orderId & 0xFF);
            data[2] = (byte)((orderId >> 8) & 0xFF);
            data[3] = (byte)((orderId >> 16) & 0xFF);
            data[4] = (byte)((orderId >> 24) & 0xFF);

            // 创建数据包
            // 根据协议要求，Result字段应该设置为0x01（成功）
            return Create(ControlCodes.STOP_CHARGING, 0x01, imei, data);
        }

        /// <summary>
        /// 创建远程停止充电响应包
        /// </summary>
        /// <param name="imei">设备IMEI</param>
        /// <param name="port">端口号</param>
        /// <param name="orderId">订单号</param>
        /// <param name="result">结果：0-执行成功，1-端口本来就空闲，2-订单号不匹配</param>
        /// <returns>响应数据包</returns>
        public static TcpPacket CreateStopChargingResponse(string imei, byte port, uint orderId, byte result)
        {
            // 创建数据区
            byte[] data = new byte[6]; // 端口(1) + 订单号(4) + 结果(1)

            // 端口号
            data[0] = port;

            // 订单号（低字节在前）
            data[1] = (byte)(orderId & 0xFF);
            data[2] = (byte)((orderId >> 8) & 0xFF);
            data[3] = (byte)((orderId >> 16) & 0xFF);
            data[4] = (byte)((orderId >> 24) & 0xFF);

            // 结果
            data[5] = result;

            // 创建数据包
            // 根据协议要求，Result字段应该设置为0x01（成功）
            return Create(ControlCodes.STOP_CHARGING, 0x01, imei, data);
        }

        #endregion
    }
}