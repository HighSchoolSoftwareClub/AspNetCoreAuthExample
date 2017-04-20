using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OAuthExample.Models.Auth
{
    public class TokenProviderOptions
    {
        public string Path { get; set; } = "/api/account/signin";

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public TimeSpan Expiration { get; set; } = TimeSpan.FromDays(15);

        public SigningCredentials SigningCredentials { get; set; }

        public Func<string, string, Task<ClaimsIdentity>> IdentityResolver { get; set; }

        public Func<Task<string>> NonceGenerator { get; set; }
            = () => Task.FromResult(Guid.NewGuid().ToString());
    }
}
