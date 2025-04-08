using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;

namespace ChargingPile.API.Data
{
    /// <summary>
    /// 空间扩展类，提供地理位置相关的扩展方法
    /// </summary>
    public static class SpatialExtensions
    {
        /// <summary>
        /// 创建地理点
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>SQL Server地理点对象</returns>
        public static SqlGeography CreatePoint(double longitude, double latitude)
        {
            // 创建SQL Server地理点，使用WGS84坐标系(SRID=4326)
            return SqlGeography.Point(latitude, longitude, 4326);
        }
    }
}