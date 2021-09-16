using AutoMapper;
using Identity.Data;
using Identity.Models;
using Identity.Models.InputModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public AccountsController(UserManager<AppUser> userManager, IMapper mapper, ApplicationDbContext appDbContext)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Post([FromBody]CreateCustomer customerObj)
        {
            var userIdentity = _mapper.Map<AppUser>(customerObj);
            var result = await _userManager.CreateAsync(userIdentity, customerObj.Password);

            if (!result.Succeeded)
            {
                return BadRequest("User Creation failed");
            }

            return Ok("User Created successfully");
        }
    }
}