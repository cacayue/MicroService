using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using User.Api.Data;
using User.Api.IntegrationEvents.Events;
using User.Api.Models;
using User.Api.Utils;

namespace User.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly UserDbContext _userDbContext;
        private readonly ILogger<UserController> _logger;
        private readonly ICapPublisher _capPublisher;

        public UserController(UserDbContext userDbContext,ILogger<UserController> logger,
            ICapPublisher capPublisher)
        {
            _userDbContext = userDbContext;
            _logger = logger;
            _capPublisher = capPublisher;
        }

        private void RaiseUserProfileChangedEvent(AppUser user)
        {
            if (_userDbContext.Entry(user).Property((nameof(user.Name))).IsModified ||
                _userDbContext.Entry(user).Property((nameof(user.Company))).IsModified ||
                _userDbContext.Entry(user).Property((nameof(user.Title))).IsModified ||
                _userDbContext.Entry(user).Property((nameof(user.Avatar))).IsModified)
            {
                _capPublisher.Publish("finbook.userapi.userprofilechanged", new UserProfileChangedEvent
                {
                    UserId = user.Id,
                    Company = user.Company,
                    Title = user.Title,
                    Avatar = user.Avatar,
                    Name = user.Name
                });
            }
            
        }

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user =await _userDbContext.AppUsers
                .AsNoTracking()
                .Include(u=>u.Properties)
                .SingleOrDefaultAsync(x=>x.Id == UserIdentity.UserId);
            if (user == null)
            {
                throw new UserOperationException($"错误的上下文用户ID{UserIdentity.UserId}");
            }

            return new JsonResult(user);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [Route("baseinfo/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetBaseUserInfo(int userId)
        {
            //TODO 检查是否有好友信息
            var user = await _userDbContext.AppUsers
                .SingleOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
                //throw new UserOperationException($"错误的用户ID{userId}");
            }

            return Ok(new
            {
                user.Id,
                user.Company,
                user.Title,
                user.Name,
                user.Avatar
            });
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="patch">更新内容</param>
        /// <returns></returns>
        [Route("")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<AppUser> patch)
        {
            var user =await _userDbContext.AppUsers
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            patch.ApplyTo(user);

            foreach (var property in user.Properties)
            {
                _userDbContext.Entry(property).State = EntityState.Detached;
            }

            var originProperties = await _userDbContext.UserProperties
                .Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync();
            var allProperties = originProperties.Union(user.Properties);

            var removedProperties = originProperties.Except(user.Properties);
            var newProperties = allProperties.Except(originProperties);

            foreach (var property in removedProperties)
            {
                _userDbContext.Remove(property);
            }

            foreach (var property in newProperties)
            {
                _userDbContext.Add(property);
            }

            await using (var trans = _userDbContext.Database.BeginTransaction(_capPublisher, autoCommit: true))
            {
                //业务代码
                RaiseUserProfileChangedEvent(user);
                _userDbContext.Update(user);
                await _userDbContext.SaveChangesAsync();
            }
           
            return new JsonResult(user);
        }

        /// <summary>
        /// 检查或创建用户(用户手机号码不存在时)
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <returns></returns>
        [Route("check-or-create")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate([FromForm]string phone)
        {
            if (!CheckUtils.CheckPhoneNumber(phone))
            {
                throw new UserOperationException($"错误的手机号码{phone}");
            }
            var userExist = await _userDbContext.AppUsers.SingleOrDefaultAsync(u=>u.Phone == phone);
            if (userExist != null) return Ok(new
            {
                userExist.Id,
                userExist.Name,
                userExist.Company,
                userExist.Avatar,
                userExist.Title
            });
            var entityEntry = await _userDbContext.AppUsers.AddAsync(new AppUser
            {
                Phone = phone
            });
            
            await _userDbContext.SaveChangesAsync();
            var userInfo = entityEntry.Entity;
            return Ok(new
            {
                userInfo.Id,
                userInfo.Name,
                userInfo.Company,
                userInfo.Avatar,
                userInfo.Title
            });
        }

        /// <summary>
        /// 获取用户标签
        /// </summary>
        /// <returns></returns>
        [Route("tags")]
        [HttpGet]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _userDbContext.UserTags.Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync());
        }

        /// <summary>
        /// 更新用户标签
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        [Route("tags")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserTags([FromBody]List<string> tags)
        {
            if (!tags.Any())
            {
                throw new UserOperationException("不能传入空的标签组");
            }
            var originTags = await _userDbContext.UserTags.Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync();
            var exceptTags = tags.Except(originTags.Select(o=>o.Tag))
                .Select(t=>new UserTag
                {
                    AppUserId = UserIdentity.UserId,
                    CreatedTime = DateTime.Now,
                    Tag = t
                });
            await _userDbContext.UserTags.AddRangeAsync(exceptTags);
            await _userDbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// 根据手机号查找用户资料
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <returns></returns>
        [Route("search")]
        [HttpPost]
        public async Task<IActionResult> Search([FromForm]string phone)
        {
            return Ok(await _userDbContext.AppUsers
                .Include(u=>u.Properties)
                .FirstOrDefaultAsync(u => u.Phone == phone));
        }
    }
}