using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Services
{
    public interface IUserService
    {
        Task<BaseUserInfo> GetBaseUserInfoAsync(int userId);
    }
}