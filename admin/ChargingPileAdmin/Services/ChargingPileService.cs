using ChargingPileAdmin.Dtos;
using ChargingPileAdmin.Models;
using ChargingPileAdmin.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 充电桩服务实现
    /// </summary>
    public class ChargingPileService : IChargingPileService
    {
        private readonly IChargingPileRepository _pileRepository;
        private readonly IChargingStationRepository _stationRepository;

        public ChargingPileService(
            IChargingPileRepository pileRepository,
            IChargingStationRepository stationRepository)
        {
            _pileRepository = pileRepository;
            _stationRepository = stationRepository;
        }

        /// <summary>
        /// 获取所有充电桩
        /// </summary>
        /// <returns>充电桩列表</returns>
        public async Task<IEnumerable<ChargingPileDto>> GetAllPilesAsync()
        {
            var piles = await _pileRepository.GetAsync(
                p => true,
                p => p.ChargingStation);

            return piles.Select(MapToDto);
        }

        /// <summary>
        /// 获取充电桩详情
        /// </summary>
        /// <param name="id">充电桩ID</param>
        /// <returns>充电桩详情</returns>
        public async Task<ChargingPileDto> GetPileByIdAsync(string id)
        {
            var pile = await _pileRepository.GetPileWithStationAsync(id);
            if (pile == null)
            {
                return null;
            }

            return MapToDto(pile);
        }

        /// <summary>
        /// 根据充电站ID获取充电桩列表
        /// </summary>
        /// <param name="stationId">充电站ID</param>
        /// <returns>充电桩列表</returns>
        public async Task<IEnumerable<ChargingPileDto>> GetPilesByStationIdAsync(string stationId)
        {
            var piles = await _pileRepository.GetAsync(
                p => p.StationId == stationId,
                p => p.ChargingStation);

            return piles.Select(MapToDto);
        }

        /// <summary>
        /// 获取充电桩分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="stationId">充电站ID，可选</param>
        /// <param name="status">充电桩状态，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>充电桩分页列表</returns>
        public async Task<PagedResponseDto<ChargingPileDto>> GetPilesPagedAsync(int pageNumber, int pageSize, string stationId = null, int? status = null, string keyword = null)
        {
            // 构建查询条件
            Expression<Func<ChargingPile, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(stationId))
            {
                filter = p => p.StationId == stationId;
            }

            if (status.HasValue)
            {
                filter = p => p.Status == status.Value;
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = p => p.PileNo.Contains(keyword) || p.Manufacturer.Contains(keyword);
            }

            // 获取分页数据
            var (piles, totalCount, totalPages) = await _pileRepository.GetPagedAsync(
                filter,
                pageNumber,
                pageSize,
                query => query.OrderByDescending(p => p.UpdateTime),
                p => p.ChargingStation
            );

            // 返回分页响应
            return new PagedResponseDto<ChargingPileDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = piles.Select(MapToDto).ToList()
            };
        }

        /// <summary>
        /// 创建充电桩
        /// </summary>
        /// <param name="dto">创建充电桩请求</param>
        /// <returns>创建的充电桩ID</returns>
        public async Task<string> CreatePileAsync(CreateChargingPileDto dto)
        {
            // 检查充电站是否存在
            var station = await _stationRepository.GetByIdAsync(dto.StationId);
            if (station == null)
            {
                throw new Exception($"充电站ID '{dto.StationId}' 不存在");
            }

            // 检查充电桩编号是否已存在
            if (await _pileRepository.IsPileNoExistsAsync(dto.PileNo))
            {
                throw new Exception($"充电桩编号 '{dto.PileNo}' 已存在");
            }

            // 创建充电桩
            var pile = new ChargingPile
            {
                Id = Guid.NewGuid().ToString(),
                StationId = dto.StationId,
                PileNo = dto.PileNo,
                PileType = dto.PileType,
                Status = dto.Status, // 使用前端传入的状态值
                PowerRate = dto.PowerRate,
                Manufacturer = dto.Manufacturer,
                IMEI = dto.IMEI,
                TotalPorts = dto.TotalPorts,
                ProtocolVersion = dto.ProtocolVersion,
                SoftwareVersion = dto.SoftwareVersion,
                HardwareVersion = dto.HardwareVersion,
                OnlineStatus = 0, // 初始在线状态为离线
                InstallationDate = dto.InstallationDate,
                UpdateTime = DateTime.Now
            };

            await _pileRepository.AddAsync(pile);
            await _pileRepository.SaveChangesAsync();

            return pile.Id;
        }

        /// <summary>
        /// 更新充电桩
        /// </summary>
        /// <param name="id">充电桩ID</param>
        /// <param name="dto">更新充电桩请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdatePileAsync(string id, UpdateChargingPileDto dto)
        {
            // 检查充电桩是否存在
            var pile = await _pileRepository.GetByIdAsync(id);
            if (pile == null)
            {
                return false;
            }

            // 检查充电桩编号是否已存在
            if (!string.IsNullOrEmpty(dto.PileNo) && dto.PileNo != pile.PileNo)
            {
                if (await _pileRepository.IsPileNoExistsAsync(dto.PileNo, id))
                {
                    throw new Exception($"充电桩编号 '{dto.PileNo}' 已存在");
                }
            }

            // 更新充电桩
            pile.PileNo = dto.PileNo ?? pile.PileNo;
            pile.PileType = dto.PileType;
            pile.PowerRate = dto.PowerRate;
            pile.Manufacturer = dto.Manufacturer ?? pile.Manufacturer;
            pile.IMEI = dto.IMEI ?? pile.IMEI;
            pile.TotalPorts = dto.TotalPorts;
            pile.ProtocolVersion = dto.ProtocolVersion ?? pile.ProtocolVersion;
            pile.SoftwareVersion = dto.SoftwareVersion ?? pile.SoftwareVersion;
            pile.HardwareVersion = dto.HardwareVersion ?? pile.HardwareVersion;
            // 添加对状态字段的更新
            if (pile.Status != dto.Status)
            {
                pile.Status = dto.Status;
            }
            pile.InstallationDate = dto.InstallationDate ?? pile.InstallationDate;
            pile.LastMaintenanceDate = dto.LastMaintenanceDate ?? pile.LastMaintenanceDate;
            pile.UpdateTime = DateTime.Now;

            _pileRepository.Update(pile);
            await _pileRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除充电桩
        /// </summary>
        /// <param name="id">充电桩ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeletePileAsync(string id)
        {
            // 检查充电桩是否存在
            var pile = await _pileRepository.GetPileWithPortsAsync(id);
            if (pile == null)
            {
                return false;
            }

            // 检查充电桩是否有充电端口
            if (pile.ChargingPorts != null && pile.ChargingPorts.Any())
            {
                throw new Exception("充电桩下存在充电端口，无法删除");
            }

            // 删除充电桩
            await _pileRepository.DeleteAsync(id);
            await _pileRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 将充电桩实体映射为DTO
        /// </summary>
        /// <param name="pile">充电桩实体</param>
        /// <returns>充电桩DTO</returns>
        private ChargingPileDto MapToDto(ChargingPile pile)
        {
            return new ChargingPileDto
            {
                Id = pile.Id,
                StationId = pile.StationId,
                StationName = pile.ChargingStation?.Name,
                PileNo = pile.PileNo,
                PileType = pile.PileType,
                Status = pile.Status,
                PowerRate = pile.PowerRate,
                Manufacturer = pile.Manufacturer,
                IMEI = pile.IMEI,
                TotalPorts = pile.TotalPorts,
                ProtocolVersion = pile.ProtocolVersion,
                SoftwareVersion = pile.SoftwareVersion,
                HardwareVersion = pile.HardwareVersion,
                OnlineStatus = pile.OnlineStatus,
                SignalStrength = pile.SignalStrength,
                Temperature = pile.Temperature,
                LastHeartbeatTime = pile.LastHeartbeatTime,
                InstallationDate = pile.InstallationDate,
                LastMaintenanceDate = pile.LastMaintenanceDate,
                UpdateTime = pile.UpdateTime
            };
        }
    }
}
