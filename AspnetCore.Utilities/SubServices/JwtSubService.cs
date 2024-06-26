using AspnetCore.Utilities.AppsettingVariables;
using AspnetCore.Utilities.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspnetCore.Utilities.SubServices
{
    public interface IJwtSubService
    {
        string CreateJsonWebToken(TokenClaimModel tokenClaim, string tokenSecret, int tokenExpires);
        TokenClaimModel ValidateJsonWebToken(string token, string tokenSecret, bool validateLifetime = true);

        string CreateAccessToken(TokenClaimModel tokenClaim);
        TokenClaimModel ValidateAccessToken(string token);
        TokenClaimModel DecodeAccessToken(string token);

        string CreateRefreshToken(TokenClaimModel tokenClaim);
        TokenClaimModel ValidateRefreshToken(string token);
        TokenClaimModel DecodeRefreshToken(string token);
    }

    public class JwtSubService : IJwtSubService
    {
        private readonly EnvironmentVariable _environmentVariable;

        public JwtSubService(IOptions<EnvironmentVariable> environmentVariableOption)
        {
            _environmentVariable = environmentVariableOption.Value;
        }

        public string CreateJsonWebToken(TokenClaimModel tokenClaim, string tokenSecret, int tokenExpires)
        {
            var claims = new[]
            {
                new Claim("UserId", tokenClaim.UserId.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _environmentVariable.JwtIssuer,
                _environmentVariable.JwtAudience,
                claims,
                null,
                DateTime.Now.AddMinutes(tokenExpires),
                credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenClaimModel ValidateJsonWebToken(string token, string tokenSecret, bool validateLifetime = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = validateLifetime,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _environmentVariable.JwtIssuer,
                    ValidAudience = _environmentVariable.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSecret)),
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                return new TokenClaimModel
                {
                    UserId = int.Parse(jwtToken.Claims.First(x => x.Type == "UserId").Value)
                };
            }
            catch
            {
                return null;
            }
        }

        public string CreateAccessToken(TokenClaimModel tokenClaim) =>
            CreateJsonWebToken(tokenClaim, _environmentVariable.JwtAccessTokenSecret, _environmentVariable.JwtAccessTokenExpires);

        public string CreateRefreshToken(TokenClaimModel tokenClaim) =>
            CreateJsonWebToken(tokenClaim, _environmentVariable.JwtRefreshTokenSecret, _environmentVariable.JwtRefreshTokenExpires);

        public TokenClaimModel ValidateAccessToken(string token) =>
            ValidateJsonWebToken(token, _environmentVariable.JwtAccessTokenSecret);

        public TokenClaimModel DecodeAccessToken(string token) =>
            ValidateJsonWebToken(token, _environmentVariable.JwtAccessTokenSecret, false);

        public TokenClaimModel ValidateRefreshToken(string token) =>
            ValidateJsonWebToken(token, _environmentVariable.JwtRefreshTokenSecret);

        public TokenClaimModel DecodeRefreshToken(string token) =>
            ValidateJsonWebToken(token, _environmentVariable.JwtRefreshTokenSecret, false);
    }
}