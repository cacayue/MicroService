using System;

namespace Recommends.API.Models
{
    public class Recommend
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string FromUserAvatar { get; set; }
        public EnumRecommendType RecommendType { get; set; }
        public int ProjectId { get; set; }
        public string Company { get; set; }
        public string Introduction { get; set; }
        public string Tags { get; set; }
        public string FinStage { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime RecommendTime { get; set; }
    }
}