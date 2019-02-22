using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;

namespace DataTabler
{
    internal static class ConverterGenerator
    {
        private static Expression AddColumnExpression(Expression table, string propName, Type type)
        {
            var tableColumn = Expression.Property(table, "Columns");

            //Find method DataColumnCollection.Add(string, Type)
            var method = typeof(DataColumnCollection).GetMethod("Add", new[] {typeof(string), typeof(Type)});

            //Call: table.Columns.Add(string, Type)
            return Expression.Call(
                tableColumn,
                method,
                Expression.Constant(propName),
                Expression.Constant(type)
            );
        }

        private static Expression FillTableExpression(Expression table, Expression item, PropertyDescriptorCollection properties)
        {
            var dbNull = Expression.Constant(DBNull.Value, typeof(object));
            var row = Expression.Variable(typeof(DataRow), "row");
            var tableNewRow = Expression.Call(table, typeof(DataTable).GetMethod("NewRow"));
            var assignment = Expression.Assign(row, tableNewRow);
            List<Expression> expressions = new List<Expression>();

            //Add initialize: row = table.NewRow();
            expressions.Add(assignment);

            //Add property initialization
            foreach (PropertyDescriptor prop in properties)
            {
                var indexer = typeof(DataRow).GetProperty("Item", typeof(object), new[] {typeof(string)});
                var rowProp = Expression.Property(row, indexer, Expression.Constant(prop.Name));
                var itemProp = Expression.Property(item, prop.Name);
                Expression itemValue;
                if (Nullable.GetUnderlyingType(prop.PropertyType) == null)
                {
                    itemValue = Expression.Convert(itemProp, typeof(object));
                }
                else
                {
                    itemValue = Expression.Coalesce(itemProp, dbNull);
                }

                //If nullable: row["propertyName"] = item.propertyName;
                //else: row["propertyName"] = item.propertyName ?? DBNull.Value;
                var rowPropAssignment = Expression.Assign(rowProp, itemValue);
                expressions.Add(rowPropAssignment);
            }


            //Find method DataRowCollection.Add(DataRow)
            var tableRowAddMethod = typeof(DataRowCollection).GetMethod("Add", new[] {typeof(DataRow)});
            var tableRow = Expression.Property(table, "Rows");

            //Add row to table: table.Rows.Add(row);
            var tableRowAdd = Expression.Call(tableRow, tableRowAddMethod, row);
            expressions.Add(tableRowAdd);

            return Expression.Block(new[] {row}, expressions);
        }

        private static Expression ForEachExpression(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(
                new[]
                {
                    enumeratorVar
                },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        test: Expression.Equal(moveNextCall, Expression.Constant(true)),
                        ifTrue: Expression.Block(
                            new[]
                            {
                                loopVar
                            },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        ifFalse: Expression.Break(breakLabel)
                    ),
                    breakLabel)
            );

            return loop;
        }

        public static Func<IEnumerable<T>, DataTable> GenerateDataTableConverter<T>()
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var data = Expression.Parameter(typeof(IEnumerable<T>), "data");
            var table = Expression.Variable(typeof(DataTable), "table");
            var assignment = Expression.Assign(table, Expression.New(typeof(DataTable)));
            List<Expression> expressions = new List<Expression>();

            //Add expression to initialize data table
            expressions.Add(assignment);

            //Add expressions that set columns and column types in the data table
            foreach (PropertyDescriptor prop in properties)
            {
                Type propType;
                if (prop.PropertyType.BaseType == typeof(Enum) ||
                    (Nullable.GetUnderlyingType(prop.PropertyType) != null &&
                     Nullable.GetUnderlyingType(prop.PropertyType).BaseType == typeof(Enum)))
                {
                    Type type;
                    if (Nullable.GetUnderlyingType(prop.PropertyType) == null)
                    {
                        type = Enum.GetUnderlyingType(prop.PropertyType);
                    }
                    else
                    {
                        type = Enum.GetUnderlyingType(Nullable.GetUnderlyingType(prop.PropertyType));
                    }

                    propType = type;
                }
                else
                {
                    propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                }

                expressions.Add(AddColumnExpression(table, prop.Name, propType));
            }

            //Add foreach expression which adds a new row for each item in the data.
            var item = Expression.Variable(typeof(T), "item");
            var forEach = ForEachExpression(data, item, FillTableExpression(table, item, properties));
            expressions.Add(forEach);

            //Add return expression which returns the data table.
            var returnTarget = Expression.Label(typeof(DataTable));
            var returnExpression = Expression.Return(returnTarget, table, typeof(DataTable));
            var returnLabel = Expression.Label(returnTarget, table);
            expressions.Add(returnExpression);
            expressions.Add(returnLabel);

            //Generate the body block from the expressions.
            var body = Expression.Block(new[] {table}, expressions);

            //Create & compile the lambda using the block of all expressions and the data parameter as input.
            var lambda = Expression.Lambda<Func<IEnumerable<T>, DataTable>>(body, data);
            return lambda.Compile();
        }
    }
}
