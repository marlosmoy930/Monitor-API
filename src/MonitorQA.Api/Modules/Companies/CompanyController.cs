using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Api.Modules.Companies.Models;
using MonitorQA.Data;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.Companies
{
    [Route("company")]
    [ApiController]
    public class CompanyController : AuthorizedController
    {
        private readonly SiteContext _context;

        public CompanyController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] UpdateCompanyRequest request)
        {
            var companyId = CurrentUser.CompanyId;
            var company = await _context.Companies
                .AsQueryable()
                .SingleAsync(x => x.Id == companyId);

            request.UpdateCompany(company);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
