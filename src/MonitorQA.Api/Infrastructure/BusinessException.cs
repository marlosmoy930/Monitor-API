using System;
using System.Collections.Generic;

namespace MonitorQA.Api.Infrastructure
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }

        public static class AuditExceptions
        {
            public static readonly BusinessException Completed = new BusinessException("audit/already-completed");
            public static readonly BusinessException Deleted = new BusinessException("audit/already-deleted");
            public static readonly BusinessException NotAvailable = new BusinessException("audit/not-available");
        }

        public static class ScoreSystemExceptions
        {
            public static BusinessException GetScroreSystemHasTemplates(IEnumerable<Guid> ids)
                => new BusinessException($"score-system/has-templates:{string.Join(",", ids)}");
        }

        public static class RoleExceptions
        {
            public static BusinessException GetHasUsers(IEnumerable<Guid> ids)
                => new BusinessException($"role/has-users:{string.Join(",", ids)}");
        }
    }
}
