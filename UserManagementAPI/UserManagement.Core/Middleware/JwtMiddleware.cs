﻿using Betting.Backend.Core.Services.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManagement.Core.Context;
using UserManagement.Domain.Entities;

namespace Betting.Backend.Core.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILoggers _log;           

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILoggers log)
        {
            _next = next;
            _configuration = configuration;
            _log = log;
        }

        public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager, IJwtService jwtService)
        {
            var path = context.Request.Path;

            var headerKey = "Authorization";
            if (context.Request.Headers.ContainsKey(headerKey))
            {
                var token = context.Request.Headers["Authorization"].First().Split(" ").Last();
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(token.Trim()))
                {
                    var userClaimes = jwtService.ValidateToken(token);
                    if (userClaimes.Count > 0)
                    {
                        if (userClaimes.ContainsKey(ClaimTypes.Expired))
                        {
                            throw new UnauthorizedAccessException("Token has expired!");
                        }
                        var user = await userManager.FindByIdAsync(userClaimes[JwtRegisteredClaimNames.Sid]);
                        if (user == null)
                        {
                           await _log.LogAsync($"User with id: {userClaimes[JwtRegisteredClaimNames.Sid]} does not exist", LogLevel.Error);

                            throw new UnauthorizedAccessException("User does not exist!");
                        }

                        // attach user to context on successful jwt validation
                        context.Items["User"] = user;
                    }
                }
            }
            await _next(context);
        }
    }
}
