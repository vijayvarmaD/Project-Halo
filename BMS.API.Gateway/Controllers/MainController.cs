using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        public IActionResult Login()
        {
            Response.Cookies.Append("jwt", "12345", new CookieOptions { HttpOnly = true });
            return Ok("Done");
        }

        [HttpGet]
        [Route("seecookies")]
        public IActionResult See()
        {
            var cookies = Request.Cookies["jwt"];
            return Ok("Seen");
        }
    }
}