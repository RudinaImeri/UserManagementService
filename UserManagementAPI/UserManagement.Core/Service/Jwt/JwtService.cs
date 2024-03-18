using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagement.Domain.Common.Enums;
using UserManagement.Domain.Entities;

namespace Betting.Backend.Core.Services.Jwt
{
    public interface IJwtService
    {
        public Dictionary<string, string> ValidateToken(string token);
        public string GenerateToken(ApplicationUser userInfo);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly PlatformEnvironment _platformEnvironment;

        public JwtService(IConfiguration configuration, PlatformEnvironment platformEnvironment)
        {
            _configuration = configuration;
            _platformEnvironment = platformEnvironment;
        }
        public string GenerateToken(ApplicationUser userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, userInfo.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, userInfo.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, userInfo.Id),
                new Claim("env", _platformEnvironment.ToString())
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpireMinutes"])),
              signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Dictionary<string, string> ValidateToken(string token)
        {
            if (token == null)
                return new Dictionary<string, string>();
            var userClaims = new Dictionary<string, string>();
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = _configuration["Jwt:Issuer"]
                }, out SecurityToken validatedToken);


                var jwtToken = (JwtSecurityToken)validatedToken;
                userClaims = jwtToken.Claims.ToDictionary(x => x.Type, x => x.Value);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IDX10223"))
                {
                    userClaims.Add(ClaimTypes.Expired, "true");
                    //token expired, user logout
                }
            }
            return userClaims;
        }
    }
}
