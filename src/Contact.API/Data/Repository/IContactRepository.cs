using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Dtos;
using Contact.API.Entity.Dtos;

namespace Contact.API.Data.Repository
{
    public interface IContactRepository
    {
        /// <summary>
        /// 更新好友信息
        /// </summary>
        /// <param name="baseUser"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateContactInfoAsync(BaseUserInfo baseUser, CancellationToken cancellationToken);

        /// <summary>
        /// 新增好友
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="approvalInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddContactBookAsync(int userId, BaseUserInfo approvalInfo, CancellationToken cancellationToken);

        /// <summary>
        /// 获取所有好友
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<Models.Contact>> GetContactsAsync(int user, CancellationToken cancellationToken);

        /// <summary>
        /// 更新好友标签
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contactId"></param>
        /// <param name="tags"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken);
    }
}