using System.Threading.Tasks;
using Recommends.API.Dtos;

namespace Recommends.API.Services
{
    public interface IUserService
    {
        Task<UserIdentity> GetBaseUserInfoAsync(int userId);
    }
}