using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using User.Api.Controllers;
using User.Api.Data;
using User.Api.Models;
using Xunit;

namespace User.Api.UnitTests
{
    public class UnitTest1
    {
        private UserDbContext GetUserContext()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var userContext = new UserDbContext(options);
            userContext.AppUsers.Add(new AppUser
            {
                Id = 1,
                Name = "jess"
            });
            userContext.SaveChanges();
            return userContext;
        }

        private (UserController, UserDbContext) GetUserController()
        {   
            var context = GetUserContext();
            var loggerMoq = new Mock<ILogger<UserController>>();
            var logger = loggerMoq.Object;
            var capMoq = new Mock<ICapPublisher>();
            var capPublish = capMoq.Object;
            var userController = new UserController(context, logger, capPublish);
            return (userController,context);
        }

        [Fact]
        public async Task Get_ReturnRightUser_WithExpectedParameters()
        {
            (UserController controller, UserDbContext context) =  GetUserController();
            var response = await controller.Get();

            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Name.Should().Be("jess");
            appUser.Id.Should().Be(1);
        }

        [Fact]
        public async Task Patch_ReturnNewName_WithExceptedNewNameParameter()
        {
            (UserController controller, UserDbContext context) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(u => u.Name, "lei");
            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;
            //assert response
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Name.Should().Be("lei");
            //assert context
            var userModel = await context.AppUsers.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Should().NotBeNull();
            userModel.Name.Should().Be("lei");
        }

        [Fact]
        public async Task Patch_ReturnAddProperties_WithExceptedProperties()
        {
            (UserController controller, UserDbContext context) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(u => u.Properties, new List<UserProperty>
            {
                new UserProperty
                {
                    Key = "fin_stage",
                    Text = "A+態",
                    Value = "A+態"
                },
                new UserProperty
                {
                    Key = "fin_stage",
                    Text = "C態",
                    Value = "C態"
                }
            });
            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;
            //assert response
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Properties.Should().HaveCount(2);
            appUser.Properties.First().Value.Should().Be("A+態");
            //assert context
            var userModel = await context.AppUsers.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Properties.Should().HaveCount(2);
            userModel.Properties.First().Value.Should().Be("A+態");
        }

        [Fact]
        public async Task Patch_ReturnRemoveProperties_WithExceptedProperties()
        {
            (UserController controller, UserDbContext context) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(u => u.Properties,new List<UserProperty>());
            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;
            //assert response
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Properties.Should().BeEmpty();
            //assert context
            var userModel = await context.AppUsers.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Properties.Should().BeEmpty();
        }
    }
}
