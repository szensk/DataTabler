using System;
using System.Collections;
using System.Collections.Generic;
using DataTabler.Test.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataTabler.Test
{
    [TestClass]
    public class ColumnTests
    {
        private Converter sut => new Converter();

        [TestMethod]
        public void SingleIntegerColumnTest()
        {
            var items = new[]
            {
                new {Id = 5},
                new {Id = -4}
            };

            var expected = items.ConvertToDataTable();
            var actual = sut.ToDataTable(items);

            actual.AssertColumns(expected);
            actual.AssertRows(expected);
        }

        [TestMethod]
        public void SingleNullableIntegerColumnTest()
        {
            var items = new[]
            {
                new {Id = (int?) 5},
                new {Id = (int?) -4},
                new {Id = (int?) null}
            };

            var expected = items.ConvertToDataTable();
            var actual = sut.ToDataTable(items);

            actual.AssertColumns(expected);
            actual.AssertRows(expected);
        }

        [TestMethod]
        public void EnumColumnTest()
        {
            var items = new[]
            {
                new EnumDataTest { Id = 5, StringId = Guid.NewGuid().ToString(), TableEnum = EnumTest.Oregano },
                new EnumDataTest { Id = 4, StringId = Guid.NewGuid().ToString(), TableEnum = EnumTest.All }
            };

            var expected = items.ConvertToDataTable();
            var actual = sut.ToDataTable(items);

            actual.AssertColumns(expected);
            actual.AssertRows(expected);
        }

        [TestMethod]
        public void NullableEnumColumnTest()
        {
            var items = new[]
            {
                new NullableEnumDataTest { Id = 5, StringId = Guid.NewGuid().ToString(), TableEnum = EnumTest.Something },
                new NullableEnumDataTest { Id = 4, StringId = Guid.NewGuid().ToString(), TableEnum = null }
            };

            var expected = items.ConvertToDataTable();
            var actual = sut.ToDataTable(items);

            actual.AssertColumns(expected);
            actual.AssertRows(expected);
        }

        [TestMethod]
        public void DerivedColumnTest()
        {
            var items = new[]
            {
                new DerivedDataTest {BaseFee = 5, DerivedFee = 10},
                new DerivedDataTest {BaseFee = 7, DerivedFee = 8}
            };

            var expected = items.ConvertToDataTable();
            var actual = sut.ToDataTable(items);

            actual.AssertColumns(expected);
            actual.AssertRows(expected);
        }

        [TestMethod]
        public void BaseColumnTest()
        {
            IEnumerable<BaseDataTest> items = new[]
            {
                new DerivedDataTest {BaseFee = 5, DerivedFee = 10},
                new DerivedDataTest {BaseFee = 7, DerivedFee = 8}
            };

            var expected = items.ConvertToDataTable();
            var actual = sut.ToDataTable(items);

            actual.AssertColumns(expected);
            actual.AssertRows(expected);
        }
    }
}
