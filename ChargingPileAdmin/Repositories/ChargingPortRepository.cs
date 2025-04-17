using ChargingPileAdmin.Data;
using ChargingPileAdmin.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 充电端口仓储实现
    /// </summary>
    public class ChargingPortRepository : Repository<ChargingPort>, IChargingPortRepository
    {
        public ChargingPortRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 获取充电端口及其所属充电桩
        /// </summary>
        /// <param name="portId">充电端口ID</param>
        /// <returns>充电端口</returns>
        public async Task<ChargingPort> GetPortWithPileAsync(string portId)
        {
            return await _context.ChargingPorts
                .Include(p => p.ChargingPile)
                .FirstOrDefaultAsync(p => p.Id == portId);
        }

        /// <summary>
        /// 根据充电桩ID获取充电端口
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电端口列表</returns>
        public async Task<IEnumerable<ChargingPort>> GetPortsByPileIdAsync(string pileId)
        {
            return await _context.ChargingPorts
                .Where(p => p.PileId == pileId)
                .ToListAsync();
        }

        /// <summary>
        /// 根据状态获取充电端口
        /// </summary>
        /// <param name="status">充电端口状态</param>
        /// <returns>充电端口列表</returns>
        public async Task<IEnumerable<ChargingPort>> GetPortsByStatusAsync(int status)
        {
            return await _context.ChargingPorts
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// 检查充电端口编号是否已存在于同一充电桩下
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <param name="portNo">充电端口编号</param>
        /// <param name="excludeId">排除的充电端口ID</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsPortNoExistsAsync(string pileId, string portNo, string excludeId = null)
        {
            if (string.IsNullOrEmpty(excludeId))
            {
                return await _context.ChargingPorts.AnyAsync(p => p.PileId == pileId && p.PortNo == portNo);
            }
            else
            {
                return await _context.ChargingPorts.AnyAsync(p => p.PileId == pileId && p.PortNo == portNo && p.Id != excludeId);
            }
        }
    }
}
