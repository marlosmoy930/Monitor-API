
using System;

namespace MonitorQA.Api.Infrastructure.Models
{
    public class IdsArrayModel
    {
        public Guid[] Ids { get; set; } = new Guid[0];
    }

    public class IdsArrayModel<T>
    {
        public T[] Ids { get; set; } = new T[0];
    }
}