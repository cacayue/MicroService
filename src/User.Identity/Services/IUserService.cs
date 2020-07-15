using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public interface IUserService
    {
        Task<UserInfo> CheckOrCreate(string phone);
    }
}