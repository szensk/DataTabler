DataTabler
----------

Convert an `IEnumerable<T>` of type T with public properties to a `System.Data.DataTable`. It uses `System.Linq.Expressions` to generate a method specific for that type. Generation of such a method is slow but when reused over many such conversions it is faster than reflection.
