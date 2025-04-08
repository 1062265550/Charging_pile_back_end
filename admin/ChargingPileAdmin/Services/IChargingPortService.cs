using ChargingPileAdmin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargingPileAdmin.Services
{
    /// <summary>
    /// 充电端口服务接口
    /// </summary>
    public interface IChargingPortService
    {
        /// <summary>
        /// 获取所有充电端口
        /// </summary>
        /// <returns>充电端口列表</returns>
        Task<IEnumerable<ChargingPortDto>> GetAllPortsAsync();

        /// <summary>
        /// 获取充电端口详情
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <returns>充电端口详情</returns>
        Task<ChargingPortDto> GetPortByIdAsync(string id);

        /// <summary>
        /// 根据充电桩ID获取充电端口列表
        /// </summary>
        /// <param name="pileId">充电桩ID</param>
        /// <returns>充电端口列表</returns>
        Task<IEnumerable<ChargingPortDto>> GetPortsByPileIdAsync(string pileId);

        /// <summary>
        /// 获取充电端口分页列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pileId">充电桩ID，可选</param>
        /// <param name="status">充电端口状态，可选</param>
        /// <param name="keyword">关键字，可选</param>
        /// <returns>充电端口分页列表</returns>
        Task<PagedResponseDto<ChargingPortDto>> GetPortsPagedAsync(int pageNumber, int pageSize, string pileId = null, int? status = null, string keyword = null);

        /// <summary>
        /// 创建充电端口
        /// </summary>
        /// <param name="dto">创建充电端口请求</param>
        /// <returns>创建的充电端口ID</returns>
        Task<string> CreatePortAsync(CreateChargingPortDto dto);

        /// <summary>
        /// 更新充电端口
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <param name="dto">更新充电端口请求</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdatePortAsync(string id, UpdateChargingPortDto dto);

        /// <summary>
        /// 删除充电端口
        /// </summary>
        /// <param name="id">充电端口ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeletePortAsync(string id);
    }
}
