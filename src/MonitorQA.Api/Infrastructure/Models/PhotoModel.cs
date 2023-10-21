using System;

namespace MonitorQA.Api.Infrastructure.Models
{
    public class PhotoModel
    {
        public Guid Id { get; set; }

        public string? Note { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}