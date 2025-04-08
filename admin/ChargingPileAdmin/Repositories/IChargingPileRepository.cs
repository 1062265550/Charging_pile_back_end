using ChargingPileAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 充电桩仓储接口
    /// </summary>
    public interface IChargingPileRepository : IRepository<ChargingPile>
    {
        /// <summary>
        /// 获取充电桩及其所属充电站
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电桩</returns>
        Task<ChargingPile> GetPileWithStationAsync(string pileId);

        /// <summary>
        /// 获取充电桩及其下属充电端口
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电桩</returns>
        Task<ChargingPile> GetPileWithPortsAsync(string pileId);

        /// <summary>
        /// 获取充电桩、所属充电站及下属充电端口
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电桩</returns>
        Task<ChargingPile> GetPileWithStationAndPortsAsync(string pileId);

        /// <summary>
        /// 根据充电站ID获取充电桩
        /// </summary>
        /// <param name="stationId">充电站ID</param>
        /// <returns>充电桩列表</returns>
        Task<IEnumerable<ChargingPile>> GetPilesByStationIdAsync(string stationId);

        /// <summary>
        /// 根据状态获取充电桩
        /// </summary>
        /// <param name="status">充电桩状态</param>
        /// <returns>充电桩列表</returns>
        Task<IEnumerable<ChargingPile>> GetPilesByStatusAsync(int status);

        /// <summary>
        /// 检查充电桩编号是否已存在
        /// </summary>
        /// <param name="pileNo">充电桩编号</param>
        /// <param name="excludeId">排除的充电桩ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsPileNoExistsAsync(string pileNo, string excludeId = null);
    }
}
