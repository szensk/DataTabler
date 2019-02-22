DataTabler
----------

Convert an IEnumerable<T> of type T with public properties to a DataTable.
It uses System.Linq.Expressions to generate a method specific for that type.
Generation of the method is slow, so it caches the conversion method for reuse.