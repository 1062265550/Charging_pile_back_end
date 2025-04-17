using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Models;
using ChargingPileAdmin.Repositories;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 充电站服务实现
    /// </summary>
    public class ChargingStationService : IChargingStationService
    {
        private readonly IChargingStationRepository _stationRepository;

        public ChargingStationService(IChargingStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        /// <summary>
        /// 获取所有充电站
        /// </summary>
        /// <returns>充电站列表</returns>
        public async Task<IEnumerable<ChargingStationDto>> GetAllStationsAsync()
        {
            var stationsWithPileCount = await _stationRepository.GetStationsWithPileCountAsync();
            return stationsWithPileCount.Select(item => MapToDto(item.Station, item.PileCount));
        }

        /// <summary>
        /// 获取充电站详情
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <returns>充电站详情</returns>
        public async Task<ChargingStationDto> GetStationByIdAsync(string id)
        {
            var station = await _stationRepository.GetStationWithPilesAsync(id);
            if (station == null)
            {
                return null;
            }

            return MapToDto(station, station.ChargingPiles?.Count ?? 0);
        }

        /// <summary>
        /// 获取充电站分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="status">充电站状态，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>充电站分页列表</returns>
        public async Task<PagedResponseDto<ChargingStationDto>> GetStationsPagedAsync(int pageNumber, int pageSize, int? status = null, string keyword = null)
        {
            // 构建查询条件
            System.Linq.Expressions.Expression<Func<ChargingStation, bool>> filter = s => true;

            if (status.HasValue)
            {
                filter = s => s.Status == status.Value;
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = s => s.Name.Contains(keyword) || s.Address.Contains(keyword);
            }

            // 获取分页数据
            var (stations, totalCount, totalPages) = await _stationRepository.GetPagedAsync(
                filter,
                pageNumber,
                pageSize,
                query => query.OrderByDescending(s => s.UpdateTime)
            );

            // 获取每个充电站的充电桩数量
            var result = new List<ChargingStationDto>();
            foreach (var station in stations)
            {
                var pileCount = await _stationRepository.GetStationWithPilesAsync(station.Id);
                result.Add(MapToDto(station, pileCount.ChargingPiles?.Count ?? 0));
            }

            // 返回分页响应
            return new PagedResponseDto<ChargingStationDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = result
            };
        }

        /// <summary>
        /// 创建充电站
        /// </summary>
        /// <param name="dto">创建充电站请求</param>
        /// <returns>创建的充电站ID</returns>
        public async Task<string> CreateStationAsync(CreateChargingStationDto dto)
        {
            // 检查名称是否已存在
            if (await _stationRepository.IsNameExistsAsync(dto.Name))
            {
                throw new Exception($"充电站名称 '{dto.Name}' 已存在");
            }

            // 处理Location字段，创建NetTopologySuite的Point对象
            Point locationPoint = null;
            if (!string.IsNullOrEmpty(dto.Location))
            {
                // 尝试解析前端传入的经纬度格式
                var parts = dto.Location.Split(',');
                if (parts.Length == 2 && double.TryParse(parts[0], out double longitude) && double.TryParse(parts[1], out double latitude))
                {
                    // 创建Point对象，注意NetTopologySuite中Point的构造函数是(X, Y)，对应(经度, 纬度)
                    // SRID 4326 是WGS84坐标系，这是最常用的GPS坐标系统
                    locationPoint = new Point(longitude, latitude) { SRID = 4326 };
                }
                else
                {
                    // 如果无法解析为有效的经纬度，可以抛出异常或使用空值
                    throw new Exception("无效的位置格式，请使用'经度,纬度'格式，例如: '116.404,39.915'");
                }
            }

            // 创建充电站
            var station = new ChargingStation
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Status = dto.Status,
                Address = dto.Address,
                Location = locationPoint,
                Description = dto.Description,
                UpdateTime = DateTime.Now
            };

            await _stationRepository.AddAsync(station);
            await _stationRepository.SaveChangesAsync();

            return station.Id;
        }

        /// <summary>
        /// 更新充电站
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <param name="dto">更新充电站请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateStationAsync(string id, UpdateChargingStationDto dto)
        {
            // 检查充电站是否存在
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
            {
                return false;
            }

            // 检查名称是否已存在
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != station.Name)
            {
                if (await _stationRepository.IsNameExistsAsync(dto.Name, id))
                {
                    throw new Exception($"充电站名称 '{dto.Name}' 已存在");
                }
            }

            // 处理Location字段，确保为有效的WKT格式
            Point locationPoint = station.Location; // 默认保持原值
            if (dto.Location != null) // 如果提供了新的位置信息
            {
                if (string.IsNullOrEmpty(dto.Location))
                {
                    // 如果明确设置为空字符串，使用空点
                    locationPoint = null;
                }
                else
                {
                    // 尝试解析前端传入的经纬度格式
                    var parts = dto.Location.Split(',');
                    if (parts.Length == 2 && double.TryParse(parts[0], out double longitude) && double.TryParse(parts[1], out double latitude))
                    {
                        // 创建Point对象，注意NetTopologySuite中Point的构造函数是(X, Y)，对应(经度, 纬度)
                        // SRID 4326 是WGS84坐标系，这是最常用的GPS坐标系统
                        locationPoint = new Point(longitude, latitude) { SRID = 4326 };
                    }
                    else
                    {
                        // 如果无法解析为有效的经纬度，抛出异常
                        throw new Exception("无效的位置格式，请使用'经度,纬度'格式，例如: '116.404,39.915'");
                    }
                }
            }

            // 更新充电站
            station.Name = dto.Name ?? station.Name;
            station.Status = dto.Status;
            station.Address = dto.Address ?? station.Address;
            station.Location = locationPoint;
            station.Description = dto.Description ?? station.Description;
            station.UpdateTime = DateTime.Now;

            _stationRepository.Update(station);
            await _stationRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除充电站
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteStationAsync(string id)
        {
            // 检查充电站是否存在
            var station = await _stationRepository.GetStationWithPilesAsync(id);
            if (station == null)
            {
                return false;
            }

            // 检查充电站是否有充电桩
            if (station.ChargingPiles != null && station.ChargingPiles.Any())
            {
                throw new Exception("充电站下存在充电桩，无法删除");
            }

            // 删除充电站
            await _stationRepository.DeleteAsync(id);
            await _stationRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 将充电站实体映射为DTO
        /// </summary>
        /// <param name="station">充电站实体</param>
        /// <param name="pileCount">充电桩数量</param>
        /// <returns>充电站DTO</returns>
        private ChargingStationDto MapToDto(ChargingStation station, int pileCount)
        {
            // 将Point对象转换为人类可读的经纬度格式
            string locationText = null;
            if (station != null && station.Location != null && !station.Location.IsEmpty)
            {
                // NetTopologySuite中Point的X对应经度，Y对应纬度
                locationText = $"{station.Location.X},{station.Location.Y}";
            }

            return new ChargingStationDto
            {
                Id = station.Id,
                Name = station.Name,
                Status = station.Status,
                Address = station.Address,
                Location = locationText, // 使用转换后的文本
                Description = station.Description,
                UpdateTime = station.UpdateTime,
                PileCount = pileCount
            };
        }
    }
}
