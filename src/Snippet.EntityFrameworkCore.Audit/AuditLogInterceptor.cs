using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Snippet.EntityFrameworkCore.Audit
{
    public class AuditLogInterceptor : SaveChangesInterceptor
    {
    }
}