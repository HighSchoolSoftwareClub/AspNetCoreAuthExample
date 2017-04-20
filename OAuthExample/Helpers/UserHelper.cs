using OAuthExample.EF.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OAuthExample
{
    public static class UserHelper
    {
        public static User GetUser(this ClaimsPrincipal claim)
        {
            var values = claim.Identities.FirstOrDefault().Claims;
            return new User();
        }
    }
}
