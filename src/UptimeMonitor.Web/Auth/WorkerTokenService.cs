using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UptimeMonitor.Web.Api.Data;
using UptimeMonitor.Web.Config;

namespace UptimeMonitor.Web.Api.Auth
{
    public class WorkerTokenService
    {
        private readonly WorkerAuthOptions _settings;
        private readonly MonitorDbContext _dbContext;

        public WorkerTokenService(IOptions<WorkerAuthOptions> settings, MonitorDbContext dbContext)
        {
            _settings = settings.Value;
            _dbContext = dbContext;
        }

        public WorkerToken GenerateToken(string workerId, string location)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, workerId),
                new Claim("workerId", workerId),
                new Claim("location", location),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expires = DateTime.UtcNow.AddHours(12);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expires, // Token valid for 12 hours
                signingCredentials: credentials
            );

            return new WorkerToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = expires
            };
        }
    }
}
