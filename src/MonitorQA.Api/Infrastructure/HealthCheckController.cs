using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data;
using MonitorQA.Firebase;

namespace MonitorQA.Api.Infrastructure
{
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly FirebaseUsersService _firebaseUsersService;
        private readonly SiteContext _context;

        public HealthCheckController(
            FirebaseUsersService firebaseUsersService,
            SiteContext context)
        {
            _firebaseUsersService = firebaseUsersService;
            _context = context;
        }

        [HttpGet]
        [Route("api/health")]
        public ActionResult Check()
        {
            return Ok(new { Success = true, Version = 2 });
        }

        [HttpGet]
        [Route("api/test")]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }

        [HttpPost]
        [Route("api/clean-up")]
        public async Task<ActionResult> CleanUp([FromBody] IdModel model)
        {
            var user = _context.Users.Find(model.Id);
            var users = await _context.Users.AsQueryable()
                .Where(u => u.CompanyId == user.CompanyId)
                .ToListAsync();

            await _firebaseUsersService.DeleteUsers(users.Select(u => u.Id));
            return Ok();
        }
    }
}