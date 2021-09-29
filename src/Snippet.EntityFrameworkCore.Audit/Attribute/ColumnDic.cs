using System;

namespace Snippet.EntityFrameworkCore.Audit.Attribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ColumnDic : System.Attribute
    {
        public object ColumnValue { get; private set; }

        public string Describe { get; private set; }

        public ColumnDic(object columnValue,
            string describe)
        {
            ColumnValue = columnValue;
            Describe = describe;
        }
    }
}
