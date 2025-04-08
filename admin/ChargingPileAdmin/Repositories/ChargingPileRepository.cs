using ChargingPileAdmin.Data;
using ChargingPileAdmin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 充电桩仓储实现
    /// </summary>
    public class ChargingPileRepository : Repository<ChargingPile>, IChargingPileRepository
    {
        public ChargingPileRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 获取充电桩及其所属充电站
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电桩</returns>
        public async Task<ChargingPile> GetPileWithStationAsync(string pileId)
        {
            return await _context.ChargingPiles
                .Include(p => p.ChargingStation)
                .FirstOrDefaultAsync(p => p.Id == pileId);
        }

        /// <summary>
        /// 获取充电桩及其下属充电端口
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电桩</returns>
        public async Task<ChargingPile> GetPileWithPortsAsync(string pileId)
        {
            return await _context.ChargingPiles
                .Include(p => p.ChargingPorts)
                .FirstOrDefaultAsync(p => p.Id == pileId);
        }

        /// <summary>
        /// 获取充电桩、所属充电站及下属充电端口
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电桩</returns>
        public async Task<ChargingPile> GetPileWithStationAndPortsAsync(string pileId)
        {
            return await _context.ChargingPiles
                .Include(p => p.ChargingStation)
                .Include(p => p.ChargingPorts)
                .FirstOrDefaultAsync(p => p.Id == pileId);
        }

        /// <summary>
        /// 根据充电站ID获取充电桩
        /// </summary>
        /// <param name="stationId">充电站ID</param>
        /// <returns>充电桩列表</returns>
        public async Task<IEnumerable<ChargingPile>> GetPilesByStationIdAsync(string stationId)
        {
            return await _context.ChargingPiles
                .Where(p => p.StationId == stationId)
                .ToListAsync();
        }

        /// <summary>
        /// 根据状态获取充电桩
        /// </summary>
        /// <param name="status">充电桩状态</param>
        /// <returns>充电桩列表</returns>
        public async Task<IEnumerable<ChargingPile>> GetPilesByStatusAsync(int status)
        {
            return await _context.ChargingPiles
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// 检查充电桩编号是否已存在
        /// </summary>
        /// <param name="pileNo">充电桩编号</param>
        /// <param name="excludeId">排除的充电桩ID</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsPileNoExistsAsync(string pileNo, string excludeId = null)
        {
            if (string.IsNullOrEmpty(excludeId))
            {
                return await _context.ChargingPiles.AnyAsync(p => p.PileNo == pileNo);
            }
            else
            {
                return await _context.ChargingPiles.AnyAsync(p => p.PileNo == pileNo && p.Id != excludeId);
            }
        }
    }
}
