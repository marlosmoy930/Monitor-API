using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Api.Infrastructure;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MonitorQA.Api.Modules.NotificationSettings
{
    [Route("notification-settings")]
    [ApiController]
    public class NotificationSettingsController : AuthorizedController
    {
        private readonly SiteContext _context;

        public NotificationSettingsController(
            SiteContext context) : base(context)
        {
            this._context = context;
        }

        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<NotificationSettingsModel>> Get()
        {
            var settings = await _context.NotificationSettings
                .AsNoTracking()
                .Where(ns => ns.UserId == CurrentUser.Id)
                .ToListAsync();

            var settingModels = NotificationSetting
                .GetAllSettings(CurrentUser.Role, CurrentUser.Id, settings)
                .Select(NotificationSettingsModel.CreateModel)
                .OrderBy(s => s.InteractionType)
                .ThenBy(s => s.NotificationEventType)
                .ToList();

            return settingModels;
        }

        [HttpPut]
        [Route("")]
        public async Task Update(IEnumerable<NotificationSettingsModel> settingModels)
        {
            var settings = await _context.NotificationSettings
                .AsQueryable()
                .Where(ns => ns.UserId == CurrentUser.Id)
                .ToListAsync();
            _context.NotificationSettings.RemoveRange(settings);

            var entities = settingModels.Select(ns => ns.CreateEntity(CurrentUser.Id));
            _context.NotificationSettings.AddRange(entities);

            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [Route("unsubscribe")]
        [AllowAnonymous]
        public async Task Unsubscribe([FromQuery] Guid userId, [FromQuery] NotificationEventType notificationEventType)
        {
            var setting = await _context.NotificationSettings
                .AsQueryable()
                .Where(ns => ns.UserId == userId)
                .Where(ns => ns.NotificationEventType == notificationEventType)
                .SingleOrDefaultAsync();

            if (setting != null)
            {
                setting.IsEnabled = false;
            }
            else
            {
                var userRole = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.Role)
                    .SingleAsync();

                var settings = NotificationSetting.GetDefaultSettings(userRole, userId);
                setting = settings.Single(ns => ns.NotificationEventType == notificationEventType);
                setting.IsEnabled = false;
                _context.NotificationSettings.AddRange(settings);
            }

            await _context.SaveChangesAsync();
        }
    }
}
