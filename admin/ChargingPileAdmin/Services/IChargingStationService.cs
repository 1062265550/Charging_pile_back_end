using ChargingPileAdmin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 充电站服务接口
    /// </summary>
    public interface IChargingStationService
    {
        /// <summary>
        /// 获取所有充电站
        /// </summary>
        /// <returns>充电站列表</returns>
        Task<IEnumerable<ChargingStationDto>> GetAllStationsAsync();

        /// <summary>
        /// 获取充电站详情
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <returns>充电站详情</returns>
        Task<ChargingStationDto> GetStationByIdAsync(string id);

        /// <summary>
        /// 获取充电站分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="status">充电站状态，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>充电站分页列表</returns>
        Task<PagedResponseDto<ChargingStationDto>> GetStationsPagedAsync(int pageNumber, int pageSize, int? status = null, string keyword = null);

        /// <summary>
        /// 创建充电站
        /// </summary>
        /// <param name="dto">创建充电站请求</param>
        /// <returns>创建的充电站ID</returns>
        Task<string> CreateStationAsync(CreateChargingStationDto dto);

        /// <summary>
        /// 更新充电站
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <param name="dto">更新充电站请求</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateStationAsync(string id, UpdateChargingStationDto dto);

        /// <summary>
        /// 删除充电站
        /// </summary>
        /// <param name="id">充电站ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteStationAsync(string id);
    }
}
