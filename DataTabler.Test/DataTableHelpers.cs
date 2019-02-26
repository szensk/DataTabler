using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace DataTabler.Test
{
    internal static class ReflectionDataTableHelpers
    {
        public static DataTable ConvertToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                if (prop.PropertyType.BaseType == typeof(System.Enum) || 
                    (Nullable.GetUnderlyingType(prop.PropertyType) != null && Nullable.GetUnderlyingType(prop.PropertyType).BaseType == typeof(System.Enum)))
                {
                    Type type;
                    if (Nullable.GetUnderlyingType(prop.PropertyType) == null)
                    {
                        type = Enum.GetUnderlyingType(prop.PropertyType);
                    }
                    else {
                        type = Enum.GetUnderlyingType(Nullable.GetUnderlyingType(prop.PropertyType));
                    }
                    table.Columns.Add(prop.Name, type);
                }
                else
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }
    }
}
