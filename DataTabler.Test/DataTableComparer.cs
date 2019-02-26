using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataTabler.Test
{
    public static class DataTableComparer
    {
        public static bool AssertRows(this DataTable left, DataTable right)
        {
            if (left.Rows.Count != right.Rows.Count)
            {
                throw new AssertFailedException($"Row counts not equal. Actual: {left.Rows.Count}. Expected {right.Rows.Count}");
            }
            for (int i = 0; i < left.Rows.Count; i++)
            {
                var leftItems = left.Rows[i].ItemArray;
                var rightItems = right.Rows[i].ItemArray;
                if (!leftItems.SequenceEqual(rightItems))
                {
                    throw new AssertFailedException($"Not equal: {left} {right}");
                }
            }
            return true;
        }

        public static bool AssertColumns(this DataTable left, DataTable right)
        {
            if (left.Columns.Count != right.Columns.Count)
            {
                throw new AssertFailedException($"Columns counts not equal. Actual: {left.Rows.Count}. Expected {right.Rows.Count}");
            }
            for(int i = 0; i < left.Columns.Count; i++)
            {
                DataColumn col1 = left.Columns[i];
                DataColumn col2 = right.Columns[i];
                string reason = null;
                if (col1.ColumnName != col2.ColumnName)
                {
                    reason = $"Column Name Mismatch. Actual: {col1.ColumnName}. Expected {col2.ColumnName}";
                }

                if (col1.DataType != col2.DataType)
                {
                    reason = $"Column Type Mismatch. Actual: {col1.DataType}. Expected {col2.DataType}";
                }

                if (col1.AllowDBNull != col2.AllowDBNull)
                {
                    reason = $"DN Null Mismatch. Actual: {col1.AllowDBNull}. Expected {col2.AllowDBNull}";
                }

                if (reason != null) throw new AssertFailedException(reason);
            }
            return true;
        }
    }
}
