using AutoMapper;
using Betting.Backend.Core.Middleware;
using Betting.Backend.Core.Services.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Core.Context;
using UserManagement.Core.Repositories;
using UserManagement.Core.Service;
using UserManagement.Domain.Common.Enums;
using UserManagement.Domain.Common.Mapper;
using UserManagement.Domain.Entities;

namespace UserManagement.Core
{
    public static class DependencyInjection
    {
        public static void AddCore(this IServiceCollection services,
                                     IConfiguration configuration,
                                     PlatformEnvironment platformEnvironment,
                                     PlatformType platformType,
                                     IHostBuilder hostBuilder,
                                     int multiplerExpiration = 1)
        {
            var connectionString = configuration["ConnectionStrings:Localhost"];

            if (platformEnvironment == PlatformEnvironment.Production)
                connectionString = configuration["ConnectionStrings:Production"];

            if (platformType == PlatformType.Console || platformType == PlatformType.Web)
            {
                services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(
                    dbContextOptions => dbContextOptions
                        .UseSqlServer(connectionString,
                            sqlServerOptionsAction: sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 5, // Maximum number of retries
                                    maxRetryDelay: TimeSpan.FromSeconds(30), // Maximum delay between retries
                                    errorNumbersToAdd: null); // SQL error numbers to consider for retries
                            })
                );
            }

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 6;
                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
                  .AddDefaultTokenProviders()
                  .AddRoles<IdentityRole>();


            services.AddTransient<UserManager<ApplicationUser>>();
            services.AddAuthentication(configuration, platformEnvironment);
            services.AddSwagger(configuration, platformEnvironment);
            services.AddAutoMapperConfig();
            services.AddApplicationServices(configuration, platformEnvironment, platformType);
        }

        private static void AddAuthentication(this IServiceCollection services,
                                 IConfiguration configuration,
                                 PlatformEnvironment platformEnvironment)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),

                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    };
                }
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                    builder.WithOrigins("https://localhost:7176")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials());
            });
        }

        private static void AddSwagger(this IServiceCollection services,
                                 IConfiguration configuration,
                                 PlatformEnvironment platformEnvironment)
        {
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                    });
            });
        }

        private static void AddApplicationServices(this IServiceCollection services,
                                IConfiguration configuration,
                                PlatformEnvironment platformEnvironment,
                                PlatformType platformType)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(typeof(PlatformEnvironment), platformEnvironment);
            services.AddSingleton(typeof(PlatformType), platformType);

            services.AddHttpContextAccessor();

            services.AddScoped<IMapperService, MapperService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton<ILoggers, ApplicationLogger>();


        }

        public static void AddSwaggerUI(this IServiceProvider service, WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        public static void AddAutoMapperConfig(this IServiceCollection services)
        {
            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        public static void UseCustomMiddlewares(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<JwtMiddleware>();
            builder.UseCors("AllowSpecificOrigin");
        }
    }
}
