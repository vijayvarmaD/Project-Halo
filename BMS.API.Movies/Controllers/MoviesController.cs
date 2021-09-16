using BMS.API.Movies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace API.Movies.Controllers
{
    [Authorize("Logged In")]
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ClaimsPrincipal _caller;
        private readonly MovieInfoService _movieInfoService;
        public MoviesController(IHttpContextAccessor httpContextAccessor, MovieInfoService movieInfoService)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _movieInfoService = movieInfoService;
        }

        [HttpGet("Home")]
        public IActionResult TestMethod()
        {
            // mongo
            var data = _movieInfoService.Read();
            // test
            var userId = _caller.Claims.Single(c => c.Type == "id");
            return Ok(data);
        }
    }
}