using ChargingPileAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Repositories
{
    /// <summary>
    /// 充电站仓储接口
    /// </summary>
    public interface IChargingStationRepository : IRepository<ChargingStation>
    {
        /// <summary>
        /// 获取充电站及其下属充电桩
        /// </summary>
        /// <param name="stationId">充电站ID</param>
        /// <returns>充电站</returns>
        Task<ChargingStation> GetStationWithPilesAsync(string stationId);

        /// <summary>
        /// 获取所有充电站及其下属充电桩数量
        /// </summary>
        /// <returns>充电站列表</returns>
        Task<IEnumerable<(ChargingStation Station, int PileCount)>> GetStationsWithPileCountAsync();

        /// <summary>
        /// 根据状态获取充电站
        /// </summary>
        /// <param name="status">充电站状态</param>
        /// <returns>充电站列表</returns>
        Task<IEnumerable<ChargingStation>> GetStationsByStatusAsync(int status);

        /// <summary>
        /// 检查充电站名称是否已存在
        /// </summary>
        /// <param name="name">充电站名称</param>
        /// <param name="excludeId">排除的充电站ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsNameExistsAsync(string name, string excludeId = null);
    }
}
