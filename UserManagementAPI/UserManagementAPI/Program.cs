using Microsoft.AspNetCore.ResponseCompression;
using System.Diagnostics;
using UserManagement.Core;
using UserManagement.Core.Context;
using UserManagement.Domain.Common.Enums;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.AddJsonFile("appsettings.json",
                optional: true,
                reloadOnChange: false)
                .Build();
var platformEnvironment = PlatformEnvironment.Production;
var platformType = PlatformType.Web;
var hostBuilder = builder.Host;

if (Debugger.IsAttached)
{
    platformEnvironment = PlatformEnvironment.Local;
}

builder.Services.AddCore(configuration, platformEnvironment, platformType, hostBuilder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();
app.Services.AddSeedData();

app.Services.AddSwaggerUI(app);

app.UseCustomMiddlewares();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
