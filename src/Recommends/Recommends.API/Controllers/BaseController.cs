using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Recommends.API.Dtos;

namespace Recommends.API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected UserIdentity UserIdentity
        {
            get
            {
                var claims = User.Claims.ToList();
                var identity = new UserIdentity
                {
                    UserId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value),
                    Name = User.Claims.FirstOrDefault(x => x.Type == "name")?.Value,
                    Company = User.Claims.FirstOrDefault(x => x.Type == "company")?.Value,
                    Title = User.Claims.FirstOrDefault(x => x.Type == "title")?.Value,
                    Avatar = User.Claims.FirstOrDefault(x => x.Type == "avatar")?.Value
                };
                return identity;
            }
        }
    }
}