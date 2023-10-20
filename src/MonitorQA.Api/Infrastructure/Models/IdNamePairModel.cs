namespace MonitorQA.Api.Infrastructure.Models
{
    public class IdNamePairModel<T>
    {
        public T Id { get; set; }

        public string? Name { get; set; }
    }
}
