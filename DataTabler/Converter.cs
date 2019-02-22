using System;
using System.Collections.Generic;
using System.Data;

namespace DataTabler
{
    public class Converter
    {
        private readonly Dictionary<Type, Delegate> Converters;

        public Converter()
        {
            Converters = new Dictionary<Type, Delegate>();
        }

        public DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            var converter = GetConverter<T>();
            return converter(data);
        }

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
