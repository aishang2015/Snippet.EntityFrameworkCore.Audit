using Microsoft.EntityFrameworkCore;
using System;

namespace Snippet.EntityFrameworkCore.Audit.Entity
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string TableName { get; set; }

        public string EntityName { get; set; }

        public string Describe { get; set; }

        public string JsonContent { get; set; }

        public EntityState Classify { get; set; }

        public string CreateBy { get; set; }

        public DateTime CreateAt { get; set; }
    }
}