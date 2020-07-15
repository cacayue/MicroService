using System;

namespace Project.API.Application.IntegrationEvents
{
    public class ProjectCreatedIntegrationEvent
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public string Company { get; set; }
        public string Introduction { get; set; }
        public string Tags { get; set; }
        public string FinStage { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}