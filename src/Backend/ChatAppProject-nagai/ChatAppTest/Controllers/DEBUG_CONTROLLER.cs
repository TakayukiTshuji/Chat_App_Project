using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DEBUG_CONTROLLER : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            var op = new CookieOptions();
            op.Expires = DateTimeOffset.Now.AddSeconds(15);
            Response.Cookies.Append("te", "here, have some cookies!", op);
            return "Owo";
        }

        [HttpPost]
        public string Post()
        {
            if (!Request.Cookies.ContainsKey("te"))
            {
                return "no cookies for you!";
            }
            return Request.Cookies["te"];
        }
    }
}
