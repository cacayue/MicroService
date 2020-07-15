using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using User.Identity.Services;

namespace User.Identity.Authentication
{
    public class SmsAuthCodeValidator:IExtensionGrantValidator
    {
        private readonly IAuthCodeService _authCodeService;
        private readonly IUserService _userService;

        public SmsAuthCodeValidator(IAuthCodeService authCodeService,
            IUserService userService)
        {
            _authCodeService = authCodeService;
            _userService = userService;
        }
        public string GrantType => "sms_auth_code";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw["phone"];
            var authCode = context.Request.Raw["auth_code"];
            var errorValidation = new GrantValidationResult(TokenRequestErrors.InvalidGrant);


            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(authCode))
            {
                context.Result = errorValidation;
                return;
            }

            if (!_authCodeService.Validate(phone, authCode))
            {
                context.Result = errorValidation;
                return;
            }

            var user =await _userService.CheckOrCreate(phone);

            if (user == null)
            {
                context.Result = errorValidation;
                return;
            }

            var claims = new List<Claim>
            {
                new Claim("name",user.Name??String.Empty), 
                new Claim("company",user.Company??String.Empty), 
                new Claim("title",user.Title??String.Empty), 
                new Claim("avatar",user.Avatar??String.Empty)
            };

            context.Result = new GrantValidationResult(user.Id.ToString(),
                GrantType,
                claims);

        }

        
    }
}