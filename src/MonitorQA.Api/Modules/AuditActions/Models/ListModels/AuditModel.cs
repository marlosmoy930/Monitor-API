using System;

namespace MonitorQA.Api.Modules.AuditActions.Models.ListModels
{
    public class AuditModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? Number { get; set; }
    }
}