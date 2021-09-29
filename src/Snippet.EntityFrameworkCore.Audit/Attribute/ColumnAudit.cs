using System;
using System.Collections.Generic;
using System.Linq;

namespace Snippet.EntityFrameworkCore.Audit.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAudit : System.Attribute
    {
        public string UpdateDescribeTemplate { get; private set; }

        public string AddDeleteDescribeTemplate { get; private set; }

        public ColumnAudit(string addDeleteDescribeTemplate = null,
            string updateDescribeTemplate = null)
        {
            AddDeleteDescribeTemplate = addDeleteDescribeTemplate;
            UpdateDescribeTemplate = updateDescribeTemplate;
        }
    }
}