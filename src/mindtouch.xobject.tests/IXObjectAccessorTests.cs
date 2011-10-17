using System.Collections.Generic;
using MindTouch.Xml;
using NUnit.Framework;

namespace MindTouch.Dream.XObject.Tests {
    
    [TestFixture]
    public class IXObjectAccessorTests {

        public interface ISuperTest : IXObject {

            [XObjectPath("sub/sub")]
            ISubTest Sub { get; set; }
        }

        public interface ISuperTest2 : IXObject {

            [XObjectPath("sub/sub")]
            IList<ISubTest> Sub { get; }
        }

        public interface ISubTest : IXObject {
            string Info { get; }
        }

        [Test]
        public void IXObjects_can_be_accessor_types() {
            XDoc doc = new XDoc("super").Start("sub").Start("sub").Elem("info", "bar").EndAll();
            ISuperTest super = XObjectBuilder.FromXDoc<ISuperTest>(doc);
            Assert.IsNotNull(super.Sub);
            Assert.AreEqual("bar", super.Sub.Info);
        }

        [Test]
        public void Can_set_new_IXObject() {
            ISuperTest x = XObjectBuilder.New<ISuperTest>();
            x.Sub = XObjectBuilder.New<ISubTest>();
            Assert.AreEqual("<super-test><sub><sub /></sub></super-test>", x.AsDocument.ToString());
        }

        [Test]
        public void Can_set_existing_IXObject() {
            ISuperTest x = XObjectBuilder.New<ISuperTest>();
            x.Sub = XObjectBuilder.FromXDoc<ISubTest>(new XDoc("sub").Elem("info", "bar"));
            Assert.AreEqual("<super-test><sub><sub><info>bar</info></sub></sub></super-test>", x.AsDocument.ToString());
        }

        [Test]
        public void IXObjects_can_be_accessor_collections() {
            XDoc doc = new XDoc("super")
                .Start("sub")
                    .Start("sub").Elem("info", "foo").End()
                    .Start("sub").Elem("info", "bar").End()
                .End();
            ISuperTest2 super = XObjectBuilder.FromXDoc<ISuperTest2>(doc);
            Assert.AreEqual(2, super.Sub.Count);
            Assert.AreEqual("foo", super.Sub[0].Info);
            Assert.AreEqual("bar", super.Sub[1].Info);
        }

        [Test]
        public void IXObjects_can_be_accessor_types_and_still_work_if_there_is_no_doc_at_the_xpath() {
            XDoc doc = new XDoc("super");
            ISuperTest super = XObjectBuilder.FromXDoc<ISuperTest>(doc);
            Assert.IsNotNull(super.Sub);
            Assert.IsNull(super.Sub.Info);
        }

    }
}
