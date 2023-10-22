using System;
using MonitorQA.Api.Modules.AuditObjects.Models;

namespace MonitorQA.Api.Modules.AuditActions.Models.ListModels
{
    public class AuditObjectModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public GeoAddress Address { get; set; }
    }
}