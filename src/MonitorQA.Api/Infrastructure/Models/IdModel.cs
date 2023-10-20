using System;

namespace MonitorQA.Api.Infrastructure.Models
{
    public class IdModel: IdModel<Guid>
    {
    }

    public class IdModel<T>
    {
        public T Id { get; set; }
    }
}
