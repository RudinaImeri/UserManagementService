using Betting.Backend.Core.Services.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManagement.Core.Service;
using UserManagement.Domain.Common.Response;
using UserManagement.Domain.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IMapperService _mapperService;

        public AccountController(IJwtService jwtService, IUserService userService, IMapperService mapperService)
        {
            _jwtService = jwtService;
            _userService = userService;
            _mapperService = mapperService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Username);
                if (user == null)
                    user = await UserManager.FindByEmailAsync(model.Username);
                if (user != null)
                {
                    var isUserLockout = await UserManager.IsLockedOutAsync(user);
                    if (isUserLockout)
                        return Forbid();

                    var result = await SignInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (result.Succeeded)
                    {
                        var jwtToken = _jwtService.GenerateToken(user);
                        return Ok(new
                        {
                            token = jwtToken,
                            uid = user.Id,
                            usr = user.UserName,
                        });
                    }
                }
            }
            return Unauthorized(new ResponseWrapper<ModelStateDictionary>()
            {
                Data = ModelState,
                StatusCode = 401,
                Message = "Incorrect username or password"
            });
        }

        [HttpPost("ValidateToken")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken(string jwtToken)
        {
            try
            {
                var userClaims = _jwtService.ValidateToken(jwtToken);
                if (userClaims.ContainsKey(ClaimTypes.Expired))
                {
                    return Unauthorized();
                }

                var userId = userClaims[JwtRegisteredClaimNames.Sid];
                var user = await UserManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Unauthorized();
                }
                return Ok(new
                {
                    token = jwtToken,
                    uid = user.Id,
                    usr = user.UserName,
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            try
            {
                var userExists = await UserManager.FindByNameAsync(model.Username);
                if (userExists != null)
                {
                    return Ok(new ResponseWrapper<object>()
                    {
                        Message = "Email exist, try login!",
                        StatusCode = 200
                    });
                }

                if (ModelState.IsValid)
                {
                    var identityResult = await UserManager.CreateAsync(new ApplicationUser()
                    {
                        FirstName = model.Firstname,
                        LastName = model.Lastname,
                        UserName = model.Username,
                        Email = model.Username,
                        PhoneNumber = model.PhoneNumber,
                        Culture = model.Culture,
                        Language = model.Language,
                    }, model.Password);

                    var user = await UserManager.FindByNameAsync(model.Username);

                    await UserManager.AddToRoleAsync(user, "user");
                    return Ok(new ResponseWrapper<IdentityResult>()
                    {
                        Data = identityResult
                    });
                }
                else
                {
                    return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                    {
                        Data = ModelState,
                        StatusCode = 400,
                        Message = "Something goes wrong, please try again later!"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                {
                    Data = ModelState,
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody]UpdateDto user)
        {
            var userMapped = _mapperService.MapUserForUpdateEntityToDto(user);
            await _userService.UpdateUserAsync(userMapped);

            return NoContent();
        }

        // DELETE: /Users/{id}
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                await _userService.DeleteUserAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
