using ChargingPileAdmin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 充电桩服务接口
    /// </summary>
    public interface IChargingPileService
    {
        /// <summary>
        /// 获取所有充电桩
        /// </summary>
        /// <returns>充电桩列表</returns>
        Task<IEnumerable<ChargingPileDto>> GetAllPilesAsync();

        /// <summary>
        /// 获取充电桩详情
        /// </summary>
        /// <param name="id">充电桩ID</param>
        /// <returns>充电桩详情</returns>
        Task<ChargingPileDto> GetPileByIdAsync(string id);

        /// <summary>
        /// 根据充电站ID获取充电桩列表
        /// </summary>
        /// <param name="stationId">充电站ID</param>
        /// <returns>充电桩列表</returns>
        Task<IEnumerable<ChargingPileDto>> GetPilesByStationIdAsync(string stationId);

        /// <summary>
        /// 获取充电桩分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="stationId">充电站ID，可选</param>
        /// <param name="status">充电桩状态，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>充电桩分页列表</returns>
        Task<PagedResponseDto<ChargingPileDto>> GetPilesPagedAsync(int pageNumber, int pageSize, string stationId = null, int? status = null, string keyword = null);

        /// <summary>
        /// 创建充电桩
        /// </summary>
        /// <param name="dto">创建充电桩请求</param>
        /// <returns>创建的充电桩ID</returns>
        Task<string> CreatePileAsync(CreateChargingPileDto dto);

        /// <summary>
        /// 更新充电桩
        /// </summary>
        /// <param name="id">充电桩ID</param>
        /// <param name="dto">更新充电桩请求</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdatePileAsync(string id, UpdateChargingPileDto dto);

        /// <summary>
        /// 删除充电桩
        /// </summary>
        /// <param name="id">充电桩ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeletePileAsync(string id);
    }
}
