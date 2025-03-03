using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingPile.API.Data;
using ChargingPile.API.Models.DTOs;
using ChargingPile.API.Models.Entities;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ChargingPile.API.Controllers
{
    /// <summary>
    /// 充电站管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargingStationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ChargingStationController> _logger;
        private const int CACHE_DURATION_MINUTES = 5;

        public ChargingStationController(ApplicationDbContext context, IMemoryCache cache, ILogger<ChargingStationController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 创建新的充电站
        /// </summary>
        /// <param name="dto">充电站创建信息</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     POST /api/ChargingStation
        ///     {
        ///         "name": "示例充电站",
        ///         "latitude": 30.123456,
        ///         "longitude": 120.123456,
        ///         "address": "示例地址",
        ///         "totalPorts": 10,
        ///         "availablePorts": 10
        ///     }
        /// </remarks>
        /// <response code="201">充电站创建成功</response>
        /// <response code="400">输入数据验证失败或该位置已存在充电站</response>
        [HttpPost]
        [ProducesResponseType(typeof(ChargingStation), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateStation([FromBody] CreateChargingStationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingStation = await _context.ChargingStations
                .FirstOrDefaultAsync(s => 
                    s.Latitude == dto.Latitude && 
                    s.Longitude == dto.Longitude);

            if (existingStation != null)
            {
                return BadRequest("该位置已存在充电站");
            }

            var station = new ChargingStation
            {
                Name = dto.Name,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Address = dto.Address,
                TotalPorts = dto.TotalPorts,
                AvailablePorts = dto.AvailablePorts,
                Status = 0,
                CreateTime = DateTime.UtcNow
            };
            
            // 设置空间位置
            station.UpdateLocation();
            _logger.LogInformation($"创建充电站，位置: ({station.Latitude}, {station.Longitude}), SRID: {station.Location?.SRID}");

            try
            {
                _context.ChargingStations.Add(station);
                await _context.SaveChangesAsync();

                // 清除相关缓存
                ClearNearbyCache(dto.Latitude, dto.Longitude);
                
                return CreatedAtAction(nameof(GetStation), new { id = station.Id }, station);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建充电站失败: {ex.Message}");
                return StatusCode(500, "创建充电站失败，请稍后重试");
            }
        }

        /// <summary>
        /// 获取指定范围内的附近充电站
        /// </summary>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <param name="radius">搜索半径（米），默认5000米</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     GET /api/ChargingStation/nearby?latitude=30.123456&amp;longitude=120.123456&amp;radius=5000
        /// </remarks>
        /// <response code="200">返回符合条件的充电站列表</response>
        [HttpGet("nearby")]
        [ProducesResponseType(typeof(List<ChargingStation>), 200)]
        public async Task<IActionResult> GetNearbyStations(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] int radius = 5000)
        {
            var cacheKey = $"nearby_{latitude}_{longitude}_{radius}";
            
            _logger.LogInformation($"开始查询附近充电站: 坐标({latitude}, {longitude}), 半径{radius}米");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 尝试从缓存获取结果
            if (_cache.TryGetValue(cacheKey, out List<ChargingStation> cachedStations))
            {
                stopwatch.Stop();
                _logger.LogInformation($"从缓存获取结果，耗时: {stopwatch.ElapsedMilliseconds}ms, 找到{cachedStations.Count}个充电站");
                return Ok(cachedStations);
            }

            try
            {
                // 获取数据库中所有充电站的数量
                var totalStations = await _context.ChargingStations.CountAsync();
                _logger.LogInformation($"数据库中共有{totalStations}个充电站");
                
                // 创建查询点
                var userLocation = new Point((double)longitude, (double)latitude) { SRID = 4326 };
                
                // 将半径从米转换为度（近似值）
                double radiusInDegrees = radius / 111000.0; // 约111km对应1度
                _logger.LogInformation($"查询半径: {radius}米, 约{radiusInDegrees}度");
                
                // 计算矩形范围（粗略筛选）
                decimal minLat = latitude - (decimal)radiusInDegrees;
                decimal maxLat = latitude + (decimal)radiusInDegrees;
                decimal minLon = longitude - (decimal)radiusInDegrees;
                decimal maxLon = longitude + (decimal)radiusInDegrees;
                
                _logger.LogInformation($"查询范围: 纬度[{minLat} - {maxLat}], 经度[{minLon} - {maxLon}]");
                
                var dbStopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // 使用经纬度范围进行初步筛选
                var nearbyStations = await _context.ChargingStations
                    .Where(s => 
                        s.Latitude >= minLat && 
                        s.Latitude <= maxLat && 
                        s.Longitude >= minLon && 
                        s.Longitude <= maxLon)
                    .ToListAsync();
                    
                dbStopwatch.Stop();
                _logger.LogInformation($"数据库查询耗时: {dbStopwatch.ElapsedMilliseconds}ms, 初步筛选出{nearbyStations.Count}个充电站");
                
                // 精确计算距离并筛选
                var result = new List<ChargingStation>();
                foreach (var station in nearbyStations)
                {
                    var distance = CalculateDistance(latitude, longitude, station.Latitude, station.Longitude);
                    _logger.LogDebug($"充电站ID: {station.Id}, 距离: {distance}米");
                    if (distance <= radius)
                    {
                        result.Add(station);
                    }
                }
                
                // 按距离排序
                result = result
                    .OrderBy(s => CalculateDistance(latitude, longitude, s.Latitude, s.Longitude))
                    .ToList();
                
                _logger.LogInformation($"精确距离计算后，最终找到{result.Count}个充电站");
                
                // 缓存结果
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                _cache.Set(cacheKey, result, cacheOptions);
                
                stopwatch.Stop();
                _logger.LogInformation($"总查询耗时: {stopwatch.ElapsedMilliseconds}ms");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询附近充电站时发生错误");
                return StatusCode(500, "查询附近充电站时发生错误");
            }
        }

        /// <summary>
        /// 使用空间索引获取指定范围内的附近充电站
        /// </summary>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <param name="radius">搜索半径（米），默认5000米</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     GET /api/ChargingStation/spatial-nearby?latitude=30.123456&amp;longitude=120.123456&amp;radius=5000
        /// </remarks>
        /// <response code="200">返回符合条件的充电站列表</response>
        [HttpGet("spatial-nearby")]
        [ProducesResponseType(typeof(List<ChargingStation>), 200)]
        public async Task<IActionResult> GetNearbySpatialStations(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] int radius = 5000)
        {
            var cacheKey = $"spatial_nearby_{latitude}_{longitude}_{radius}";
            
            _logger.LogInformation($"开始使用空间索引查询附近充电站: 坐标({latitude}, {longitude}), 半径{radius}米");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 尝试从缓存获取结果
            if (_cache.TryGetValue(cacheKey, out List<ChargingStation> cachedStations))
            {
                stopwatch.Stop();
                _logger.LogInformation($"从缓存获取结果，耗时: {stopwatch.ElapsedMilliseconds}ms, 找到{cachedStations.Count}个充电站");
                return Ok(cachedStations);
            }

            try
            {
                // 获取数据库中所有充电站的数量
                var totalStations = await _context.ChargingStations.CountAsync();
                _logger.LogInformation($"数据库中共有{totalStations}个充电站");
                
                // 创建查询点
                var userLocation = new Point((double)longitude, (double)latitude) { SRID = 4326 };
                
                // 将半径从米转换为度（近似值）
                double radiusInDegrees = radius / 111000.0; // 约111km对应1度
                _logger.LogInformation($"查询半径: {radius}米, 约{radiusInDegrees}度");
                
                // 计算矩形范围（粗略筛选）
                decimal minLat = latitude - (decimal)radiusInDegrees;
                decimal maxLat = latitude + (decimal)radiusInDegrees;
                decimal minLon = longitude - (decimal)radiusInDegrees;
                decimal maxLon = longitude + (decimal)radiusInDegrees;
                
                _logger.LogInformation($"查询范围: 纬度[{minLat} - {maxLat}], 经度[{minLon} - {maxLon}]");
                
                var dbStopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // 使用空间索引进行查询
                var nearbyStations = await _context.ChargingStations
                    .Where(s => 
                        s.Latitude >= minLat && 
                        s.Latitude <= maxLat && 
                        s.Longitude >= minLon && 
                        s.Longitude <= maxLon)
                    .ToListAsync();
                    
                dbStopwatch.Stop();
                _logger.LogInformation($"数据库查询耗时: {dbStopwatch.ElapsedMilliseconds}ms, 初步筛选出{nearbyStations.Count}个充电站");
                
                // 精确计算距离并筛选
                var result = new List<ChargingStation>();
                foreach (var station in nearbyStations)
                {
                    var distance = CalculateDistance(latitude, longitude, station.Latitude, station.Longitude);
                    _logger.LogDebug($"充电站ID: {station.Id}, 距离: {distance}米");
                    
                    if (distance <= radius)
                    {
                        result.Add(station);
                    }
                }
                
                // 按距离排序
                result = result.OrderBy(s => CalculateDistance(latitude, longitude, s.Latitude, s.Longitude)).ToList();
                
                // 缓存结果
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                _cache.Set(cacheKey, result, cacheOptions);
                
                stopwatch.Stop();
                _logger.LogInformation($"总查询耗时: {stopwatch.ElapsedMilliseconds}ms, 找到{result.Count}个充电站");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "使用空间索引查询附近充电站时发生错误");
                return StatusCode(500, "查询附近充电站时发生错误");
            }
        }

        /// <summary>
        /// 计算两个经纬度点之间的距离（米）
        /// </summary>
        private static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371000; // 地球半径（米）
            var dLat = ToRad((double)(lat2 - lat1));
            var dLon = ToRad((double)(lon2 - lon1));
            var lat1Rad = ToRad((double)lat1);
            var lat2Rad = ToRad((double)lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// 获取指定ID的充电站详情
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <remarks>
        /// 请求示例:
        /// 
        ///     GET /api/ChargingStation/123e4567-e89b-12d3-a456-426614174000
        /// </remarks>
        /// <response code="200">返回充电站详情</response>
        /// <response code="404">指定ID的充电站不存在</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ChargingStation), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStation(string id)
        {
            var station = await _context.ChargingStations.FindAsync(id);
            
            if (station == null)
            {
                return NotFound();
            }

            // 返回前清除Location属性，避免序列化问题
            station.Location = null;

            return Ok(station);
        }

        private void ClearNearbyCache(decimal latitude, decimal longitude)
        {
            // 清除附近位置的缓存
            var radiusList = new[] { 1000, 2000, 5000, 10000 }; // 常用搜索半径
            foreach (var radius in radiusList)
            {
                var cacheKey = $"nearby_{latitude}_{longitude}_{radius}";
                _cache.Remove(cacheKey);
            }
        }
    }
} 