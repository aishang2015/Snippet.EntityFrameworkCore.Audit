using System;

namespace Snippet.EntityFrameworkCore.Audit.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAudit : System.Attribute
    {
        public bool IsEnabled { get; private set; }

        public string UpdateDescribeTemplate { get; private set; }

        public string AddDeleteDescribeTemplate { get; private set; }

        public TableAudit(bool isEnabled = true, string addDeleteDescribeTemplate = null,
            string updateDescribeTemplate = null)
        {
            IsEnabled = isEnabled;
            UpdateDescribeTemplate = updateDescribeTemplate;
            AddDeleteDescribeTemplate = addDeleteDescribeTemplate;
        }
    }
}