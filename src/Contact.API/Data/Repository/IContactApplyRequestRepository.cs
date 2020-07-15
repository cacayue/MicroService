using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Models;

namespace Contact.API.Data.Repository
{
    public interface IContactApplyRequestRepository
    {
        /// <summary>
        /// 发起好友申请请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="applierId">申请人ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken);

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<ContactApplyRequest>> GetApplyRequestList(int userId, CancellationToken cancellationToken);
    }
}