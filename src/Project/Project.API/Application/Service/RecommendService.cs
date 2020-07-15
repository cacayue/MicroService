using System.Threading.Tasks;

namespace Project.API.Application.Service
{
    public class RecommendService:IRecommendService
    {
        public Task<bool> IsProjectInRecommend(int userId, int projectId)
        {
            return Task.FromResult(true);
        }
    }
}