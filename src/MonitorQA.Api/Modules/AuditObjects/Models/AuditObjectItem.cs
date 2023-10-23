using MonitorQA.Api.Infrastructure.Models;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MonitorQA.Api.Modules.AuditObjects.Models
{
    public class AuditObjectItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<TagModel> Tags { get; set; }

        public IEnumerable<IdNamePairModel<Guid>> ParticipantUserGroups { get; set; }

        public IEnumerable<IdNamePairModel<Guid>> ParticipantUsers { get; set; }

        public IEnumerable<Guid> AuditObjectGroupIds { get; set; }

        public GeoAddress GeoAddress { get; set; }

        public static Expression<Func<AuditObject, AuditObjectItem>> GetSelectExpression()
        {
            return ao => new AuditObjectItem
            {
                Id = ao.Id,
                Name = ao.Name,
                Tags = ao.AuditObjectTags
                    .OrderBy(tags => tags.Tag.Name)
                    .Select(tags => new TagModel
                    {
                        Id = tags.Tag.Id,
                        Name = tags.Tag.Name,
                        Color = tags.Tag.Color
                    }),
                ParticipantUserGroups = ao.AuditObjectUserGroups
                    .Select(aoug => new IdNamePairModel<Guid> { Id = aoug.UserGroupId, Name = aoug.UserGroup.Name }),
                ParticipantUsers = ao.AuditObjectUsers
                    .Where(aou => !aou.User.IsDeleted)
                    .Where(aou => !aou.UserGroupId.HasValue)
                    .Select(aou => new IdNamePairModel<Guid> { Id = aou.UserId, Name = aou.User.Name }),
                AuditObjectGroupIds = ao.AuditObjectAuditObjectGroups.Select(aoaog => aoaog.AuditObjectGroupId),
                GeoAddress = new GeoAddress
                {
                    Lat = ao.Latitude,
                    Lng = ao.Longitude,
                    Name = ao.AddressName,
                    Address = ao.Address
                },
            };
        }
    }
}
