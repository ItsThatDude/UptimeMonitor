using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UptimeMonitor.Web.Models;

namespace UptimeMonitor.Web.Controllers.Api
{
    [ApiController]
    [Route("api/user")]
    [EndpointGroupName("Public")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetCurrentUser() => Ok(CreateUserInfo(User));

        private static UserInfo CreateUserInfo(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null || claimsPrincipal.Identity == null
                || !claimsPrincipal.Identity.IsAuthenticated)
            {
                return UserInfo.Anonymous;
            }

            var userInfo = new UserInfo
            {
                IsAuthenticated = true
            };

            if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
            {
                userInfo.NameClaimType = claimsIdentity.NameClaimType;
                userInfo.RoleClaimType = claimsIdentity.RoleClaimType;
            }
            else
            {
                userInfo.NameClaimType = ClaimTypes.Name;
                userInfo.RoleClaimType = ClaimTypes.Role;
            }

            if (claimsPrincipal.Claims?.Any() ?? false)
            {
                //var claims = claimsPrincipal.FindAll(userInfo.NameClaimType)
                //                            .Select(u => new ClaimValue(userInfo.NameClaimType, u.Value))
                //                            .ToList();

                var claims = claimsPrincipal.Claims.Select(u => new ClaimValue(u.Type, u.Value))
                                                      .ToList();

                userInfo.Claims = claims;
            }

            return userInfo;
        }
    }
}
