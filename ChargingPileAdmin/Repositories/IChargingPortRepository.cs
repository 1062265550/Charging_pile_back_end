using ChargingPileAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 充电端口仓储接口
    /// </summary>
    public interface IChargingPortRepository : IRepository<ChargingPort>
    {
        /// <summary>
        /// 获取充电端口及其所属充电桩
        /// </summary>
        /// <param name="portId">充电端口ID</param>
        /// <returns>充电端口</returns>
        Task<ChargingPort> GetPortWithPileAsync(string portId);

        /// <summary>
        /// 根据充电桩ID获取充电端口
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电端口列表</returns>
        Task<IEnumerable<ChargingPort>> GetPortsByPileIdAsync(string pileId);

        /// <summary>
        /// 根据状态获取充电端口
        /// </summary>
        /// <param name="status">充电端口状态</param>
        /// <returns>充电端口列表</returns>
        Task<IEnumerable<ChargingPort>> GetPortsByStatusAsync(int status);

        /// <summary>
        /// 检查充电端口编号是否已存在于同一充电桩下
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <param name="portNo">充电端口编号</param>
        /// <param name="excludeId">排除的充电端口ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsPortNoExistsAsync(string pileId, string portNo, string excludeId = null);
    }
}
