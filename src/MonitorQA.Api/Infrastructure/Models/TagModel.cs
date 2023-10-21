using System;

namespace MonitorQA.Api.Infrastructure.Models
{
    public class TagModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }
    }
}