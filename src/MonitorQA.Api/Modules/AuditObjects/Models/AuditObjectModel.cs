using Microsoft.EntityFrameworkCore;
using MonitorQA.Data;
using MonitorQA.Data.Entities;
using MonitorQA.Domain;
using MonitorQA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorQA.Api.Modules.AuditObjects.Models
{
    public class AuditObjectModel
    {
        public string? Name { get; set; }

        public GeoAddress? GeoAddress { get; set; }

        public List<Guid>? TagsIds { get; set; }

        public IEnumerable<Guid>? ParticipantUserIds { get; set; }

        public IEnumerable<Guid>? ParticipantUserGroupIds { get; set; }

        public IEnumerable<Guid>? AuditObjectGroupIds { get; set; }

        public IEnumerable<string>? NotificationEmails { get; set; }

        public async Task<ICollection<AuditObjectUser>> CreateParticipants(SiteContext context)
        {
            var userParticipants = (ParticipantUserIds ?? new List<Guid>())
                .Select(userId => new AuditObjectUser
                {
                    UserId = userId
                }).ToList();

            if (ParticipantUserGroupIds == null || !ParticipantUserGroupIds.Any())
            {
                return userParticipants;
            }

            var userGroupIds = ParticipantUserGroupIds.ToList();
            var userUserGroups = await context.UserUserGroups
                .AsNoTracking()
                .Where(uug => userGroupIds.Contains(uug.UserGroupId))
                .ToListAsync();

            var userGroupParticipants = userUserGroups
                .Select(uug => new AuditObjectUser
                {
                    UserId = uug.UserId,
                    UserGroupId = uug.UserGroupId,
                }).ToList();

            return userParticipants
                .Concat(userGroupParticipants)
                .ToList();
        }

        public ICollection<AuditObjectAuditObjectGroup> CreateAuditObjectAuditObjectGroups()
        {
            return (AuditObjectGroupIds ?? new List<Guid>())
                .Select(auditObjectGroupId => new AuditObjectAuditObjectGroup
                {
                    AuditObjectGroupId = auditObjectGroupId
                }).ToList();
        }

        public async Task<bool> UpdateEntity(SiteContext context, Guid auditObjectId, User user)
        {
            var auditObject = await context.AuditObjects
                .Where(AuditObject.Predicates.UserHasAccess(user))
                .Include(ao => ao.AuditObjectTags)
                .Include(ao => ao.AuditObjectUsers)
                .Include(ao => ao.AuditObjectAuditObjectGroups)
                .Include(ao => ao.AuditObjectUserGroups)
                .FirstOrDefaultAsync(ao => ao.Id == auditObjectId);

            if (auditObject == null) return false;

            auditObject.Name = Name ?? auditObject.Name;

            UpdateGeoAddressAndTags(auditObject);

            await AddAndRemovePendingAudits(context, auditObject, user);

            UpdateAuditObjectGroups(auditObject);

            await UpdateParticipants(context, auditObject);

            await context.SaveChangesAsync();
            return true;
        }

        private async Task UpdateParticipants(SiteContext context, AuditObject auditObject)
        {
            if (ParticipantUserIds == null && ParticipantUserGroupIds == null) return;

            var allAuditObjectUsers = new List<AuditObjectUser>();

            if (ParticipantUserIds != null)
            {
                var userParticipants = ParticipantUserIds
                    .Select(userId => new AuditObjectUser
                    {
                        AuditObjectId = auditObject.Id,
                        UserId = userId
                    });
                allAuditObjectUsers.AddRange(userParticipants);
            }

            if (ParticipantUserGroupIds != null)
            {
                var userUserGroups = await context.UserUserGroups
                                    .AsNoTracking()
                                    .Where(uug => !uug.User.IsDeleted)
                                    .Where(uug => ParticipantUserGroupIds.Contains(uug.UserGroupId))
                                    .ToListAsync();

                var groupParticipants = userUserGroups
                    .Select(x => new AuditObjectUser
                    {
                        AuditObjectId = auditObject.Id,
                        UserGroupId = x.UserGroupId,
                        UserId = x.UserId,
                    });
                allAuditObjectUsers.AddRange(groupParticipants);

                var auditObjectUserGroups = ParticipantUserGroupIds
                    .Select(userGroupId => new AuditObjectUserGroup
                    {
                        AuditObjectId = auditObject.Id,
                        UserGroupId = userGroupId
                    })
                    .ToList();
                context.AuditObjectUserGroups.RemoveRange(auditObject.AuditObjectUserGroups);
                context.AuditObjectUserGroups.AddRange(auditObjectUserGroups);
            }

            context.AuditObjectUsers.RemoveRange(auditObject.AuditObjectUsers);
            context.AuditObjectUsers.AddRange(allAuditObjectUsers);
        }

        private async Task AddAndRemovePendingAudits(SiteContext context, AuditObject auditObject, User user)
        {
            if (AuditObjectGroupIds == null) return;

            var existingAuditObjectGroupIds = auditObject.AuditObjectAuditObjectGroups
                .Select(aoaog => aoaog.AuditObjectGroupId);

            var domainModel = new AuditObjectDomainModel(auditObject.Id, existingAuditObjectGroupIds);
            await domainModel.AddAndRemovePendingAudits(context, AuditObjectGroupIds, user);
        }

        private void UpdateAuditObjectGroups(AuditObject auditObject)
        {
            if (AuditObjectGroupIds == null) return;

            auditObject.AuditObjectAuditObjectGroups = auditObject.AuditObjectAuditObjectGroups
                                .Where(aoaog => AuditObjectGroupIds.Contains(aoaog.AuditObjectGroupId))
                                .ToList();

            var newGroups = AuditObjectGroupIds.Where(
                auditObjectGroupId => !auditObject.AuditObjectAuditObjectGroups.Any(
                    aoaog => aoaog.AuditObjectGroupId == auditObjectGroupId))
                .Select(auditObjectGroupId => new AuditObjectAuditObjectGroup
                {
                    AuditObjectGroupId = auditObjectGroupId
                });

            auditObject.AuditObjectAuditObjectGroups.AddRange(newGroups);
        }

        private void UpdateGeoAddressAndTags(AuditObject auditObject)
        {
            auditObject.Longitude = GeoAddress?.Lng;
            auditObject.Latitude = GeoAddress?.Lat;
            auditObject.Address = GeoAddress?.Address;
            auditObject.AddressName = GeoAddress?.Name;

            auditObject.AuditObjectTags = TagsIds?.Select(tid => new AuditObjectTag
            {
                TagId = tid
            }).ToList();
        }
    }
}
