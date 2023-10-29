using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MonitorQA.Api.Modules.Registration.Models;
using MonitorQA.Cloud.Messaging;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Firebase;
using MonitorQA.I18n;
using MonitorQA.Notifications;

namespace MonitorQA.Api.Modules.Registration
{
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly CloudMessagePublisher _publisher;
        private readonly SiteContext _context;
        private readonly FirebaseUsersService _firebaseUsersService;

        public RegistrationController(
            CloudMessagePublisher publisher,
            SiteContext context, 
            FirebaseUsersService firebaseUsersService)
        {
            this._publisher = publisher;
            _context = context;
            _firebaseUsersService = firebaseUsersService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostRegister([FromBody] RegisterModel model)
        {
            if (model == null)
                return BadRequest();

            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = model.CompanyName,
                CreatedAt = DateTime.UtcNow
            };

            var roles = PredefinedRoles.GetRoles(company);
            var adminRole = roles[0];

            var scoreSystems = PredefinedScoreSystems.GetScoreSystems(company);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = model.CompanyAdmin.FullName,
                Email = model.CompanyAdmin.Email,
                IsActive = true,
                Role = adminRole,
                Company = company,
                Locale = Locales.Default,
                HasResetPassword = true
            };

            try
            {
                await _firebaseUsersService.CreateUser(company.Id, user, adminRole, model.CompanyAdmin.Password);
            }
            catch (FirebaseException e)
            {
                return Conflict(e.Error);
            }

            _context.Roles.AddRange(roles);
            _context.ScoreSystems.AddRange(scoreSystems);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var message = new UserWelcomeMessage
            {
                UserId = user.Id,
            };
            await _publisher.Publish(message);

            return Ok();
        }
    }
}