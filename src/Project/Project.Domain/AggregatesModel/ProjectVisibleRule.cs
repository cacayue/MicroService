using System.Collections.Generic;
using Project.Domain.SeedWork;

namespace Project.Domain.AggregatesModel
{
    /// <summary>
    /// 确定哪些标签可见
    /// </summary>
    public class ProjectVisibleRule:Entity
    {
        public int ProjectId { get; set; }
        public bool Visible { get; set; }
        public string Tags { get; set; }

    }
}