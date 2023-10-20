using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitorQA.Data;
using MonitorQA.Data.Entities;

namespace MonitorQA.Api.Infrastructure
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthorizedController : ControllerBase
    {
        private readonly SiteContext _context;
        private readonly Lazy<User> _user;

        public AuthorizedController(SiteContext context)
        {
            _context = context;
            _user = new Lazy<User>(GetUser);
        }

        protected User CurrentUser => _user.Value;

        protected string GetUserId()
        {
            return User.FindFirst(claim => claim.Type.Equals("user_id", StringComparison.OrdinalIgnoreCase)).Value;
        }

        private User GetUser()
        {
            var userId = GetUserId();
            return _context.Users.AsNoTracking()
                .Include(u => u.Role)
                .Single(u => u.Id == Guid.Parse(userId));
        }
    }
}