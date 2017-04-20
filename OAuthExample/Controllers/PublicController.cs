using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OAuthExample.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PublicController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Json("It's public. You are now Anonymous");
        }
    }
}