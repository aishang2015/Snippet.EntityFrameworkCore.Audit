namespace Snippet.EntityFrameworkCore.Audit.Model
{
    public class ColumnInfo
    {
        public string PropertyName { get; set; }

        public string FieldName { get; set; }

        public string Value { get; set; }

        public string OriginalValue { get; set; }

        public string CurrentValue { get; set; }
    }
}