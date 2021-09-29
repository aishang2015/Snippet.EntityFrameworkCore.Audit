using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Snippet.EntityFrameworkCore.Audit.Attribute;
using Snippet.EntityFrameworkCore.Audit.Entity;
using Snippet.EntityFrameworkCore.Audit.Extension;
using Snippet.EntityFrameworkCore.Audit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Snippet.EntityFrameworkCore.Audit
{
    public abstract class AuditLogDbContext : DbContext
    {
        private bool _auditLogEnabled = true;

        private readonly IUserInfoAccessor _userInfoProvider;

        public AuditLogDbContext() : base() { }

        public AuditLogDbContext(DbContextOptions options) : base(options) { }

        public AuditLogDbContext(IUserInfoAccessor UserInfoProvider) : base()
        {
            _userInfoProvider = UserInfoProvider;
        }

        public AuditLogDbContext(DbContextOptions options, IUserInfoAccessor UserInfoProvider) : base(options)
        {
            _userInfoProvider = UserInfoProvider;
        }

        protected void ConfigAuditLog(bool auditLogEnabled)
        {
            _auditLogEnabled = auditLogEnabled;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<AuditLog>();
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            if (_auditLogEnabled)
            {
                // get entires that need to be logged
                var entries = (from e in ChangeTracker.Entries()
                               where (e.State == EntityState.Added ||
                                     e.State == EntityState.Deleted ||
                                     e.State == EntityState.Modified) &&
                                     e.Entity.GetType() != typeof(AuditLog)
                               select e).ToList();

                // get current transaction's id
                var processId = Database.CurrentTransaction?.TransactionId.ToString() ?? Guid.NewGuid().ToString();
                var time = DateTime.Now;
                foreach (var entry in entries)
                {
                    // check if allow to record audit log
                    var tableAuditData = entry.Metadata.ClrType.GetCustomAttributeData<TableAudit>();
                    var isenable = tableAuditData?.ConstructorArguments[0].Value as bool?;
                    if (isenable == null || !isenable.Value)
                    {
                        continue;
                    }

                    // get audit attribute's setting about this entry
                    var log = new AuditLog
                    {
                        TransactionId = processId.ToString(),
                        Classify = entry.State,
                        TableName = entry.Metadata.GetTableName(),
                        EntityName = entry.Entity.GetType().ToString(),
                        CreateAt = time,
                        CreateBy = _userInfoProvider?.GetUserName()
                    };

                    // make describe
                    var describe = entry.State == EntityState.Modified ?
                        tableAuditData?.ConstructorArguments[2].Value?.ToString() :
                        tableAuditData?.ConstructorArguments[1].Value?.ToString();
                    foreach (var p in entry.Properties)
                    {
                        var attributeData = p.Metadata.PropertyInfo.GetCustomAttributeData<ColumnAudit>();
                        var updateTemplate = attributeData?
                            .ConstructorArguments[entry.State == EntityState.Modified ? 1 : 0].Value?.ToString();

                        var columnDic = p.Metadata.PropertyInfo.GetCustomAttributeDatas<ColumnDic>();

                        if (!string.IsNullOrEmpty(updateTemplate))
                        {
                            var currentValue = columnDic
                                .FirstOrDefault(d => d.ConstructorArguments[0].Value?.ToString() == p.CurrentValue?.ToString())?
                                .ConstructorArguments[1].Value?.ToString() ?? p.CurrentValue?.ToString();
                            var originalValue = columnDic
                                .FirstOrDefault(d => d.ConstructorArguments[0].Value?.ToString() == p.OriginalValue?.ToString())?
                                .ConstructorArguments[1].Value?.ToString() ?? p.OriginalValue?.ToString();

                            // value keyword is for add or delete template
                            updateTemplate = updateTemplate.Replace("{value}", currentValue);

                            // original keyword and current keyword is for update template
                            updateTemplate = updateTemplate.Replace("{original}", p.OriginalValue?.ToString());
                            updateTemplate = updateTemplate.Replace("{current}", currentValue);

                            updateTemplate = updateTemplate.Replace("{column}", p.GetColumnName());
                            updateTemplate = updateTemplate.Replace("{name}", p.Metadata.Name);
                        }
                        else
                        {
                            // deault describe template
                            if (entry.State != EntityState.Modified)
                            {
                                updateTemplate = $"{p.Metadata.Name}:{p.CurrentValue}";
                            }
                            else
                            {
                                updateTemplate = $"{p.Metadata.Name}:{p.OriginalValue}->{p.CurrentValue}";
                            }
                        }
                        describe = describe.Replace($"{{{p.Metadata.Name}}}", updateTemplate, true, null);
                    }
                    describe = describe.Replace("{User}", log.CreateBy, true, null);
                    log.Describe = describe;

                    // make json content
                    var props = entry.State switch
                    {
                        EntityState.Added => entry.Properties,
                        EntityState.Deleted => entry.Properties,
                        EntityState.Modified => entry.Properties.Where(p => p.IsModified),
                        _ => new List<PropertyEntry>()
                    };

                    // record all column info
                    var changeInfos = new List<ColumnInfo>();
                    foreach (var prop in props)
                    {
                        changeInfos.Add(new ColumnInfo
                        {
                            PropertyName = prop.Metadata.Name,
                            FieldName = prop.GetColumnName(),
                            OriginalValue = entry.State is EntityState.Modified ? prop.OriginalValue?.ToString() : null,
                            CurrentValue = entry.State is EntityState.Modified ? prop.CurrentValue?.ToString() : null,
                            Value = entry.State is not EntityState.Modified ? prop.OriginalValue?.ToString() : null
                        });
                    }

                    // record all key column info
                    var keyInfos = new List<ColumnInfo>();
                    var keyProps = entry.Metadata.FindPrimaryKey();
                    foreach (var keyProp in keyProps.Properties)
                    {
                        var prop = entry.Property(keyProp.Name);
                        keyInfos.Add(new ColumnInfo
                        {
                            PropertyName = prop.Metadata.Name,
                            FieldName = prop.GetColumnName(),
                            Value = prop.CurrentValue?.ToString()
                        });
                    }

                    log.JsonContent = JsonSerializer.Serialize<object>(new
                    {
                        Operate = Enum.GetName(entry.State),
                        Key = keyInfos,
                        Column = changeInfos
                    }, new JsonSerializerOptions { IgnoreNullValues = true });

                    Add(log);
                }
            }
            return base.SaveChanges();
        }
    }

    public static class Extensions
    {
        internal static string GetColumnName(this PropertyEntry property)
        {
            var storeObjectId = StoreObjectIdentifier.Create(property.Metadata.DeclaringEntityType, StoreObjectType.Table);
            return property.Metadata.GetColumnName(storeObjectId.GetValueOrDefault());
        }

        internal static CustomAttributeData GetCustomAttributeData<T>(this PropertyInfo propertyInfo) where T : System.Attribute
        {
            return propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(T));
        }

        internal static IEnumerable<CustomAttributeData> GetCustomAttributeDatas<T>(this PropertyInfo propertyInfo) where T : System.Attribute
        {
            return propertyInfo.CustomAttributes.Where(a => a.AttributeType == typeof(T));
        }

        internal static CustomAttributeData GetCustomAttributeData<T>(this Type type) where T : System.Attribute
        {
            return type.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(T));
        }
    }
}