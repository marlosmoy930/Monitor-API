using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Modules.Notifications
{
    [Route("notifications")]
    [ApiController]
    public class NotificationsController : AuthorizedController
    {
        private readonly SiteContext _context;

        public NotificationsController(SiteContext context) : base(context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("device")]
        public async Task<IActionResult> AddDevice(UserDeviceModel model)
        {
            var token = model.RegistrationToken;

            if (string.IsNullOrEmpty(token)) throw new InvalidOperationException(nameof(token));

            var userDevice = await _context.UsersDevices
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserId == CurrentUser.Id && t.RegistrationToken == token);

            if (userDevice == null)
            {
                _context.UsersDevices.Add(new UserDevice
                {
                    UserId = CurrentUser.Id,
                    RegistrationToken = token,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
