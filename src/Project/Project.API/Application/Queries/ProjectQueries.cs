extern alias MySqlConnectorAlias;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Dapper;

namespace Project.API.Application.Queries
{
    public class ProjectQueries:IProjectQueries
    {
        private readonly string _connStr;

        public ProjectQueries(string connStr)
        {
            _connStr = connStr;
        }

        public async Task<dynamic> GetProjectsByUserId(int userId)
        {
            await using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var sql = @"SELECT 
                    p.Id,
                    p.Avatar,
                    p.Company,
                    p.FinStage,
                    p.FinStage,
                    p.FinPercentage,
                    p.Introduction,
                    p.Income,
                    p.ShowSecurityInfo,
                    p.CreatedTime
                    FROM projects p 
                    WHERE p.UserId = @userId";
            var projects = await conn.QueryAsync<dynamic>(sql, new { userId });
            return projects;
        }

        public async Task<dynamic> GetProjectDetail(int projectId)
        {
            await using var conn = new MySqlConnection(_connStr);
            conn.Open();
            var sql = @"SELECT 
                p.Company,
                p.CityName,
                p.ProvinceName,
                p.AreaName,
                p.FinStage,
                p.FinStage,
                p.FinPercentage,
                p.Introduction,
                p.UserId,
                p.Income,
                p.Revenue,
                p.Avatar,
                p.BrokerageOption,
                p.ShowSecurityInfo, 
                p.CreatedTime,
                pv.Tags,
                pv.Visible
                FROM projects p 
                INNER JOIN projectvisiblerules pv 
                ON p.Id = pv.ProjectId
                WHERE p.Id = @projectId;";
            var projects = await conn.QueryAsync<dynamic>(sql, new { projectId });
            return projects.FirstOrDefault();
        }
    }
}