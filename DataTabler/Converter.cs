using System;
using System.Collections.Generic;
using System.Data;

namespace DataTabler
{
    public class Converter
    {
        private readonly Dictionary<Type, Delegate> Converters;

        /// <summary>
        /// Create a new converter.
        /// Each converter instance maintains it's own cache of conversion methods for types.
        /// </summary>
        public Converter()
        {
            Converters = new Dictionary<Type, Delegate>();
        }

        /// <summary>
        /// Converts an IEnumerable to a DataTable.
        /// </summary>
        /// <typeparam name="T">type of each element in data</typeparam>
        /// <param name="data">IEnumerable containing elements of type T</param>
        /// <returns>DataTable with columns for each public property of type T and rows for each element in data.</returns>
        public DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            var converter = GetConverter<T>();
            return converter(data);
        }

        // Get or create a conversion method for type T
        private Func<IEnumerable<T>, DataTable> GetConverter<T>()
        {
            Type type = typeof(T);
            if (Converters.TryGetValue(type, out var converter) && 
                converter is Func<IEnumerable<T>, DataTable> funcConverter)
            {
                return funcConverter;
            }
            var typedConverter = ConverterGenerator.GenerateDataTableConverter<T>();
            Converters.Add(type, typedConverter);
            return typedConverter;
        }
    }
}
