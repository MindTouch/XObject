using System;
using MindTouch.Dream.XObject.TypeConverter;
using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {

    [TestFixture]
    public class CustomConverterTests {
        public interface INotEmptyConverterTest : IXObject {
            [XObjectPath("permissions[permission='read']")]
            [XObjectTypeConverter(typeof(XObjectNotEmptyConverter))]
            bool CanRead { get; }
            [XObjectPath("permissions[permission='write']")]
            [XObjectTypeConverter(typeof(XObjectNotEmptyConverter))]
            bool CanWrite { get; }
        }

        [Test]
        public void NotEmptyConverter_checks_for_node_existence() {
            XDoc doc = new XDoc("doc")
                .Start("permissions")
                    .Elem("permission", "read")
                .End();
            INotEmptyConverterTest x = XObjectBuilder.FromXDoc<INotEmptyConverterTest>(doc);
            Assert.IsTrue(x.CanRead);
            Assert.IsFalse(x.CanWrite);
        }

        [Flags]
        public enum Permission {
            Read,
            Write,
            Execute
        }

        public interface IPermissionConverterTest : IXObject {

            [XObjectPath("permission")]
            Permission Permission { get; }
        }

        [Test]
        public void Enums_automatically_get_XObjectEnumConverter() {
            XDoc doc = new XDoc("doc").Elem("permission", "execute");
            IPermissionConverterTest x = XObjectBuilder.FromXDoc<IPermissionConverterTest>(doc);
            Assert.AreEqual(Permission.Execute, x.Permission);
        }

        [Test]
        public void Enums_automatically_get_XObjectEnumConverter_and_use_default_if_node_is_empty() {
            XDoc doc = new XDoc("doc");
            IPermissionConverterTest x = XObjectBuilder.FromXDoc<IPermissionConverterTest>(doc);
            Assert.AreEqual(Permission.Read, x.Permission);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Enums_automatically_get_XObjectEnumConverter_and_throws_on_bad_value() {
            XDoc doc = new XDoc("doc").Elem("permission", "foo");
            IPermissionConverterTest x = XObjectBuilder.FromXDoc<IPermissionConverterTest>(doc);
#pragma warning disable 168
            Permission y = x.Permission;
#pragma warning restore 168
        }

        public interface INullablePermissionConverterTest : IXObject {

            [XObjectPath("permission")]
            Permission? Permission { get; }
        }

        [Test]
        public void Nullable_enums_automatically_get_XObjectEnumConverter() {
            XDoc doc = new XDoc("doc").Elem("permission", "execute");
            INullablePermissionConverterTest x = XObjectBuilder.FromXDoc<INullablePermissionConverterTest>(doc);
            Assert.IsTrue(x.Permission.HasValue);
            Assert.AreEqual(Permission.Execute, x.Permission.Value);
        }

        [Test]
        public void Nullable_enums_automatically_get_XObjectEnumConverter_and_return_null_if_node_is_empty() {
            XDoc doc = new XDoc("doc");
            INullablePermissionConverterTest x = XObjectBuilder.FromXDoc<INullablePermissionConverterTest>(doc);
            Assert.IsFalse(x.Permission.HasValue);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Nullable_enums_automatically_get_XObjectEnumConverter_and_throws_on_bad_value() {
            XDoc doc = new XDoc("doc").Elem("permission", "foo");
            INullablePermissionConverterTest x = XObjectBuilder.FromXDoc<INullablePermissionConverterTest>(doc);
#pragma warning disable 168
            Permission? y = x.Permission;
#pragma warning restore 168
        }

        public class PermissionsConverter : IXObjectTypeConverter {
            public bool CanConvert(Type t) {
                return typeof(Permission).IsAssignableFrom(t);
            }

            public object Convert(XDoc doc) {
                Permission p = Permission.Read;
                bool first = true;
                foreach(XDoc perm in doc) {
                    Permission p1 = (Permission)Enum.Parse(typeof(Permission), perm.AsText, true);
                    if(first) {
                        p = p1;
                    } else {
                        p |= p1;
                    }
                    first = false;
                }
                return p;
            }

            public void ConvertBack(XDoc doc, object value) {
                throw new NotImplementedException();
            }

            public bool CanConvertBack { get { return false; } }

        }

        public interface IPermissionsConverterTest : IXObject {
            [XObjectPath("permissions/permission")]
            [XObjectTypeConverter(typeof(PermissionsConverter))]
            Permission Permissions { get; }
        }

        [Test]
        public void Custom_converter_via_accessor_attribute_gets_invoked() {
            XDoc doc = new XDoc("doc")
                .Start("permissions")
                    .Elem("permission", "read")
                    .Elem("permission", "execute")
                .End();
            IPermissionsConverterTest x = XObjectBuilder.FromXDoc<IPermissionsConverterTest>(doc);
            Assert.AreEqual(Permission.Read | Permission.Execute, x.Permissions);
        }

        public class CannnotConvertConverter : IXObjectTypeConverter {
            public bool CanConvert(Type t) {
                return false;
            }

            public object Convert(XDoc doc) {
                throw new System.NotImplementedException();
            }

            public bool CanConvertBack {
                get { return false; }
            }

            public void ConvertBack(XDoc doc, object value) {
                throw new System.NotImplementedException();
            }
        }

        public interface IHasAccessorWithInappropriateConverter : IXObject {
            [XObjectTypeConverter(typeof(CannnotConvertConverter))]
            int Foo { get; }
        }

        [Test]
        public void Converter_on_accessor_that_cannot_convert_the_type_throws_at_XObjectBuilder_creation() {
            try {
                XObjectBuilder.New<IHasAccessorWithInappropriateConverter>();
            } catch(XObjectContractException e) {
                Assert.AreEqual("Specified converter 'MindTouch.Dream.XObject.Tests.CustomConverterTests+CannnotConvertConverter' on accessor 'Foo' cannot convert type 'System.Int32'", e.Message);
                return;
            }
            Assert.Fail("didn't get exception");
        }

        public class NotAConverter { }

        [XObjectTypeConverter(typeof(NotAConverter))]
        public interface IHasInvalidMarkupOnInterface : IXObject {
            int Foo { get; }
        }

        public interface IHasInvalidMarkupOnAccessor : IXObject {
            [XObjectTypeConverter(typeof(NotAConverter))]
            int Foo { get; }
        }

        [Test]
        public void Providing_bad_converter_type_on_interface_throws_at_XObjectBuilder_creation() {
            try {
                XObjectBuilder.New<IHasInvalidMarkupOnInterface>();
            } catch(XObjectContractException e) {
                Assert.AreEqual("The converter 'MindTouch.Dream.XObject.Tests.CustomConverterTests+NotAConverter' must be a assignable from type IXObjectTypeConverter", e.Message);
                return;
            }
            Assert.Fail("didn't get exception");
        }

        [Test]
        public void Providing_bad_converter_type_on_accessor_throws_at_XObjectBuilder_creation() {
            try {
                XObjectBuilder.New<IHasInvalidMarkupOnAccessor>();
            } catch(XObjectContractException e) {
                Assert.AreEqual("The converter 'MindTouch.Dream.XObject.Tests.CustomConverterTests+NotAConverter' must be a assignable from type IXObjectTypeConverter", e.Message);
                return;
            }
            Assert.Fail("didn't get exception");
        }

        public class CustomIntConverter : IXObjectTypeConverter {
            public static int ConvertCalled;

            public bool CanConvert(Type t) {
                return typeof(int).IsAssignableFrom(t);
            }

            public object Convert(XDoc doc) {
                ConvertCalled++;
                return doc.AsInt ?? 0;
            }

            public bool CanConvertBack {
                get { return false; }
            }

            public void ConvertBack(XDoc doc, object value) {
                throw new System.NotImplementedException();
            }
        }

        [XObjectTypeConverter(typeof(CustomIntConverter))]
        public interface IUsesConverterMarkupOnInterface : IXObject {
            int Foo { get; }
        }

        [Test]
        public void Custom_converter_on_interface_gets_attached_to_accessor_of_handled_type() {
            CustomIntConverter.ConvertCalled = 0;
            IUsesConverterMarkupOnInterface x = XObjectBuilder.New<IUsesConverterMarkupOnInterface>();
            Assert.AreEqual(0, x.Foo);
            Assert.AreEqual(1, CustomIntConverter.ConvertCalled);
        }

        public interface IUsesConverterFromRepository : IXObject {
            int Foo { get; }
        }

        [Test]
        public void Custom_converter_from_repository_gets_attached_to_accessor_of_handled_type() {
            XObjectBuilderRepository repository = new XObjectBuilderRepository();
            repository.RegisterConverter(new CustomIntConverter());
            CustomIntConverter.ConvertCalled = 0;
            IUsesConverterFromRepository x = repository.GetBuilder<IUsesConverterFromRepository>().New();
            Assert.AreEqual(0, x.Foo);
            Assert.AreEqual(1, CustomIntConverter.ConvertCalled);
        }
    }
}
