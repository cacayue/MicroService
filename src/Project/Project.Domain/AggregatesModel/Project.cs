using System;
using System.Collections.Generic;
using System.Linq;
using Project.Domain.Events;
using Project.Domain.SeedWork;

namespace Project.Domain.AggregatesModel
{
    public class Project:Entity,IAggregateRoot
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 项目logo
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 原BP文件地址
        /// </summary>
        public string OriginBPFile { get; set; }
        /// <summary>
        /// 转换后BP文件地址
        /// </summary>
        public string FormatBPFile { get; set; }
        /// <summary>
        /// 是否显示敏感信息
        /// </summary>
        public bool ShowSecurityInfo { get; set; }
        /// <summary>
        /// 省Id
        /// </summary>
        public int ProvinceId { get; set; }
        /// <summary>
        /// 省名称
        /// </summary>
        public string ProvinceName { get; set; }
        /// <summary>
        /// 市ID
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// 市名称
        /// </summary>
        public string CityName { get; set; }
        /// <summary>
        /// 区域ID
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 公司注册时间
        /// </summary>
        public DateTime RegisterTime { get; set; }
        /// <summary>
        /// 项目基本信息
        /// </summary>
        public string Introduction { get; set; }
        /// <summary>
        /// 出让股份比例
        /// </summary>
        public string FinPercentage { get; set; }
        /// <summary>
        /// 融资阶段
        /// </summary>
        public string FinStage { get; set; }
        /// <summary>
        /// 融资金额 单位(万)
        /// </summary>
        public int FinMoney { get; set; }
        /// <summary>
        /// 收入 单位(万)
        /// </summary>
        public int Income { get; set; }
        /// <summary>
        /// 利润 单位(万)
        /// </summary>
        public int Revenue { get; set; }
        /// <summary>
        /// 估值 单位(万)
        /// </summary>
        public int Valuation { get; set; }
        /// <summary>
        /// 佣金分配方式
        /// </summary>
        public string BrokerageOption { get; set; }
        /// <summary>
        /// 是否委托给平台
        /// </summary>
        public bool OnPlatform { get; set; }
        /// <summary>
        /// 根引用项目ID
        /// </summary>
        public int SourceId { get; set; }
        /// <summary>
        /// 上级引用项目Id
        /// </summary>
        public int ReferenceId { get; set; }
        /// <summary>
        /// 项目标签
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        /// 项目属性: 行业领域,融资币种
        /// </summary>
        public List<ProjectProperty> Properties { get; set; }
        /// <summary>
        /// 可设置可见范围
        /// </summary>
        public ProjectVisibleRule VisibleRule { get; set; }
        /// <summary>
        /// 贡献者列表
        /// </summary>
        public List<ProjectContributor> Contributors { get; set; }
        /// <summary>
        /// 查看者
        /// </summary>
        public List<ProjectViewer> Viewers { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 拷贝项目
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Project CloneProject(Project source = null)
        {
            if (source == null)
                return this;
            var newProject = new Project
            {
                AreaId = source.AreaId,
                BrokerageOption = source.BrokerageOption,
                Avatar = source.Avatar,
                CityId = source.CityId,
                CityName = source.CityName,
                AreaName = source.AreaName,
                Company = source.Company,
                Contributors = new List<ProjectContributor>(),
                CreatedTime = DateTime.Now,
                FinMoney = source.FinMoney,
                FinPercentage = source.FinPercentage,
                FinStage = source.FinStage,
                FormatBPFile = source.FormatBPFile,
                OriginBPFile = source.OriginBPFile,
                Viewers = new List<ProjectViewer>(),
                Income = source.Income,
                OnPlatform = source.OnPlatform,
                ProvinceId = source.ProvinceId,
                ProvinceName = source.ProvinceName,
                Valuation = source.Valuation,
                VisibleRule = source.VisibleRule == null
                    ? null
                    : new ProjectVisibleRule() {Visible = source.VisibleRule.Visible, Tags = source.VisibleRule.Tags},
                ShowSecurityInfo = source.ShowSecurityInfo,
                Tags = source.Tags,
                RegisterTime = source.RegisterTime,
                Revenue = source.Revenue,
                Properties = new List<ProjectProperty>()
            };
            foreach (var item in source.Properties)
            {
                newProject.Properties.Add(new ProjectProperty()
                {
                    Key = item.Key,
                    Text = item.Text,
                    Value = item.Value
                });
            }
            return newProject;
        }
        /// <summary>
        /// 参与者获取拷贝
        /// </summary>
        /// <param name="contributorId"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public Project ContributorFork(int contributorId, Project source = null)
        {
            if (source == null)
                return this;
            var newProject = CloneProject(source);
            newProject.UserId = contributorId;
            newProject.SourceId = source.SourceId == 0 ? source.Id : source.SourceId;
            newProject.ReferenceId = source.ReferenceId == 0 ? source.Id : source.SourceId;
            newProject.UpdateTime = DateTime.Now;
            return newProject;
        }

        public Project()
        {
            Contributors = new List<ProjectContributor>();
            Viewers = new List<ProjectViewer>();
            AddDomainEvent(new ProjectCreatedEvent()
            {
                Project = this,

            });
        }

        public void AddViewer(int userId, string userName, string avatar)
        {
            var viewer = new ProjectViewer
            {
                UserName = userName,UserId = userId,Avatar = avatar,CreatedTime = DateTime.Now
            };
            if (Viewers.All(u => u.UserId != userId))
            {
                Viewers.Add(viewer);
                AddDomainEvent(new ProjectViewedEvent()
                {
                    Company = Company,
                    Introduction = Introduction,
                    Viewer = viewer
                });
            }
                
        }

        public void AddContributor(ProjectContributor contributor)
        {
            if (Contributors.All(u => u.UserId != contributor.UserId))
            {
                Contributors.Add(contributor);
                AddDomainEvent(new ProjectJoinedEvent()
                {
                    Company = Company,
                    Introduction = Introduction,
                    Contributor = contributor
                });
            }    
        }

    }
}