using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OAuthExample.EF;
using OAuthExample.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using OAuthExample.Models;

namespace OAuthExample.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly DataContext _db;

        public AuthMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options,
            DataContext db)
        {
            _next = next;
            _options = options.Value;
            _db = db;
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                return _next(context);
            }

            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            return GenerateToken(context);
        }

        private Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            EF.Tables.User user = _db.Users.SingleOrDefault(p => p.Username == username && p.Password == password);
            if (user != null)
                return Task.FromResult(
                        new ClaimsIdentity(new System.Security.Principal.GenericIdentity(username, "Token"),
                            new Claim[] {
                                new Claim("User",user.Username),
                                new Claim("UserId",user.Id.ToString()),
                            }));

            return Task.FromResult<ClaimsIdentity>(null);
        }

        private async Task GenerateToken(HttpContext context)
        {
            var username = context.Request.Form["username"];
            var password = context.Request.Form["password"];

            var identity = await GetIdentity(username, password);
            if (identity == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(JsonConvert.SerializeObject("Invalid username or password."));
                return;
            }

            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Authentication,username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.Second.ToString(), ClaimValueTypes.Integer64)
            };

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new ApiResultModel<object>()
            {
                IsSuccess = true,
                Data = new
                {
                    AccessToken = encodedJwt,
                    ExpiresIn = (int)_options.Expiration.TotalSeconds
                }
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            }));
        }
    }
}
