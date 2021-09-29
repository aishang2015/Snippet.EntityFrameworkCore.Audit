using Microsoft.EntityFrameworkCore;
using Snippet.EntityFrameworkCore.Audit.Extension;
using System;

namespace Snippet.EntityFrameworkCore.Audit.Sample.Data
{
    public class SampleDbContext : AuditLogDbContext
    {
        public DbSet<SampleUser> TestEntities { get; set; }

        public SampleDbContext(IUserInfoAccessor provider) : base(provider)
        {
            ConfigAuditLog(true);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("Server=127.0.0.1;Port=3306;Database=AuditSampleStore;Uid=root;Pwd=123456;"
                , new MySqlServerVersion(new Version("8.0.21")));
            base.OnConfiguring(optionsBuilder);
        }
    }
}