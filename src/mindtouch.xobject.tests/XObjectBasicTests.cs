using System;
using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    // TODO: need test for converter marking up a collection
    [TestFixture]
    public class BasicTests {

        public interface ISimpleTest : IXObject {
            string Name { get; }
        }

        [Test]
        public void Can_get_XDoc_back_from_IFoo() {
            XDoc doc = new XDoc("foo").Elem("name", "blah");
            ISimpleTest foo = XObjectBuilder.FromXDoc<ISimpleTest>(doc);
            Assert.AreEqual(doc["name"].AsText,foo.Name);
            Assert.AreEqual(doc, foo.AsDocument);
        }

        [Test]
        public void Can_get_IFoo_Name() {
            ISimpleTest foo = XObjectBuilder.FromXDoc<ISimpleTest>(new XDoc("foo").Elem("name", "blah"));
            Assert.AreEqual("blah", foo.Name);
        }

        [Test]
        public void Can_get_IFoo_Name_even_if_doc_does_not_contain_xpath() {
            ISimpleTest foo = XObjectBuilder.FromXDoc<ISimpleTest>(new XDoc("foo"));
            Assert.IsNull(foo.Name);
        }

        [Test]
        public void Can_get_IFoo_name_via_xpath() {
            ISimpleTest foo = XObjectBuilder.FromXDoc<ISimpleTest>(new XDoc("foo").Elem("name", "blah"));
            Assert.AreEqual("blah", foo["name"].AsText);
        }

        public interface IAllXDocSupportedTypeConversions : IXObject {
            bool Bool { get; set; }
            bool? BoolNullable { get; set; }
            byte Byte { get; set; }
            byte? ByteNullable { get; set; }
            byte[] Bytes { get; set; }
            DateTime DateTime { get; set; }
            DateTime? DateTimeNullable { get; set; }
            decimal Decimal { get; set; }
            decimal? DecimalNullable { get; set; }
            double Double { get; set; }
            double? DoubleNullable { get; set; }
            float Float { get; set; }
            float? FloatNullable { get; set; }
            int Int { get; set; }
            int? IntNullable { get; set; }
            long Long { get; set; }
            long? LongNullable { get; set; }
            sbyte SByte { get; set; }
            sbyte? SByteNullable { get; set; }
            short Short { get; set; }
            short? ShortNullable { get; set; }
            uint UInt { get; set; }
            uint? UIntNullable { get; set; }
            ulong ULong { get; set; }
            ulong? ULongNullable { get; set; }
            XUri Uri { get; set; }
            ushort UShort { get; set; }
            ushort? UShortNullable { get; set; }
        }

        [Test]
        public void All_XDoc_As_types_auto_convert_and_use_convention_for_xpath() {
            XDoc doc = new XDoc("doc")
                .Elem("bool", "true")
                .Elem("bool-nullable", "true")
                .Elem("byte", "1")
                .Elem("byte-nullable", "1")
                .Elem("bytes", Convert.ToBase64String(new byte[] { 1, 2, 3 }))
                .Elem("date-time", "1/1/2009")
                .Elem("date-time-nullable", "1/1/2009")
                .Elem("decimal", "1.2")
                .Elem("decimal-nullable", "1.2")
                .Elem("double", "1.2")
                .Elem("double-nullable", "1.2")
                .Elem("float", "1.2")
                .Elem("float-nullable", "1.2")
                .Elem("int", "10")
                .Elem("int-nullable", "10")
                .Elem("long", "10000000000")
                .Elem("long-nullable", "10000000000")
                .Elem("s-byte", "1")
                .Elem("s-byte-nullable", "1")
                .Elem("short", "1")
                .Elem("short-nullable", "1")
                .Elem("u-int", "10")
                .Elem("u-int-nullable", "10")
                .Elem("u-long", "10000000000")
                .Elem("u-long-nullable", "10000000000")
                .Elem("uri", "http://foo.com/bar")
                .Elem("u-short", "1")
                .Elem("u-short-nullable", "1");
            IAllXDocSupportedTypeConversions x = XObjectBuilder.FromXDoc<IAllXDocSupportedTypeConversions>(doc);
            Assert.AreEqual(true, x.Bool);
            Assert.AreEqual(true, x.BoolNullable.Value);
            Assert.AreEqual(1, x.Byte);
            Assert.AreEqual(1, x.ByteNullable.Value);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, x.Bytes);
            Assert.AreEqual(DateTime.Parse("1/1/2009").ToUniversalTime(), x.DateTime);
            Assert.AreEqual(DateTime.Parse("1/1/2009").ToUniversalTime(), x.DateTimeNullable.Value);
            Assert.AreEqual(1.2, x.Decimal);
            Assert.AreEqual(1.2, x.DecimalNullable.Value);
            Assert.AreEqual(1.2, x.Double);
            Assert.AreEqual(1.2, x.DoubleNullable.Value);
            Assert.AreEqual(1.2f, x.Float);
            Assert.AreEqual(1.2f, x.FloatNullable.Value);
            Assert.AreEqual(10, x.Int);
            Assert.AreEqual(10, x.IntNullable.Value);
            Assert.AreEqual(10000000000, x.Long);
            Assert.AreEqual(10000000000, x.LongNullable.Value);
            Assert.AreEqual(1, x.SByte);
            Assert.AreEqual(1, x.SByteNullable.Value);
            Assert.AreEqual(1, x.Short);
            Assert.AreEqual(1, x.ShortNullable.Value);
            Assert.AreEqual(10, x.UInt);
            Assert.AreEqual(10, x.UIntNullable.Value);
            Assert.AreEqual(10000000000, x.ULong);
            Assert.AreEqual(10000000000, x.ULongNullable.Value);
            Assert.AreEqual("http://foo.com/bar", x.Uri.ToString());
            Assert.AreEqual(1, x.UShort);
            Assert.AreEqual(1, x.UShortNullable.Value);
        }

        [Test]
        public void All_XDoc_As_types_can_set() {

            IAllXDocSupportedTypeConversions x = XObjectBuilder.New<IAllXDocSupportedTypeConversions>();
            x.Bool = true;
            Assert.AreEqual(true, x.Bool);
            x.BoolNullable = true;
            Assert.AreEqual(true, x.BoolNullable.Value);
            x.Byte = 1;
            Assert.AreEqual(1, x.Byte);
            x.ByteNullable = 1;
            Assert.AreEqual(1, x.ByteNullable.Value);
            x.Bytes = new byte[] { 1, 2, 3 };
            Assert.AreEqual(new byte[] { 1, 2, 3 }, x.Bytes);
            DateTime dt = DateTime.UtcNow.Date;
            x.DateTime = dt;
            Assert.AreEqual(dt, x.DateTime);
            x.DateTimeNullable = dt;
            Assert.AreEqual(dt, x.DateTimeNullable.Value);
            x.Decimal = 1.2m;
            Assert.AreEqual(1.2, x.Decimal);
            x.DecimalNullable = 1.2m;
            Assert.AreEqual(1.2, x.DecimalNullable.Value);
            x.Double = 1.2d;
            Assert.AreEqual(1.2, x.Double);
            x.DoubleNullable = 1.2d;
            Assert.AreEqual(1.2, x.DoubleNullable.Value);
            x.Float = 1.2f;
            Assert.AreEqual(1.2f, x.Float);
            x.FloatNullable = 1.2f;
            Assert.AreEqual(1.2f, x.FloatNullable.Value);
            x.Int = 10;
            Assert.AreEqual(10, x.Int);
            x.IntNullable = 10;
            Assert.AreEqual(10, x.IntNullable.Value);
            x.Long = 10000000000;
            Assert.AreEqual(10000000000, x.Long);
            x.LongNullable = 10000000000;
            Assert.AreEqual(10000000000, x.LongNullable.Value);
            x.SByte = 1;
            Assert.AreEqual(1, x.SByte);
            x.SByteNullable = 1;
            Assert.AreEqual(1, x.SByteNullable.Value);
            x.Short = 1;
            Assert.AreEqual(1, x.Short);
            x.ShortNullable = 1;
            Assert.AreEqual(1, x.ShortNullable.Value);
            x.UInt = 10;
            Assert.AreEqual(10, x.UInt);
            x.UIntNullable = 10;
            Assert.AreEqual(10, x.UIntNullable.Value);
            x.ULong = 10000000000;
            Assert.AreEqual(10000000000, x.ULong);
            x.ULongNullable = 10000000000;
            Assert.AreEqual(10000000000, x.ULongNullable.Value);
            x.Uri = new XUri("http://foo.com/bar");
            Assert.AreEqual("http://foo.com/bar", x.Uri.ToString());
            x.UShort = 1;
            Assert.AreEqual(1, x.UShort);
            x.UShortNullable = 1;
            Assert.AreEqual(1, x.UShortNullable.Value);
        }

        public interface IBasicSetters : IXObject {
            string Elem { get; set; }
            string attr { get; set; }
            string Sub_Path { get; set; }
            string Sub_Path_with_attr { get; set; }
            string elem_withAttr { get; set; }
        }

        [Test]
        public void Can_set_existing_element() {
            XDoc doc = new XDoc("doc").Elem("elem", "foo");
            IBasicSetters x = XObjectBuilder.FromXDoc<IBasicSetters>(doc);
            Assert.AreEqual("foo", x.Elem);
            x.Elem = "bar";
            Assert.AreEqual("bar", x.Elem);
        }

        [Test]
        public void Can_access_new_value_by_xpath_after_set() {
            XDoc doc = new XDoc("doc").Elem("elem", "foo");
            IBasicSetters x = XObjectBuilder.FromXDoc<IBasicSetters>(doc);
            Assert.AreEqual("foo", x.Elem);
            Assert.AreEqual("foo", x["elem"].AsText);
            x.Elem = "bar";
            Assert.AreEqual("bar", x.Elem);
            Assert.AreEqual("bar", x["elem"].AsText);
        }

        [Test]
        public void Can_set_element_on_new_object() {
            IBasicSetters x = XObjectBuilder.New<IBasicSetters>();
            Assert.AreEqual(null, x.Elem);
            x.Elem = "bar";
            Assert.AreEqual("bar", x.Elem);
            Assert.AreEqual("<basic-setters><elem>bar</elem></basic-setters>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_existing_attribute() {
            XDoc doc = new XDoc("doc").Attr("attr", "foo");
            IBasicSetters x = XObjectBuilder.FromXDoc<IBasicSetters>(doc);
            Assert.AreEqual("foo", x.attr);
            x.attr = "bar";
            Assert.AreEqual("bar", x.attr);
        }

        [Test]
        public void Can_set_attribute_on_new_object() {
            IBasicSetters x = XObjectBuilder.New<IBasicSetters>();
            Assert.AreEqual(null, x.attr);
            x.attr = "bar";
            Assert.AreEqual("bar", x.attr);
            Assert.AreEqual("<basic-setters attr=\"bar\" />", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_existing_sub_path_elem() {
            XDoc doc = new XDoc("doc").Start("sub").Elem("path", "foo").End();
            IBasicSetters x = XObjectBuilder.FromXDoc<IBasicSetters>(doc);
            Assert.AreEqual("foo", x.Sub_Path);
            x.Sub_Path = "bar";
            Assert.AreEqual("bar", x.Sub_Path);
        }

        [Test]
        public void Can_set_sub_path_elem_on_new_object() {
            IBasicSetters x = XObjectBuilder.New<IBasicSetters>();
            Assert.AreEqual(null, x.Sub_Path);
            x.Sub_Path = "bar";
            Assert.AreEqual("bar", x.Sub_Path);
            Assert.AreEqual("<basic-setters><sub><path>bar</path></sub></basic-setters>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_existing_sub_path_with_attribute() {
            XDoc doc = new XDoc("doc").Start("sub").Elem("path", "foo").End();
            IBasicSetters x = XObjectBuilder.FromXDoc<IBasicSetters>(doc);
            Assert.AreEqual("foo", x.Sub_Path);
            x.Sub_Path = "bar";
            Assert.AreEqual("bar", x.Sub_Path);
        }

        [Test]
        public void Can_set_sub_path_elem_with_attribute_on_new_object() {
            IBasicSetters x = XObjectBuilder.New<IBasicSetters>();
            Assert.AreEqual(null, x.Sub_Path_with_attr);
            x.Sub_Path_with_attr = "bar";
            Assert.AreEqual("bar", x.Sub_Path_with_attr);
            Assert.AreEqual("<basic-setters><sub><path><with attr=\"bar\" /></path></sub></basic-setters>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_elem_then_attr_on_existing_doc() {
            XDoc doc = new XDoc("doc").Start("elem").Attr("with-attr", "abc").Value("xyz").End();
            IBasicSetters x = XObjectBuilder.FromXDoc<IBasicSetters>(doc);
            Assert.AreEqual("abc", x.elem_withAttr);
            Assert.AreEqual("xyz", x.Elem);
            x.Elem = "bar";
            Assert.AreEqual("abc", x.elem_withAttr);
            Assert.AreEqual("bar", x.Elem);
            x.elem_withAttr = "baz";
            Assert.AreEqual("baz", x.elem_withAttr);
            Assert.AreEqual("bar", x.Elem);
        }

        [Test]
        public void Can_set_attr_then_elem_on_existing_doc() {
            XDoc doc = new XDoc("doc").Start("elem").Attr("with-attr", "abc").Value("xyz").End();
            IBasicSetters x = XObjectBuilder.FromXDoc<IBasicSetters>(doc);
            Assert.AreEqual("abc", x.elem_withAttr);
            Assert.AreEqual("xyz", x.Elem);
            x.elem_withAttr = "baz";
            Assert.AreEqual("baz", x.elem_withAttr);
            Assert.AreEqual("xyz", x.Elem);
            x.Elem = "bar";
            Assert.AreEqual("baz", x.elem_withAttr);
            Assert.AreEqual("bar", x.Elem);
        }

        [Test]
        public void Can_set_elem_then_attr_on_new_object() {
            IBasicSetters x = XObjectBuilder.New<IBasicSetters>();
            Assert.AreEqual(null, x.elem_withAttr);
            Assert.AreEqual(null, x.Elem);
            x.Elem = "bar";
            Assert.AreEqual(null, x.elem_withAttr);
            Assert.AreEqual("bar", x.Elem);
            x.elem_withAttr = "baz";
            Assert.AreEqual("baz", x.elem_withAttr);
            Assert.AreEqual("bar", x.Elem);
            Assert.AreEqual("<basic-setters><elem with-attr=\"baz\">bar</elem></basic-setters>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_attr_then_elem_on_new_object() {
            IBasicSetters x = XObjectBuilder.New<IBasicSetters>();
            Assert.AreEqual(null, x.elem_withAttr);
            Assert.AreEqual(null, x.Elem);
            x.elem_withAttr = "baz";
            Assert.AreEqual("baz", x.elem_withAttr);
            Assert.AreEqual(null, x.Elem);
            Assert.AreEqual("<basic-setters><elem with-attr=\"baz\" /></basic-setters>", x.AsDocument.ToString());
            x.Elem = "bar";
            Assert.AreEqual("baz", x.elem_withAttr);
            Assert.AreEqual("bar", x.Elem);
            Assert.AreEqual("<basic-setters><elem with-attr=\"baz\">bar</elem></basic-setters>", x.AsDocument.ToString());
        }

    }
}
