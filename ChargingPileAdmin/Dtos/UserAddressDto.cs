using System;

namespace ChargingPileAdmin.Dtos
{
    /// <summary>
    /// 用户地址数据传输对象
    /// </summary>
    public class UserAddressDto
    {
        /// <summary>
        /// 地址唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        public string RecipientPhone { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 区县
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// 完整地址
        /// </summary>
        public string FullAddress => $"{Province}{City}{District}{DetailAddress}";

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 标签：家、公司、学校等
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 创建用户地址请求
    /// </summary>
    public class CreateUserAddressDto
    {
        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        public string RecipientPhone { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 区县
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 标签：家、公司、学校等
        /// </summary>
        public string Tag { get; set; }
    }

    /// <summary>
    /// 更新用户地址请求
    /// </summary>
    public class UpdateUserAddressDto
    {
        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// 收件人电话
        /// </summary>
        public string RecipientPhone { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 区县
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 标签：家、公司、学校等
        /// </summary>
        public string Tag { get; set; }
    }

    /// <summary>
    /// 设置默认地址请求
    /// </summary>
    public class SetDefaultAddressDto
    {
        /// <summary>
        /// 地址ID
        /// </summary>
        public int AddressId { get; set; }
    }
}
