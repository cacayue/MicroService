using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Data.Repository;
using Contact.API.Dtos;
using Contact.API.Entity.Dtos;
using Contact.API.Enum;
using Contact.API.Models;
using Contact.API.Services;
using Contact.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Contact.API.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    public class ContactController : BaseController
    {
        private readonly IContactApplyRequestRepository _contactApplyRequestRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IContactApplyRequestRepository contactApplyRequestRepository,
            IContactRepository contactRepository,
            IUserService userService,
            ILogger<ContactController> logger)
        {
            _contactApplyRequestRepository = contactApplyRequestRepository;
            _contactRepository = contactRepository;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequests(CancellationToken cancellationToken)
        {
            return Ok(await _contactApplyRequestRepository.GetApplyRequestList(UserIdentity.UserId, cancellationToken));
        }

        /// <summary>
        /// 添加好友请求
        /// </summary>
        /// <param name="userId">被申请人Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("apply-requests/{userId}")]
        public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken)
        {
            //发起申请
            var result = await _contactApplyRequestRepository.AddRequestAsync(new ContactApplyRequest
            {
                ApplierId = UserIdentity.UserId,
                UserId = userId,
                Avatar = UserIdentity.Avatar,
                Company = UserIdentity.Company,
                Name = UserIdentity.Name,
                Title = UserIdentity.Title,
                Approvaled = (int)ApprovaledEnum.UnApproval,
                ApplyTime = DateTime.Now
            },cancellationToken);
            if (!result)
            {
                _logger.LogError($"通过好友请求出错,被申请人ID:{userId},申请人ID:{UserIdentity.UserId}");
                return BadRequest();
            }
            return Ok();
        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="applierId">申请人Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("apply-requests/{applierId}")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId, CancellationToken cancellationToken)
        {
            var result =await _contactApplyRequestRepository.ApprovalAsync(UserIdentity.UserId,applierId, cancellationToken);
            if (!result)
            {
                _logger.LogError($"通过好友请求出错,用户ID:{UserIdentity.UserId},申请人ID:{applierId}");
                return BadRequest();
            }
            var userInfo = new BaseUserInfo
            {
                Id = UserIdentity.UserId,
                Name = UserIdentity.Name,
                Avatar = UserIdentity.Avatar,
                Company = UserIdentity.Company,
                Title = UserIdentity.Title
            };
            var applierInfo =await _userService.GetBaseUserInfoAsync(applierId);

            await _contactRepository.AddContactBookAsync(UserIdentity.UserId, applierInfo, cancellationToken);
            await _contactRepository.AddContactBookAsync(applierId, userInfo, cancellationToken);
            return Ok();
        }

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetContacts(CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactsAsync(UserIdentity.UserId, cancellationToken));
        }

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetContacts(int userId,CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactsAsync(userId, cancellationToken));
        }

        /// <summary>
        /// 更新好友标签
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("tag")]
        public async Task<IActionResult> GetApplyRequests([FromBody]TagContactViewModel view,CancellationToken cancellationToken)
        {
            var result = await _contactRepository
                .TagContactAsync(UserIdentity.UserId, view.ContactId, view.Tags, cancellationToken);
            if (result)
            {
                return Ok();
            }
            else
            {
                _logger.LogError($"更新标签出错,用户ID:{UserIdentity.UserId},好友ID:{view.ContactId}");
                return BadRequest();
            }
        }
    }
}
