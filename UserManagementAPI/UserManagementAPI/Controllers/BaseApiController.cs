using Betting.Backend.Core.Services.Jwt;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManagement.Domain.Common.Response;
using UserManagement.Domain.Entities;

namespace UserManagement.API.Controllers
{
    [Route("gateway/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        public SignInManager<ApplicationUser> SignInManager
        {
            get
            {
                return HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            }
        }

        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            }
        }

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
            }
        }


        public Task<ApplicationUser> CurrentUserAsync
        {
            get
            {
                var currentUser = UserManager.FindByEmailAsync(Username);
                return currentUser;
            }
        }

        public string Username
        {
            get
            {
                var headerKey = "Authorization";
                if (HttpContext.Request.Headers.ContainsKey(headerKey))
                {
                    var jwtService = HttpContext.RequestServices.GetRequiredService<IJwtService>();

                    var token = HttpContext.Request.Headers["Authorization"].First().Split(" ").Last();
                    var userClaimes = jwtService.ValidateToken(token);
                    if (userClaimes.Count > 0)
                    {
                        if (userClaimes.ContainsKey(ClaimTypes.Expired))
                        {
                            throw new UnauthorizedAccessException("Token has expired!");
                        }

                        // attach user to context on successful jwt validation
                        var username = userClaimes[JwtRegisteredClaimNames.Email];
                        return username;
                    }
                    throw new UnauthorizedAccessException("Jwt token failed!");
                }
                return string.Empty;
            }
        }

        protected ObjectResult Result<T>(ResponseWrapper<T> response) where T : class
        {
            if (response.StatusCode == StatusCodes.Status200OK)
                return Ok(response);
            if (response.StatusCode == StatusCodes.Status401Unauthorized)
                return Unauthorized(response);
            if (response.StatusCode != StatusCodes.Status404NotFound)
                return NotFound(response);
            if (response.StatusCode != StatusCodes.Status405MethodNotAllowed)
                return NotFound(response);
            return BadRequest(response);
        }
    }
}
