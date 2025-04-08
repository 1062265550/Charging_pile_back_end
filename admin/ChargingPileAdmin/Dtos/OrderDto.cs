using System;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 订单数据传输对象
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// 订单唯一标识符
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; } = null!;

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? UserNickname { get; set; }

        /// <summary>
        /// 充电桩ID
        /// </summary>
        public string PileId { get; set; } = null!;

        /// <summary>
        /// 充电桩编号
        /// </summary>
        public string? PileNo { get; set; }

        /// <summary>
        /// 充电端口ID
        /// </summary>
        public string PortId { get; set; } = null!;

        /// <summary>
        /// 充电端口编号
        /// </summary>
        public string? PortNo { get; set; }

        /// <summary>
        /// 订单状态：0-待支付，1-充电中，2-已完成，3-已取消，4-异常
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 订单状态描述
        /// </summary>
        public string StatusDescription => Status switch
        {
            0 => "待支付",
            1 => "充电中",
            2 => "已完成",
            3 => "已取消",
            4 => "异常",
            _ => "未知"
        };

        /// <summary>
        /// 计费模式：0-按时间，1-电量+电量服务费，2-电量+时间服务费
        /// </summary>
        public short? BillingMode { get; set; }

        /// <summary>
        /// 计费模式描述
        /// </summary>
        public string BillingModeDescription => BillingMode switch
        {
            0 => "按时间",
            1 => "电量+电量服务费",
            2 => "电量+时间服务费",
            _ => "未知"
        };

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 充电时长(秒)
        /// </summary>
        public int? ChargingTime { get; set; }

        /// <summary>
        /// 充电时长(格式化)
        /// </summary>
        public string ChargingTimeFormatted
        {
            get
            {
                if (!ChargingTime.HasValue || ChargingTime.Value <= 0)
                {
                    return "0分钟";
                }

                var hours = ChargingTime.Value / 3600;
                var minutes = (ChargingTime.Value % 3600) / 60;
                var seconds = ChargingTime.Value % 60;

                if (hours > 0)
                {
                    return $"{hours}小时{minutes}分钟{seconds}秒";
                }
                else if (minutes > 0)
                {
                    return $"{minutes}分钟{seconds}秒";
                }
                else
                {
                    return $"{seconds}秒";
                }
            }
        }

        /// <summary>
        /// 充电电量(kWh)
        /// </summary>
        public decimal? PowerConsumption { get; set; }

        /// <summary>
        /// 充电金额(元)
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 服务费(元)
        /// </summary>
        public decimal? ServiceFee { get; set; }

        /// <summary>
        /// 总金额(元)
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 支付状态：0-未支付，1-已支付，2-已退款
        /// </summary>
        public int? PaymentStatus { get; set; }

        /// <summary>
        /// 支付状态描述
        /// </summary>
        public string PaymentStatusDescription => PaymentStatus switch
        {
            0 => "未支付",
            1 => "已支付",
            2 => "已退款",
            _ => "未知"
        };

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-余额
        /// </summary>
        public int? PaymentMethod { get; set; }

        /// <summary>
        /// 支付方式描述
        /// </summary>
        public string PaymentMethodDescription => PaymentMethod switch
        {
            1 => "微信",
            2 => "支付宝",
            3 => "余额",
            _ => "未知"
        };

        /// <summary>
        /// 交易流水号
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// 充电功率(kW)
        /// </summary>
        public decimal? Power { get; set; }

        /// <summary>
        /// 充电模式：1-充满自停，2-按金额，3-按时间，4-按电量，5-其他
        /// </summary>
        public int? ChargingMode { get; set; }

        /// <summary>
        /// 充电模式描述
        /// </summary>
        public string ChargingModeDescription => ChargingMode switch
        {
            1 => "充满自停",
            2 => "按金额",
            3 => "按时间",
            4 => "按电量",
            5 => "其他",
            _ => "未知"
        };

        /// <summary>
        /// 停止原因：0-充满自停，1-时间用完，2-金额用完，3-手动停止，4-电量用完
        /// </summary>
        public short? StopReason { get; set; }

        /// <summary>
        /// 停止原因描述
        /// </summary>
        public string StopReasonDescription => StopReason switch
        {
            0 => "充满自停",
            1 => "时间用完",
            2 => "金额用完",
            3 => "手动停止",
            4 => "电量用完",
            _ => "未知"
        };

        /// <summary>
        /// 尖峰时段电量(kWh)
        /// </summary>
        public decimal? SharpElectricity { get; set; }

        /// <summary>
        /// 尖峰时段金额(元)
        /// </summary>
        public decimal? SharpAmount { get; set; }

        /// <summary>
        /// 峰时段电量(kWh)
        /// </summary>
        public decimal? PeakElectricity { get; set; }

        /// <summary>
        /// 峰时段金额(元)
        /// </summary>
        public decimal? PeakAmount { get; set; }

        /// <summary>
        /// 平时段电量(kWh)
        /// </summary>
        public decimal? FlatElectricity { get; set; }

        /// <summary>
        /// 平时段金额(元)
        /// </summary>
        public decimal? FlatAmount { get; set; }

        /// <summary>
        /// 谷时段电量(kWh)
        /// </summary>
        public decimal? ValleyElectricity { get; set; }

        /// <summary>
        /// 谷时段金额(元)
        /// </summary>
        public decimal? ValleyAmount { get; set; }

        /// <summary>
        /// 深谷时段电量(kWh)
        /// </summary>
        public decimal? DeepValleyElectricity { get; set; }

        /// <summary>
        /// 深谷时段金额(元)
        /// </summary>
        public decimal? DeepValleyAmount { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 创建订单请求
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 充电桩ID
        /// </summary>
        public string PileId { get; set; } = null!;

        /// <summary>
        /// 充电端口ID
        /// </summary>
        public string PortId { get; set; } = null!;

        /// <summary>
        /// 计费模式：0-按时间，1-电量+电量服务费，2-电量+时间服务费
        /// </summary>
        public short BillingMode { get; set; }

        /// <summary>
        /// 充电模式：1-充满自停，2-按金额，3-按时间，4-按电量，5-其他
        /// </summary>
        public int ChargingMode { get; set; }
    }

    /// <summary>
    /// 更新订单状态请求
    /// </summary>
    public class UpdateOrderStatusDto
    {
        /// <summary>
        /// 订单状态：0-待支付，1-充电中，2-已完成，3-已取消，4-异常
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 停止原因：0-充满自停，1-时间用完，2-金额用完，3-手动停止，4-电量用完
        /// </summary>
        public short? StopReason { get; set; }
    }

    /// <summary>
    /// 订单支付请求
    /// </summary>
    public class OrderPaymentDto
    {
        /// <summary>
        /// 支付方式：1-微信，2-支付宝，3-余额
        /// </summary>
        public int PaymentMethod { get; set; }

        /// <summary>
        /// 交易流水号
        /// </summary>
        public string TransactionId { get; set; } = null!;
    }

    /// <summary>
    /// 订单统计数据
    /// </summary>
    public class OrderStatisticsDto
    {
        /// <summary>
        /// 总订单数
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// 总充电电量(kWh)
        /// </summary>
        public decimal TotalPowerConsumption { get; set; }

        /// <summary>
        /// 总充电时长(秒)
        /// </summary>
        public int TotalChargingTime { get; set; }

        /// <summary>
        /// 总充电金额(元)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 待支付订单数
        /// </summary>
        public int PendingPaymentOrders { get; set; }

        /// <summary>
        /// 已支付待充电订单数
        /// </summary>
        public int PendingChargingOrders { get; set; }

        /// <summary>
        /// 充电中订单数
        /// </summary>
        public int ChargingOrders { get; set; }

        /// <summary>
        /// 已完成订单数
        /// </summary>
        public int CompletedOrders { get; set; }

        /// <summary>
        /// 已取消订单数
        /// </summary>
        public int CancelledOrders { get; set; }

        /// <summary>
        /// 异常订单数
        /// </summary>
        public int AbnormalOrders { get; set; }
    }
}
