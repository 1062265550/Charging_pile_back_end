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
    /// 充电端口服务实现
    /// </summary>
    public class ChargingPortService : IChargingPortService
    {
        private readonly IChargingPortRepository _portRepository;
        private readonly IChargingPileRepository _pileRepository;

        public ChargingPortService(
            IChargingPortRepository portRepository,
            IChargingPileRepository pileRepository)
        {
            _portRepository = portRepository;
            _pileRepository = pileRepository;
        }

        /// <summary>
        /// 获取所有充电端口
        /// </summary>
        /// <returns>充电端口列表</returns>
        public async Task<IEnumerable<ChargingPortDto>> GetAllPortsAsync()
        {
            var ports = await _portRepository.GetAsync(
                p => true,
                p => p.ChargingPile);

            return ports.Select(MapToDto);
        }

        /// <summary>
        /// 获取充电端口详情
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <returns>充电端口详情</returns>
        public async Task<ChargingPortDto> GetPortByIdAsync(string id)
        {
            var port = await _portRepository.GetPortWithPileAsync(id);
            if (port == null)
            {
                return null;
            }

            return MapToDto(port);
        }

        /// <summary>
        /// 根据充电桩ID获取充电端口列表
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电端口列表</returns>
        public async Task<IEnumerable<ChargingPortDto>> GetPortsByPileIdAsync(string pileId)
        {
            var ports = await _portRepository.GetPortsByPileIdAsync(pileId);
            return ports.Select(MapToDto);
        }

        /// <summary>
        /// 获取充电端口分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pileId">充电桩ID，可选</param>
        /// <param name="status">充电端口状态，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>充电端口分页列表</returns>
        public async Task<PagedResponseDto<ChargingPortDto>> GetPortsPagedAsync(int pageNumber, int pageSize, string pileId = null, int? status = null, string keyword = null)
        {
            // 构建查询条件
            Expression<Func<ChargingPort, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(pileId))
            {
                filter = p => p.PileId == pileId;
            }

            if (status.HasValue)
            {
                filter = p => p.Status == status.Value;
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = p => p.PortNo.Contains(keyword);
            }

            // 获取分页数据
            var (ports, totalCount, totalPages) = await _portRepository.GetPagedAsync(
                filter,
                pageNumber,
                pageSize,
                query => query.OrderByDescending(p => p.UpdateTime),
                p => p.ChargingPile
            );

            // 返回分页响应
            return new PagedResponseDto<ChargingPortDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = ports.Select(MapToDto).ToList()
            };
        }

        /// <summary>
        /// 创建充电端口
        /// </summary>
        /// <param name="dto">创建充电端口请求</param>
        /// <returns>创建的充电端口ID</returns>
        public async Task<string> CreatePortAsync(CreateChargingPortDto dto)
        {
            // 检查充电桩是否存在
            var pile = await _pileRepository.GetByIdAsync(dto.PileId);
            if (pile == null)
            {
                throw new Exception($"找不到ID为 '{dto.PileId}' 的充电桩");
            }

            // 检查端口编号是否已存在于同一充电桩下
            if (await _portRepository.IsPortNoExistsAsync(dto.PileId, dto.PortNo))
            {
                throw new Exception($"端口编号 '{dto.PortNo}' 已存在于该充电桩下");
            }

            // 创建新的充电端口
            var port = new ChargingPort
            {
                Id = Guid.NewGuid().ToString(),
                PileId = dto.PileId,
                PortNo = dto.PortNo,
                PortType = dto.PortType,
                Status = dto.Status ?? 0, // 使用DTO中的状态值，如果为空则默认为离线
                Voltage = dto.Voltage,     // 设置电压
                CurrentAmpere = dto.CurrentAmpere, // 设置电流
                Power = dto.Power,    // 设置功率
                Temperature = dto.Temperature, // 设置温度
                FaultType = 0, // 初始无故障
                IsDisabled = false, // 初始未禁用
                TotalChargingTimes = 0,
                TotalChargingDuration = 0,
                TotalPowerConsumption = 0,
                UpdateTime = DateTime.Now
            };

            await _portRepository.AddAsync(port);
            await _portRepository.SaveChangesAsync();

            return port.Id;
        }

        /// <summary>
        /// 更新充电端口
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <param name="dto">更新充电端口请求</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdatePortAsync(string id, UpdateChargingPortDto dto)
        {
            // 检查充电端口是否存在
            var port = await _portRepository.GetByIdAsync(id);
            if (port == null)
            {
                return false;
            }

            // 检查端口编号是否已存在于同一充电桩下
            if (!string.IsNullOrEmpty(dto.PortNo) && dto.PortNo != port.PortNo)
            {
                if (await _portRepository.IsPortNoExistsAsync(port.PileId, dto.PortNo, id))
                {
                    throw new Exception($"端口编号 '{dto.PortNo}' 已存在于该充电桩下");
                }
            }

            // 更新充电端口
            port.PortNo = dto.PortNo ?? port.PortNo;
            port.PortType = dto.PortType ?? port.PortType;
            port.IsDisabled = dto.IsDisabled ?? port.IsDisabled;
            
            // 更新状态和参数值
            if (dto.Status.HasValue)
            {
                port.Status = dto.Status.Value;
            }
            
            // 更新电压、电流、功率和温度，如果有提供的话
            if (dto.Voltage.HasValue)
            {
                port.Voltage = dto.Voltage;
            }
            
            if (dto.CurrentAmpere.HasValue)
            {
                port.CurrentAmpere = dto.CurrentAmpere;
            }
            
            if (dto.Power.HasValue)
            {
                port.Power = dto.Power;
            }
            
            if (dto.Temperature.HasValue)
            {
                port.Temperature = dto.Temperature;
            }
            
            port.UpdateTime = DateTime.Now;

            _portRepository.Update(port);
            await _portRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 删除充电端口
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeletePortAsync(string id)
        {
            // 检查充电端口是否存在
            var port = await _portRepository.GetByIdAsync(id);
            if (port == null)
            {
                return false;
            }

            // 检查充电端口是否正在使用中
            if (port.Status == 2)
            {
                throw new Exception("充电端口正在使用中，无法删除");
            }

            // 删除充电端口
            await _portRepository.DeleteAsync(id);
            await _portRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 将充电端口实体映射为DTO
        /// </summary>
        /// <param name="port">充电端口实体</param>
        /// <returns>充电端口DTO</returns>
        private ChargingPortDto MapToDto(ChargingPort port)
        {
            return new ChargingPortDto
            {
                Id = port.Id,
                PileId = port.PileId,
                PileNo = port.ChargingPile?.PileNo,
                PortNo = port.PortNo,
                PortType = port.PortType,
                Status = port.Status,
                FaultType = port.FaultType,
                IsDisabled = port.IsDisabled,
                CurrentOrderId = port.CurrentOrderId,
                Voltage = port.Voltage,
                CurrentAmpere = port.CurrentAmpere,
                Power = port.Power,
                Temperature = port.Temperature,
                Electricity = port.Electricity,
                TotalChargingTimes = port.TotalChargingTimes,
                TotalChargingDuration = port.TotalChargingDuration,
                TotalPowerConsumption = port.TotalPowerConsumption,
                LastCheckTime = port.LastCheckTime,
                LastOrderId = port.LastOrderId,
                UpdateTime = port.UpdateTime
            };
        }
    }
}
