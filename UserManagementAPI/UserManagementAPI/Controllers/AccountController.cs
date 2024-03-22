using Betting.Backend.Core.Services.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using UserManagement.Core.Context;
using UserManagement.Domain.Common.Response;
using UserManagement.Domain.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IJwtService _jwtService;
        private readonly ILoggers _log;

        public AccountController(IJwtService jwtService, ILoggers log)
        {
            _jwtService = jwtService;
            _log = log;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            try
            {
                var message = "";

                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindByNameAsync(model.Username);

                    user ??= await UserManager.FindByEmailAsync(model.Username);

                    if (user != null)
                    {
                        var isUserLockout = await UserManager.IsLockedOutAsync(user);

                        if (isUserLockout)
                        {
                            message = $"User '{user.UserName}' cannot be login!";
                            await _log.LogAsync(message, LogLevel.Error);

                            return Forbid();
                        }

                        var result = await SignInManager.CheckPasswordSignInAsync(user, model.Password, false);

                        if (result.Succeeded)
                        {
                            var jwtToken = _jwtService.GenerateToken(user);

                            message = $"User '{user.UserName}' has been successfully login!";
                            await _log.LogAsync(message, LogLevel.Information);

                            return Ok(new
                            {
                                token = jwtToken,
                                uid = user.Id,
                                usr = user.UserName,
                            });
                        }
                    }
                }

                message = "Incorrect username or password";
                await _log.LogAsync(message, LogLevel.Error);

                return Unauthorized(new ResponseWrapper<ModelStateDictionary>()
                {
                    StatusCode = 401,
                    Message = message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            try
            {
                var userExists = await UserManager.FindByNameAsync(model.Username);
                var message = "";

                if (userExists != null)
                {
                    message = $"User exist, try login";
                    await _log.LogAsync(message, LogLevel.Warning);

                    return Ok(new ResponseWrapper<object>()
                    {
                        Message = message,
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

                    message = $"User '{user.UserName}' has been added successfully";
                    await _log.LogAsync(message, LogLevel.Information);

                    return Ok(new ResponseWrapper<IdentityResult>()
                    {
                        Data = identityResult
                    });
                }
                else
                {
                    message = $"User '{model.Username}' cannot be added";
                    await _log.LogAsync(message, LogLevel.Error);

                    return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                    {
                        StatusCode = 400,
                        Message = $"{message}, please try again later!"
                    });
                }
            }
            catch (Exception ex)
            {
                await _log.LogAsync(ex.Message, LogLevel.Error);

                return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var currentUser = await CurrentUserAsync;
                var currentId = await UserManager.GetUserIdAsync(currentUser);
                var user = await UserManager.FindByIdAsync(currentId);
                var controllerName = ControllerContext.ActionDescriptor.ControllerName;
                var message = "";

                if (user == null)
                {
                    message = $"User '{currentUser.UserName}' does not exist!";
                    await _log.LogAsync(message, LogLevel.Information);

                    return NotFound(new ResponseWrapper<ModelStateDictionary>()
                    {
                        StatusCode = 404,
                        Message = message
                    });
                }

                message = $"Successfully got user: '{user.UserName}' data";
                await _log.LogAsync(message, LogLevel.Information);

                return Ok(user);
            }
            catch (Exception ex)
            {
                await _log.LogAsync(ex.Message, LogLevel.Error);

                return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateDto user)
        {
            try
            {
                var currentUser = await CurrentUserAsync;
                var currentId = UserManager.GetUserIdAsync(currentUser);
                var userEntity = await UserManager.FindByIdAsync(currentId.Result);
                var message = "";

                if (userEntity == null)
                {
                    message = $"User with ID '{currentId}' not found.";
                    await _log.LogAsync(message, LogLevel.Information);

                    return NotFound(new ResponseWrapper<ModelStateDictionary>()
                    {
                        Data = ModelState,
                        StatusCode = 404,
                        Message = message
                    });
                }

                userEntity.FirstName = user.FirstName;
                userEntity.LastName = user.LastName;
                userEntity.Email = user.Email;
                userEntity.PhoneNumber = user.MobileNumber;
                userEntity.Language = user.Language;
                userEntity.Culture = user.Culture;

                var result = await UserManager.UpdateAsync(userEntity);

                if (!result.Succeeded)
                {
                    message = result.Errors.FirstOrDefault()?.ToString();
                    await _log.LogAsync(message != null ? message : "Something went wrong!", LogLevel.Error);

                    return BadRequest(result.Errors);
                }

                var updatedUser = await UserManager.FindByIdAsync(currentId.Result);

                message = $"User '{currentUser.UserName}' updated successfully!";
                await _log.LogAsync(message, LogLevel.Information);

                return Ok(new ResponseWrapper<ApplicationUser>()
                {
                    Data = updatedUser != null ? updatedUser : userEntity,
                    Message = message,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                await _log.LogAsync(ex.Message, LogLevel.Error);

                return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                {
                    Data = ModelState,
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                var currentUser = await CurrentUserAsync;
                var currentId = await UserManager.GetUserIdAsync(currentUser);
                var user = await UserManager.FindByIdAsync(currentId);
                var message = "";

                if (user == null)
                {
                    message = $"User '{currentUser.UserName}' does not exist!";
                    await _log.LogAsync(message, LogLevel.Information);

                    return NotFound(new ResponseWrapper<ModelStateDictionary>()
                    {
                        Data = ModelState,
                        StatusCode = 404,
                        Message = message
                    });
                }

                await UserManager.DeleteAsync(user);

                message = $"User '{user.UserName}' has been deleted";
                await _log.LogAsync(message, LogLevel.Information);

                return Ok(new ResponseWrapper<object>()
                {
                    Message = message,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                await _log.LogAsync(ex.Message, LogLevel.Error);

                return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                {
                    Data = ModelState,
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("ValidateUserPassword")]
        public async Task<IActionResult> ValidateUserPassword(LoginDto login)
        {
            try
            {
                var user = await UserManager.FindByNameAsync(login.Username);
                var message = "";

                if (user != null)
                {
                    var result = await UserManager.CheckPasswordAsync(user, login.Password);

                    if (result)
                    {
                        message = "Password is valid";
                        await _log.LogAsync(message, LogLevel.Information);

                        return Ok(new ResponseWrapper<object>()
                        {
                            Message = message,
                            StatusCode = 200
                        });
                    }
                    else
                    {
                        message = "Invalid password";
                        await _log.LogAsync(message, LogLevel.Error);
                        
                        return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                        {
                            StatusCode = 400,
                            Message = message
                        });
                    }
                }
                else
                {
                    message = $"User '{login.Username}' not found";
                    await _log.LogAsync(message, LogLevel.Error);

                    return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                    {
                        StatusCode = 400,
                        Message = message
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseWrapper<ModelStateDictionary>()
                {
                    StatusCode = 400,
                    Message = ex.Message
                });
            }
        }
    }
}
