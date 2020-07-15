using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace User.Identity.Authentication
{
    public class UserProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subject = context.Subject ?? throw new ArgumentException(nameof(context.Subject));
            var subjectId = subject.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            if (!int.TryParse(subjectId, out int intUserId))
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var contextIssuedClaims = context.Subject.Claims.ToList();
            context.IssuedClaims = contextIssuedClaims;
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var subject = context.Subject ?? throw new ArgumentException(nameof(context.Subject));
            var subjectId = subject.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            context.IsActive = int.TryParse(subjectId, out int intUserId);
            return Task.CompletedTask;
        }
    }
}