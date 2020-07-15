using System.Collections.Generic;
using System.Threading.Tasks;
using Recommends.API.Dtos;

namespace Recommends.API.Services
{
    public interface IContactService
    {
        Task<List<Contact>> GetContactsByUserId(int userId);
    }
}