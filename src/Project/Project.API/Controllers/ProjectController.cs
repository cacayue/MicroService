using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project.API.Application.Commands;
using Project.API.Application.Queries;
using Project.API.Application.Service;
using Project.Domain.AggregatesModel;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IRecommendService _recommendService;
        private readonly IProjectQueries _projectQueries;

        public ProjectController(IMediator mediator,
            IRecommendService recommendService,
            IProjectQueries projectQueries)
        {
            _mediator = mediator;
            _recommendService = recommendService;
            _projectQueries = projectQueries;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateProject([FromBody]Domain.AggregatesModel.Project project)
        {
            if(project == null)
                throw new ArgumentException(nameof(Domain.AggregatesModel.Project));
            project.UserId = UserIdentity.UserId;
            project.CreatedTime = DateTime.Now;
            var command = new CreateOrderCommand(){Project = project};
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetProjectByUserId()
        {
            var projects = await _projectQueries.GetProjectsByUserId(UserIdentity.UserId);
            return Ok(projects);
        }

        [HttpGet]
        [Route("my/{projectId}")]
        public async Task<IActionResult> GetMyProjectDetail(int projectId)
        {
            var project = await _projectQueries.GetProjectDetail(projectId);
            if (project == null)
            {
                return NotFound();
            }
            if (project.UserId != UserIdentity.UserId)
            {
                return BadRequest("没有权限");
            }
            return Ok(project);
        }

        [HttpGet]
        [Route("recommend/{projectId}")]
        public async Task<IActionResult> GetRecommendProjectDetail(int projectId)
        {
            if (!(await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId)))
            {
                return BadRequest("没有权限");
            }
            var project = await _projectQueries.GetProjectDetail(projectId);
            if (project == null)
            { 
                return NotFound();
            }
            return Ok(project);
        }

        [HttpPut] 
        [Route("view/{projectId}")]
        public async Task<IActionResult> ViewProject(int projectId)
        {
            if (!(await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId)))
            {
                return BadRequest("没有权限");
            }

            var command = new ViewProjectCommand()
            {
                ProjectId = projectId,
                Avatar = UserIdentity.Avatar,
                UserName = UserIdentity.Name,
                UserId = UserIdentity.UserId
            };
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPost]
        [Route("join/{projectId}")]
        public async Task<IActionResult> JoinProject(int projectId,[FromBody]ProjectContributor contributor)
        {
            if (!(await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId)))
            {
                return BadRequest("没有权限");
            }
            var command = new JoinProjectCommand() { Contributor = contributor };
            await _mediator.Send(command);
            return Ok();
        }
    }
}
