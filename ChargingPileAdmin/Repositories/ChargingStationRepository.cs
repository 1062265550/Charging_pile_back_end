using ChargingPileAdmin.Data;
using ChargingPileAdmin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 充电站仓储实现
    /// </summary>
    public class ChargingStationRepository : Repository<ChargingStation>, IChargingStationRepository
    {
        public ChargingStationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 获取充电站及其下属充电桩
        /// </summary>
        /// <param name="stationId">充电站ID</param>
        /// <returns>充电站</returns>
        public async Task<ChargingStation> GetStationWithPilesAsync(string stationId)
        {
            return await _context.ChargingStations
                .Include(s => s.ChargingPiles)
                .FirstOrDefaultAsync(s => s.Id == stationId);
        }

        /// <summary>
        /// 获取所有充电站及其下属充电桩数量
        /// </summary>
        /// <returns>充电站列表</returns>
        public async Task<IEnumerable<(ChargingStation Station, int PileCount)>> GetStationsWithPileCountAsync()
        {
            var stations = await _context.ChargingStations.ToListAsync();
            var result = new List<(ChargingStation Station, int PileCount)>();

            foreach (var station in stations)
            {
                var pileCount = await _context.ChargingPiles.CountAsync(p => p.StationId == station.Id);
                result.Add((station, pileCount));
            }

            return result;
        }

        /// <summary>
        /// 根据状态获取充电站
        /// </summary>
        /// <param name="status">充电站状态</param>
        /// <returns>充电站列表</returns>
        public async Task<IEnumerable<ChargingStation>> GetStationsByStatusAsync(int status)
        {
            return await _context.ChargingStations
                .Where(s => s.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// 检查充电站名称是否已存在
        /// </summary>
        /// <param name="name">充电站名称</param>
        /// <param name="excludeId">排除的充电站ID</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsNameExistsAsync(string name, string excludeId = null)
        {
            if (string.IsNullOrEmpty(excludeId))
            {
                return await _context.ChargingStations.AnyAsync(s => s.Name == name);
            }
            else
            {
                return await _context.ChargingStations.AnyAsync(s => s.Name == name && s.Id != excludeId);
            }
        }
    }
}
